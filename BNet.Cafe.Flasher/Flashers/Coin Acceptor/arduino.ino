#include <Keyboard.h>

const int coinPin    = 3;
const int inhibitPin = 4;
const bool INHIBIT_ACTIVE_LOW = true;

// ---- Settings ----
const bool DEBUG = true;                      // set false to silence logs
const unsigned long timeout       = 250;      // ms gap = coin finished
const unsigned long minPulseUs    = 10000UL;  // reject pulses < 10ms (spark spikes)
const unsigned long minGapMs      = 10;       // min gap between accepted pulses
const unsigned long settleMs      = 500;      // ignore after lock/unlock

// ---- Shared with ISR ----
volatile int pulseCount = 0;
volatile unsigned long lastPulseTime = 0;     // ms, last accepted pulse
volatile unsigned long ignoreUntil = 0;
volatile unsigned long fallUs = 0;
volatile bool pulseActive = false;
volatile bool lockedISR = true;

bool locked = true;

// ---- Event log ring buffer (ISR writes, loop prints) ----
enum EvType : uint8_t { EV_FALL, EV_ACCEPT, EV_REJ_SHORT, EV_REJ_LOCKED, EV_REJ_SETTLE, EV_REJ_GAP };
struct Event { uint8_t type; unsigned long ms; unsigned long widthUs; };
const uint8_t LOG_SIZE = 32;
volatile Event logBuf[LOG_SIZE];
volatile uint8_t logHead = 0, logTail = 0;

void logEvent(uint8_t type, unsigned long widthUs) {
  uint8_t next = (logHead + 1) % LOG_SIZE;
  if (next == logTail) return;                // buffer full, drop
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
      case EV_FALL:        Serial.println(F("pin went LOW")); break;
      case EV_ACCEPT:      Serial.print(F("ACCEPTED pulse, width="));
                           Serial.print(e.widthUs); Serial.println(F("us")); break;
      case EV_REJ_SHORT:   Serial.print(F("REJECTED too short (noise/spark), width="));
                           Serial.print(e.widthUs); Serial.println(F("us")); break;
      case EV_REJ_LOCKED:  Serial.print(F("rejected: locked, width="));
                           Serial.print(e.widthUs); Serial.println(F("us")); break;
      case EV_REJ_SETTLE:  Serial.print(F("rejected: settle window, width="));
                           Serial.print(e.widthUs); Serial.println(F("us")); break;
      case EV_REJ_GAP:     Serial.print(F("rejected: gap too small, width="));
                           Serial.print(e.widthUs); Serial.println(F("us")); break;
    }
  }
}

void applyLockState() {
  bool pinHigh = INHIBIT_ACTIVE_LOW ? !locked : locked;
  if (pinHigh) {
    pinMode(inhibitPin, INPUT);
  } else {
    pinMode(inhibitPin, OUTPUT);
    digitalWrite(inhibitPin, LOW);
  }
  noInterrupts();
  pulseCount = 0;
  pulseActive = false;
  lockedISR = locked;
  ignoreUntil = millis() + settleMs;
  interrupts();

  if (DEBUG) {
    Serial.print(F("LOG state: "));
    Serial.println(locked ? F("LOCKED") : F("UNLOCKED"));
  }
}

// Fires on both edges. Pulse = LOW period; width is checked on the RISING edge.
void coinISR() {
  unsigned long nowUs = micros();
  bool level = digitalRead(coinPin);

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

int evaluateCoin(int pulses) {
  switch (pulses) {
    case 1: case 5: case 10: case 20: return pulses;
    default: return 0;
  }
}

void setup() {
  delay(2000);
  pinMode(coinPin, INPUT_PULLUP);
  Serial.begin(9600);
  Keyboard.begin();
  applyLockState();
  attachInterrupt(digitalPinToInterrupt(coinPin), coinISR, CHANGE);
  if (DEBUG) Serial.println(F("LOG ready"));
}

void loop() {
  while (Serial.available() > 0) {
    char c = Serial.read();
    if (c == 'L' && !locked)      { locked = true;  applyLockState(); }
    else if (c == 'U' && locked)  { locked = false; applyLockState(); }
  }

  if (DEBUG) flushLog();

  int count = 0;
  noInterrupts();
  if (pulseCount > 0 && millis() - lastPulseTime > timeout) {
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
}