@echo off
echo.
echo ===============================================
echo   Classic Games Collection - Debug Build
echo ===============================================
echo.

echo Building Classic Games Collection...
dotnet build ClassicGamesCollection.sln --verbosity normal

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ Build successful! 
    echo.
    echo Checking output directory contents...
    dir src\GameLauncher\bin\Debug\net8.0-windows\*.dll
    echo.
    echo Starting the application with console output...
    echo.
    dotnet run --project src\GameLauncher
) else (
    echo.
    echo ❌ Build failed. Please check the error messages above.
    pause
)
