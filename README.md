# Classic Games Collection

A modern implementation of classic arcade games built with C# and .NET Windows Forms, showcasing advanced software architecture patterns and clean code principles.

## 🎮 Features

### Games Included
- **Minesweeper** - Classic mine detection puzzle with multiple difficulty levels
- **Solitaire** - Klondike Solitaire with full drag-and-drop support and dynamic card scaling
- **Snake** - Traditional snake arcade game *(coming soon)*
- **Tetris** - Falling blocks puzzle game *(coming soon)*

### Architecture Highlights
- **Plugin System** - Modular architecture with dynamic game discovery
- **Clean Interfaces** - Well-defined contracts using IGame interface
- **Statistics & Settings** - Persistent game statistics and user preferences
- **Modern UI** - Clean, responsive Windows Forms interface
- **Professional Code** - Enterprise-level patterns and best practices

## 🏗️ Architecture

### Project Structure
```
ClassicGamesCollection/
├── src/
│   ├── GameCore/              # Shared interfaces and base classes
│   │   ├── Interfaces/        # IGame, IGameStatistics
│   │   ├── Models/           # GameState, Events, Settings
│   │   └── Base/             # Base implementations
│   ├── GameLauncher/         # Main application
│   │   ├── Services/         # Game discovery service
│   │   └── Forms/            # UI components
│   └── Games/                # Individual game implementations
│       ├── Minesweeper/      # Complete minesweeper implementation
│       ├── Solitaire/        # Complete Klondike Solitaire implementation
│       ├── Snake/            # Snake game (placeholder)
│       └── Tetris/           # Tetris game (placeholder)
```

### Key Design Patterns
- **Plugin Architecture** - Games are discovered and loaded dynamically
- **Strategy Pattern** - Different difficulty implementations
- **Observer Pattern** - Event-driven state management
- **Factory Pattern** - Game instance creation
- **Template Method** - Base statistics and settings handling

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Windows operating system
- Visual Studio 2022 (recommended) or VS Code

### Building the Project
```bash
# Clone the repository
git clone https://github.com/rebzie22/classic-games-collection.git
cd classic-games-collection

# Build the solution
dotnet build

# Run the application
dotnet run --project src/GameLauncher
```

### Development Setup
1. Open `ClassicGamesCollection.sln` in Visual Studio
2. Set `GameLauncher` as the startup project
3. Build and run (F5)

## 🎯 Portfolio Highlights

This project demonstrates:

### Technical Skills
- **C# & .NET** - Modern language features and framework usage
- **Windows Forms** - Desktop application development
- **Software Architecture** - Clean, modular, extensible design
- **Design Patterns** - Multiple enterprise patterns implemented
- **Code Quality** - Professional standards with comprehensive error handling

### Software Engineering Practices
- **Modular Design** - Easy to extend with new games
- **Interface Segregation** - Clean contracts and dependencies
- **Single Responsibility** - Each class has a focused purpose
- **Open/Closed Principle** - Open for extension, closed for modification
- **Documentation** - Comprehensive inline and external documentation

### Problem Solving
- **Plugin Discovery** - Dynamic assembly loading and reflection
- **State Management** - Complex game state handling
- **Resource Management** - Proper disposal and memory management
- **User Experience** - Intuitive interface and smooth interactions

## 🔧 Extending the Project

### Adding a New Game
1. Create a new project in `src/Games/YourGame/`
2. Implement the `IGame` interface
3. Extend `BaseGameStatistics` for game-specific statistics
4. Add project reference to `GameLauncher`
5. The plugin system will automatically discover your game

### Example Game Implementation
```csharp
public class YourGame : IGame
{
    public string GameId => "yourgame";
    public string DisplayName => "Your Game";
    public string Description => "Description of your game";
    
    // Implement other IGame members...
}
```

## 📊 Game Statistics

Each game tracks:
- **Score & Best Score** - Current and historical performance
- **Games Played/Won** - Win rate statistics  
- **Time Tracking** - Current game time and best times
- **Persistent Storage** - Statistics saved between sessions

## ⚙️ Configuration

### Game Settings
- Sound enabled/disabled and volume control
- Theme selection for visual customization
- Animation speed adjustment
- Custom key bindings
- Game-specific configuration options

### File Locations
- Settings: `%APPDATA%/ClassicGamesCollection/`
- Statistics: `%APPDATA%/ClassicGamesCollection/Statistics/`

## 🔮 Future Enhancements

- Complete Snake and Tetris implementations
- High score leaderboards
- Online multiplayer support
- Custom themes and skins
- Game replay system
- Achievement system
- Sound effects and music
- Localization support

## 📝 Technical Notes

### Performance Considerations
- Efficient game loop implementation
- Memory-conscious resource management
- Optimized rendering for smooth gameplay

### Code Quality
- Comprehensive error handling and logging
- Unit tests for game logic
- Code documentation and inline comments
- Consistent coding standards throughout

## 🏆 Portfolio Value

This project showcases:
- **Advanced C# Skills** - Modern language features and best practices
- **Architecture Design** - Enterprise-level software design patterns
- **Problem Solving** - Complex technical challenges solved elegantly
- **Code Quality** - Professional, maintainable, well-documented code
- **User Experience** - Polished, intuitive desktop application

Perfect for demonstrating software engineering capabilities to potential employers or clients.

## 📄 License

This project is created for portfolio demonstration purposes.

---

*Built with passion for clean code and great user experiences* ✨
