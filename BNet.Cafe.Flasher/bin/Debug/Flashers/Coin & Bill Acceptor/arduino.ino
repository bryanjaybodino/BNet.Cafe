#include <Keyboard.h>

#if !defined(ARDUINO_ARCH_RP2040)
  #error "This sketch is for Raspberry Pi Pico (RP2040). Select the Pico board."
#endif

// Pico GPIO is 3.3V ONLY.
// ---- Coin acceptor: GP4 = pin 1, GP5 = pin 2 ----
const int coinPin    = 1;
const int inhibitPin = 2;
const bool INHIBIT_ACTIVE_LOW = true;

// ---- Bill acceptor: GP1 = pin 4, GP2 = pin 5 ----
const int billPin         = 4;
const int billInhibitPin  = 5;
const bool BILL_INHIBIT_ACTIVE_LOW = true;

// ---- Settings ----
const bool DEBUG = true;

// Coin
const unsigned long coinTimeoutMs = 50;       // ms gap = coin finished
const unsigned long minPulseUs    = 10000UL;  // reject pulses < 10ms
const unsigned long minGapMs      = 10;       // min gap between accepted pulses
const unsigned long settleMs      = 500;      // ignore after lock/unlock

// Bill (bill pulses are usually ~50ms wide with ~50-100ms gaps)
const unsigned long billTimeoutMs  = 300;     // ms gap = bill finished
const unsigned long billMinPulseUs = 20000UL; // reject pulses < 20ms

// ---- Coin shared with ISR ----
volatile int pulseCount = 0;
volatile unsigned long lastPulseTime = 0;
volatile unsigned long ignoreUntil = 0;
volatile unsigned long fallUs = 0;
volatile bool pulseActive = false;
volatile bool lockedISR = true;

// ---- Bill shared with ISR ----
volatile int billPulseCount = 0;
volatile unsigned long billLastPulseTime = 0;
volatile unsigned long billIgnoreUntil = 0;
volatile unsigned long billFallUs = 0;
volatile bool billPulseActive = false;

bool locked = true;

// ---- Event log ring buffer ----
enum EvType : uint8_t {
  EV_FALL, EV_ACCEPT, EV_REJ_SHORT, EV_REJ_LOCKED, EV_REJ_SETTLE, EV_REJ_GAP,
  EV_BILL_FALL, EV_BILL_ACCEPT, EV_BILL_REJ_SHORT, EV_BILL_REJ_LOCKED, EV_BILL_REJ_SETTLE
};
struct Event { uint8_t type; unsigned long ms; unsigned long widthUs; };
const uint8_t LOG_SIZE = 32;
volatile Event logBuf[LOG_SIZE];
volatile uint8_t logHead = 0, logTail = 0;

void logEvent(uint8_t type, unsigned long widthUs) {
  uint8_t next = (logHead + 1) % LOG_SIZE;
  if (next == logTail) return;
  logBuf[logHead].type = type;
  logBuf[logHead].ms = millis();
  logBuf[logHead].widthUs = widthUs;
  logHead = next;
}

void flushLog() {
  while (logTail != logHead) {
    Event e;
    e.type = logBuf[logTail].type;
    e.ms = logBuf[logTail].ms;
    e.widthUs = logBuf[logTail].widthUs;
    logTail = (logTail + 1) % LOG_SIZE;

    Serial.print(F("LOG "));
    Serial.print(e.ms);
    Serial.print(F("ms "));
    switch (e.type) {
      case EV_FALL:        Serial.println(F("COIN pin went LOW")); break;
      case EV_ACCEPT:      Serial.print(F("COIN ACCEPTED pulse, width="));
                           Serial.print(e.widthUs); Serial.println(F("us")); break;
      case EV_REJ_SHORT:   Serial.print(F("COIN REJECTED too short (noise/spark), width="));
                           Serial.print(e.widthUs); Serial.println(F("us")); break;
      case EV_REJ_LOCKED:  Serial.print(F("COIN rejected: locked, width="));
                           Serial.print(e.widthUs); Serial.println(F("us")); break;
      case EV_REJ_SETTLE:  Serial.print(F("COIN rejected: settle window, width="));
                           Serial.print(e.widthUs); Serial.println(F("us")); break;
      case EV_REJ_GAP:     Serial.print(F("COIN rejected: gap too small, width="));
                           Serial.print(e.widthUs); Serial.println(F("us")); break;

      case EV_BILL_FALL:        Serial.println(F("BILL pin went LOW")); break;
      case EV_BILL_ACCEPT:      Serial.print(F("BILL ACCEPTED pulse, width="));
                                Serial.print(e.widthUs); Serial.println(F("us")); break;
      case EV_BILL_REJ_SHORT:   Serial.print(F("BILL REJECTED too short (noise), width="));
                                Serial.print(e.widthUs); Serial.println(F("us")); break;
      case EV_BILL_REJ_LOCKED:  Serial.print(F("BILL rejected: locked, width="));
                                Serial.print(e.widthUs); Serial.println(F("us")); break;
      case EV_BILL_REJ_SETTLE:  Serial.print(F("BILL rejected: settle window, width="));
                                Serial.print(e.widthUs); Serial.println(F("us")); break;
    }
  }
}

