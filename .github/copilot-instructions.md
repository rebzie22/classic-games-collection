<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# Classic Games Collection - Copilot Instructions

This is a C# .NET Windows Forms project showcasing advanced software architecture patterns through a classic games collection.

## Project Architecture

### Core Principles
- **Plugin Architecture**: Each game implements the `IGame` interface for modular design
- **Clean Separation**: Core interfaces, base implementations, and game-specific logic are well separated
- **Modern C# Practices**: Nullable reference types, async/await, proper disposal patterns
- **Enterprise Patterns**: Dependency injection ready, plugin discovery, event-driven architecture

### Key Components
1. **GameCore Library**: Contains interfaces, base classes, and shared models
2. **GameLauncher**: Main application with plugin discovery and game management
3. **Individual Games**: Each game is a separate project implementing IGame interface

### Code Standards
- Use modern C# language features (records, pattern matching, etc.)
- Follow Microsoft naming conventions
- Implement proper resource disposal
- Use events for loose coupling between components
- Include comprehensive error handling
- Write defensive code with null checks

### Game Implementation Guidelines
- All games must implement `IGame` interface
- Use `BaseGameStatistics` for common statistics functionality  
- Implement proper state management with `GameState` enum
- Support multiple difficulty levels
- Provide persistent settings and statistics
- Handle window lifecycle properly

### UI/UX Principles
- Modern flat design with consistent color scheme
- Responsive layouts that work on different screen sizes
- Accessibility considerations (keyboard navigation, high contrast)
- Professional appearance suitable for portfolio demonstration

### Testing Considerations
- Write unit tests for game logic
- Integration tests for plugin discovery
- UI automation tests for critical user flows
- Performance tests for game rendering loops

When suggesting code improvements or new features, prioritize:
1. Maintainability and clean architecture
2. Performance and memory efficiency  
3. User experience and polish
4. Portfolio-worthy code quality
5. Documentation and code clarity
