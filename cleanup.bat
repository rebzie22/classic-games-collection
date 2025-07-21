@echo off
echo Cleaning up unused files...

REM Remove duplicate GameDiscoveryService files
del "src\GameLauncher\Services\GameDiscoveryServiceClean.cs" 2>nul
del "src\GameLauncher\Services\GameDiscoveryServiceFixed.cs" 2>nul
del "src\GameLauncher\Services\GameDiscoveryService_New.cs" 2>nul

REM Remove temporary development files
del "debug-build.bat" 2>nul
del "fix-and-run.bat" 2>nul
del "MINESWEEPER_INTEGRATION.md" 2>nul

REM Remove any null files
del "$null" 2>nul

echo Cleanup complete!
pause
