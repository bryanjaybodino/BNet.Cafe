@echo off
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -Command "Start-Process powershell -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File \"%~dp0patcher.ps1\" -TargetDir \"%~dp0\"' -Verb RunAs"