# Getting Started with Classic Games Collection

## Quick Start

### Option 1: Using the Batch File (Recommended)
1. Double-click `build-and-run.bat` in the project root
2. The script will build the project and launch the game launcher

### Option 2: Using VS Code Tasks
1. Open the Command Palette (`Ctrl+Shift+P`)
2. Type "Tasks: Run Task"
3. Select "Build Classic Games Collection" first
4. Then run "Run Classic Games Collection"

### Option 3: Using Terminal
```bash
# Build the project
dotnet build ClassicGamesCollection.sln

# Run the application
dotnet run --project src/GameLauncher
```

## Troubleshooting

### PowerShell Execution Policy Issues
If you see execution policy warnings, you can:
1. Use the batch file (recommended)
2. Run PowerShell as Administrator and execute:
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

### Build Errors
If you encounter build errors:
1. Ensure you have .NET 8.0 SDK installed
2. Try cleaning and rebuilding:
   ```bash
   dotnet clean
   dotnet build
   ```

## Project Structure
- `src/GameCore/` - Core interfaces and shared components
- `src/GameLauncher/` - Main application launcher
- `src/Games/Minesweeper/` - Fully implemented Minesweeper game
- `src/Games/Snake/` - Snake game placeholder
- `src/Games/Tetris/` - Tetris game placeholder

## Next Steps
1. Run the application to see the game launcher
2. Play Minesweeper to test the functionality
3. Extend Snake and Tetris games
4. Add your own games by implementing the IGame interface

## Development Tips
- The plugin architecture automatically discovers new games
- Each game should implement `IGame` interface
- Use `BaseGameStatistics` for common functionality
- Settings and statistics are automatically persisted

Enjoy building your classic games collection! 🎮
