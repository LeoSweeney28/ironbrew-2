@echo off
setlocal enabledelayedexpansion

REM Quick obfuscate - drag and drop a file or pass it as argument
REM Usage: quick-obfuscate.bat yourfile.lua
REM    or: drag and drop file onto this .bat

if "%~1"=="" (
	color 0C
	echo.
	echo ERROR: No file specified!
	echo.
	echo Usage:
	echo   1. Drag and drop a .lua file onto this batch file
	echo   2. Or run: quick-obfuscate.bat yourfile.lua
	echo.
	color 0A
	timeout /t 3 >nul
	exit /b 1
)

set inputfile=%~1
set inputfile=!inputfile:"=!

if not exist "!inputfile!" (
	color 0C
	echo.
	echo ERROR: File not found: !inputfile!
	echo.
	color 0A
	timeout /t 3 >nul
	exit /b 1
)

color 0A
cls
echo.
echo ============================================================
echo           IRONBREW2 - QUICK OBFUSCATE
echo ============================================================
echo.
echo Input file: !inputfile!
echo.
echo Starting obfuscation...
echo.

cd /d "%~dp0IronBrew2 CLI"
dotnet run -- "!inputfile!" 2>&1

echo.
echo ============================================================

if exist "out.lua" (
	color 0B
	echo [SUCCESS] Output saved to: out.lua
	color 0A
	echo Location: %cd%
	echo.
	echo Opening folder in 2 seconds...
	timeout /t 2 >nul
	start explorer.exe "%cd%"
) else (
	color 0C
	echo [FAILED] Obfuscation did not complete successfully.
	color 0A
	echo Please check the error messages above.
	echo.
	pause
)

exit /b 0
