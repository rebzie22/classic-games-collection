using System.ComponentModel;
using GameCore.Models;

namespace GameCore.Interfaces
{
    /// <summary>
    /// Base interface that all games must implement for integration with the game launcher
    /// </summary>
    public interface IGame
    {
        /// <summary>
        /// Unique identifier for the game
        /// </summary>
        string GameId { get; }
        
        /// <summary>
        /// Display name of the game
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Brief description of the game
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Game version
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// Game icon (can be null)
        /// </summary>
        Image? Icon { get; }
        
        /// <summary>
        /// Game difficulty levels available
        /// </summary>
        IReadOnlyList<string> DifficultyLevels { get; }
        
        /// <summary>
        /// Current game state
        /// </summary>
        GameState State { get; }
        
        /// <summary>
        /// Game statistics and scoring
        /// </summary>
        IGameStatistics Statistics { get; }
        
        /// <summary>
        /// Event raised when game state changes
        /// </summary>
        event EventHandler<GameStateChangedEventArgs>? StateChanged;
        
        /// <summary>
        /// Event raised when score changes
        /// </summary>
        event EventHandler<ScoreChangedEventArgs>? ScoreChanged;
        
        /// <summary>
        /// Initialize the game with specified settings
        /// </summary>
        void Initialize(GameSettings settings);
        
        /// <summary>
        /// Start a new game
        /// </summary>
        void StartNew();
        
        /// <summary>
        /// Start a new game with specific difficulty
        /// </summary>
        void StartNew(string difficulty);
        
        /// <summary>
        /// Pause the current game
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Resume the paused game
        /// </summary>
        void Resume();
        
        /// <summary>
        /// Stop the current game
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Get the main game window/form
        /// </summary>
        Form GetGameWindow();
        
        /// <summary>
        /// Cleanup resources when game is being unloaded
        /// </summary>
        void Dispose();
    }
}
