namespace GameCore.Models
{
    /// <summary>
    /// Represents the current state of a game
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Game is not initialized
        /// </summary>
        NotInitialized,
        
        /// <summary>
        /// Game is ready to start
        /// </summary>
        Ready,
        
        /// <summary>
        /// Game is currently running
        /// </summary>
        Running,
        
        /// <summary>
        /// Game is paused
        /// </summary>
        Paused,
        
        /// <summary>
        /// Game has ended - player won
        /// </summary>
        Won,
        
        /// <summary>
        /// Game has ended - player lost
        /// </summary>
        Lost,
        
        /// <summary>
        /// Game was stopped by user
        /// </summary>
        Stopped
    }
    
    /// <summary>
    /// Event arguments for game state changes
    /// </summary>
    public class GameStateChangedEventArgs : EventArgs
    {
        public GameState PreviousState { get; }
        public GameState NewState { get; }
        public DateTime Timestamp { get; }
        
        public GameStateChangedEventArgs(GameState previousState, GameState newState)
        {
            PreviousState = previousState;
            NewState = newState;
            Timestamp = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Event arguments for score changes
    /// </summary>
    public class ScoreChangedEventArgs : EventArgs
    {
        public long PreviousScore { get; }
        public long NewScore { get; }
        public long ScoreDelta { get; }
        public DateTime Timestamp { get; }
        
        public ScoreChangedEventArgs(long previousScore, long newScore)
        {
            PreviousScore = previousScore;
            NewScore = newScore;
            ScoreDelta = newScore - previousScore;
            Timestamp = DateTime.Now;
        }
    }
}
