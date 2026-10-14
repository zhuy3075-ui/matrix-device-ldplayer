@echo off
chcp 65001 >nul
where pwsh.exe >nul 2>nul
if errorlevel 1 (
    echo 需要 PowerShell 7。请在 PowerShell 7 中运行 Start-MultiVerifiedDesktop.ps1。
    pause
    exit /b 1
)
pwsh.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0Start-MultiVerifiedDesktop.ps1"
if errorlevel 1 pause
