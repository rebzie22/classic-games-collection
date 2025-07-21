@echo off
echo Cleaning up corrupted files and fixing the project...

cd src\GameLauncher\Services

echo Removing corrupted files...
if exist GameDiscoveryService.cs del GameDiscoveryService.cs
if exist GameDiscoveryServiceFixed.cs del GameDiscoveryServiceFixed.cs
if exist GameDiscoveryService_New.cs del GameDiscoveryService_New.cs

echo Renaming clean file...
ren GameDiscoveryServiceClean.cs GameDiscoveryService.cs

echo File cleanup complete!
cd ..\..\..

echo Cleaning up old Minesweeper placeholder files...
cd src\Games\Minesweeper
if exist MinesweeperGame.cs del MinesweeperGame.cs
cd ..\..\..

echo Stopping any running instances...
taskkill /F /IM "GameLauncher.exe" 2>nul
taskkill /F /IM "Classic Games Collection.exe" 2>nul
timeout /t 2 /nobreak >nul

echo Building the project...
dotnet build ClassicGamesCollection.sln

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ Build successful! Starting the application...
    echo.
    dotnet run --project src\GameLauncher
) else (
    echo.
    echo ❌ Build failed.
    pause
)