void applyLockState() {
  // Coin inhibit
  bool pinHigh = INHIBIT_ACTIVE_LOW ? !locked : locked;
  if (pinHigh) {
    pinMode(inhibitPin, INPUT);
  } else {
    pinMode(inhibitPin, OUTPUT);
    digitalWrite(inhibitPin, LOW);
  }

  // Bill inhibit
  bool billPinHigh = BILL_INHIBIT_ACTIVE_LOW ? !locked : locked;
  if (billPinHigh) {
    pinMode(billInhibitPin, INPUT);
  } else {
    pinMode(billInhibitPin, OUTPUT);
    digitalWrite(billInhibitPin, LOW);
  }

  noInterrupts();
  pulseCount = 0;
  pulseActive = false;
  billPulseCount = 0;
  billPulseActive = false;
  lockedISR = locked;
  ignoreUntil = millis() + settleMs;
  billIgnoreUntil = millis() + settleMs;
  interrupts();

  if (DEBUG) {
    Serial.print(F("LOG state: "));
    Serial.println(locked ? F("LOCKED") : F("UNLOCKED"));
  }
}

// ---- Coin ISR: fires on both edges. Pulse = LOW period; width checked on RISING. ----
void coinISR() {
  unsigned long nowUs = micros();
  bool level = gpio_get(coinPin);

  if (!level) {                               // FALLING
    fallUs = nowUs;
    pulseActive = true;
    if (DEBUG) logEvent(EV_FALL, 0);
    return;
  }

  // RISING
  if (!pulseActive) return;
  pulseActive = false;
  unsigned long width = nowUs - fallUs;
  unsigned long nowMs = millis();

  if (lockedISR)                  { if (DEBUG) logEvent(EV_REJ_LOCKED, width); return; }
  if (nowMs < ignoreUntil)        { if (DEBUG) logEvent(EV_REJ_SETTLE, width); return; }
  if (width < minPulseUs)         { if (DEBUG) logEvent(EV_REJ_SHORT, width);  return; }
  if (pulseCount > 0 && nowMs - lastPulseTime < minGapMs)
                                  { if (DEBUG) logEvent(EV_REJ_GAP, width);    return; }

  pulseCount++;
  lastPulseTime = nowMs;
  if (DEBUG) logEvent(EV_ACCEPT, width);
}

// ---- Bill ISR: same idea, separate state and pin ----
void billISR() {
  unsigned long nowUs = micros();
  bool level = gpio_get(billPin);

  if (!level) {                               // FALLING
    billFallUs = nowUs;
    billPulseActive = true;
    if (DEBUG) logEvent(EV_BILL_FALL, 0);
    return;
  }

  // RISING
  if (!billPulseActive) return;
  billPulseActive = false;
  unsigned long width = nowUs - billFallUs;
  unsigned long nowMs = millis();

  if (lockedISR)                  { if (DEBUG) logEvent(EV_BILL_REJ_LOCKED, width); return; }
  if (nowMs < billIgnoreUntil)    { if (DEBUG) logEvent(EV_BILL_REJ_SETTLE, width); return; }
  if (width < billMinPulseUs)     { if (DEBUG) logEvent(EV_BILL_REJ_SHORT, width);  return; }

  billPulseCount++;
  billLastPulseTime = nowMs;
  if (DEBUG) logEvent(EV_BILL_ACCEPT, width);
}

int evaluateCoin(int pulses) {
  switch (pulses) {
    case 1: case 5: case 10: case 20: return pulses;
    default: return 0;
  }
}

// Map bill pulse count -> peso value. EDIT THIS to match your acceptor's
// pulse settings (check its DIP switches / manual).
int evaluateBill(int pulses) {
  switch (pulses) {
    case 2:   return 20;
    case 5:   return 50;
    case 10:  return 100;
    case 20:  return 200;
    case 50:  return 500;
    case 100: return 1000;
    default:  return 0;
  }
}

void setup() {
  Serial.begin(115200);
  Keyboard.begin();
  pinMode(coinPin, INPUT_PULLUP);
  pinMode(billPin, INPUT_PULLUP);

  unsigned long t0 = millis();
  while (!Serial && millis() - t0 < 2000) delay(10);

  applyLockState();
  attachInterrupt(digitalPinToInterrupt(coinPin), coinISR, CHANGE);
  attachInterrupt(digitalPinToInterrupt(billPin), billISR, CHANGE);
  if (DEBUG) Serial.println(F("LOG ready"));
}

void loop() {
  while (Serial.available() > 0) {
    char c = Serial.read();
    if (c == 'L' && !locked)      { locked = true;  applyLockState(); }
    else if (c == 'U' && locked)  { locked = false; applyLockState(); }
  }

  if (DEBUG) flushLog();

  // ---- Coin ----
  int count = 0;
  noInterrupts();
  if (pulseCount > 0 && millis() - lastPulseTime > coinTimeoutMs) {
    count = pulseCount;
    pulseCount = 0;
  }
  interrupts();

  if (count > 0) {
    int value = evaluateCoin(count);
    if (DEBUG) {
      Serial.print(F("LOG coin finished: pulses="));
      Serial.print(count);
      Serial.print(F(" value="));
      Serial.println(value);
    }
    if (value > 0 && !locked) {
      Keyboard.print(value);
      Keyboard.write(KEY_RETURN);
    }
  }

  // ---- Bill ----
  int billCount = 0;
  noInterrupts();
  if (billPulseCount > 0 && millis() - billLastPulseTime > billTimeoutMs) {
    billCount = billPulseCount;
    billPulseCount = 0;
  }
  interrupts();

  if (billCount > 0) {
    int billValue = evaluateBill(billCount);
    if (DEBUG) {
      Serial.print(F("LOG bill finished: pulses="));
      Serial.print(billCount);
      Serial.print(F(" value="));
      Serial.println(billValue);
    }
    if (billValue > 0 && !locked) {
      Keyboard.print(billValue);
      Keyboard.write(KEY_RETURN);
    }
  }
}