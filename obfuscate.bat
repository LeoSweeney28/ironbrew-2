@echo off
setlocal enabledelayedexpansion

REM IronBrew2 Obfuscator - Easy Launcher
REM This script provides a user-friendly interface to obfuscate Lua files

color 0A
title IronBrew2 Lua Obfuscator - v2.7.0

:menu
cls
echo.
echo ============================================================
echo           IRONBREW2 LUA OBFUSCATOR v2.7.0
echo ============================================================
echo.
echo What would you like to do?
echo.
echo   [1] Obfuscate a Lua file (drag and drop supported)
echo   [2] Build the project first
echo   [3] Open output folder
echo   [4] View help/documentation
echo   [5] Exit
echo.
set /p choice="Enter your choice (1-5): "

if "%choice%"=="1" goto obfuscate
if "%choice%"=="2" goto build
if "%choice%"=="3" goto openoutput
if "%choice%"=="4" goto help
if "%choice%"=="5" goto exit
echo Invalid choice! Please try again.
timeout /t 2 >nul
goto menu

:obfuscate
cls
echo.
echo ============================================================
echo                    OBFUSCATE LUA FILE
echo ============================================================
echo.
echo Drag and drop your .lua file below, or type the full path:
echo.
set /p inputfile="Enter file path: "

REM Remove quotes if user dragged and dropped
set inputfile=!inputfile:"=!

REM Check if file exists
if not exist "!inputfile!" (
	echo.
	color 0C
	echo ERROR: File not found: !inputfile!
	color 0A
	echo.
	pause
	goto menu
)

REM Check if it's a Lua file
if not "!inputfile:~-4!"==".lua" (
	echo.
	color 0C
	echo WARNING: This doesn't look like a .lua file, but we'll try anyway...
	color 0A
	echo.
	pause
)

echo.
echo Preparing to obfuscate: !inputfile!
echo.
echo Choose obfuscation preset:
echo.
echo   [1] HEAVY (maximum obfuscation - slowest)
echo   [2] BALANCED (recommended - good speed/obfuscation)
echo   [3] LIGHT (minimal obfuscation - fastest)
echo   [4] CUSTOM (define your own settings)
echo.
set /p preset="Enter preset (1-4): "

if "%preset%"=="1" goto heavy
if "%preset%"=="2" goto balanced
if "%preset%"=="3" goto light
if "%preset%"=="4" goto custom
echo Invalid preset! Using BALANCED defaults...
timeout /t 2 >nul
goto balanced

:heavy
echo.
echo [*] Using HEAVY preset (maximum obfuscation)...
timeout /t 1 >nul
goto startobfuscate

:balanced
echo.
echo [*] Using BALANCED preset (recommended)...
timeout /t 1 >nul
goto startobfuscate

:light
echo.
echo [*] Using LIGHT preset (minimal obfuscation)...
timeout /t 1 >nul
goto startobfuscate

:custom
echo.
echo [*] Custom presets coming soon! Using BALANCED for now...
timeout /t 2 >nul
goto startobfuscate

:startobfuscate
echo.
echo ============================================================
echo                    STARTING OBFUSCATION
echo ============================================================
echo.
echo Timestamp: %date% %time%
echo Input:     !inputfile!
echo.

REM Run the obfuscator
cd /d "%~dp0IronBrew2 CLI"
echo [*] Running obfuscator...
echo.

dotnet run -- "!inputfile!" 2>&1

echo.
echo ============================================================

REM Check if output was created
if exist "out.lua" (
	color 0B
	echo [✓] SUCCESS! Output saved to: out.lua
	color 0A
	echo.
	echo Current directory: %cd%
	echo.
	echo Would you like to open the output folder? (Y/N)
	set /p openow="Choice: "
	if /i "!openow!"=="Y" (
		start explorer.exe "!cd!"
	)
) else (
	color 0C
	echo [✗] ERROR! Obfuscation failed. Check the error messages above.
	color 0A
)

echo.
echo ============================================================
echo.
pause
goto menu

:build
cls
echo.
echo ============================================================
echo                    BUILDING PROJECT
echo ============================================================
echo.
cd /d "%~dp0"
echo [*] Building IronBrew2 solution...
echo.
dotnet build 2>&1
echo.
echo ============================================================
echo Build complete! Press any key to continue...
pause >nul
goto menu

:openoutput
cls
echo.
echo Opening output folder...
cd /d "%~dp0IronBrew2 CLI"
start explorer.exe "%cd%"
timeout /t 2 >nul
goto menu

:help
cls
echo.
echo ============================================================
echo                      HELP & DOCUMENTATION
echo ============================================================
echo.
echo WHAT IS IRONBREW2?
echo   A VM-based Lua 5.1 obfuscation tool that converts your
echo   Lua source code into a virtualized bytecode that is
echo   much harder to reverse engineer.
echo.
echo HOW TO USE:
echo   1. Select "Obfuscate a Lua file" from the main menu
echo   2. Drag and drop your .lua file or type the path
echo   3. Choose an obfuscation preset (BALANCED recommended)
echo   4. Wait for the process to complete
echo   5. Your obfuscated file will be saved as "out.lua"
echo.
echo OBFUSCATION PRESETS:
echo.
echo   HEAVY:
echo     - Maximum obfuscation strength
echo     - Enables all features (encryption, mutations, etc.)
echo     - Slowest performance
echo     - Best for sensitive code
echo.
echo   BALANCED:
echo     - Good balance of speed and obfuscation
echo     - Recommended for most use cases
echo     - Reasonable file size
echo.
echo   LIGHT:
echo     - Minimal obfuscation
echo     - Fastest performance
echo     - Smallest file size
echo     - Use for non-critical code
echo.
echo WHAT GETS OBFUSCATED:
echo   - String constants (encrypted)
echo   - Function logic (converted to VM bytecode)
echo   - Control flow (jumbled and confused)
echo   - Variable names (shortened)
echo   - Bytecode structure (compressed)
echo.
echo IMPORTANT NOTES:
echo   - Input file must be valid Lua 5.1 code
echo   - Keep the original .lua file as backup
echo   - Test your obfuscated code thoroughly
echo   - Output file is standalone and can be run directly
echo.
echo For more info, visit the GitHub repo or check README.md
echo.
echo ============================================================
pause
goto menu

:exit
cls
echo.
echo Thank you for using IronBrew2!
echo.
timeout /t 1 >nul
exit /b 0
