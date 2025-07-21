@echo off
echo.
echo ===============================================
echo   Classic Games Collection - Build and Run
echo ===============================================
echo.

echo Building Classic Games Collection...
dotnet build ClassicGamesCollection.sln --verbosity minimal

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ Build successful! 
    echo.
    echo Games should now be visible in the launcher!
    echo Starting the application...
    echo.
    start "" dotnet run --project src\GameLauncher
    echo.
    echo Application started! Check the new window.
) else (
    echo.
    echo ❌ Build failed. Please check the error messages above.
    echo.
    echo Common fixes:
    echo - Ensure .NET 8.0 SDK is installed
    echo - Try running: dotnet clean
    echo - Check that all project references are correct
    echo.
    pause
)
