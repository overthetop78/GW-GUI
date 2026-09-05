@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0publish-wiki.ps1"
exit /b %errorlevel%
