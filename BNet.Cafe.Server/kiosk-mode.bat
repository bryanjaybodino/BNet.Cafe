@echo off
set "URL=http://localhost:2000/BNet.Cafe.Server/BNetVendoMachine.aspx"

echo Closing existing browser background tasks...
taskkill /F /IM chrome.exe /T >nul 2>&1
taskkill /F /IM msedge.exe /T >nul 2>&1

timeout /t 1 /nobreak >nul

echo Launching True Full Screen Mode...

:: Try Chrome first
if exist "C:\Program Files\Google\Chrome\Application\chrome.exe" (
    start "" "C:\Program Files\Google\Chrome\Application\chrome.exe" --app="%URL%" --start-fullscreen --user-data-dir="%TEMP%\BNetKiosk"
    exit /b
)

if exist "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" (
    start "" "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --app="%URL%" --start-fullscreen --user-data-dir="%TEMP%\BNetKiosk"
    exit /b
)

:: Fallback to Edge
if exist "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" (
    start "" "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" --app="%URL%" --start-fullscreen --user-data-dir="%TEMP%\BNetEdgeKiosk"
    exit /b
)

if exist "C:\Program Files\Microsoft\Edge\Application\msedge.exe" (
    start "" "C:\Program Files\Microsoft\Edge\Application\msedge.exe" --start-fullscreen --app="%URL%" --user-data-dir="%TEMP%\BNetEdgeKiosk"
    exit /b
)

pause