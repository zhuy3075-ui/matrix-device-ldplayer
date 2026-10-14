@echo off
if not defined MATRIX_DOTNET_EXE exit /b 2
if not defined MATRIX_CSC_DLL exit /b 2
"%MATRIX_DOTNET_EXE%" exec "%MATRIX_CSC_DLL%" %*
exit /b %errorlevel%
