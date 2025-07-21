namespace GameCore.Interfaces
{
    /// <summary>
    /// Interface for game statistics and scoring
    /// </summary>
    public interface IGameStatistics
    {
        /// <summary>
        /// Current score
        /// </summary>
        long Score { get; }
        
        /// <summary>
        /// Best score achieved
        /// </summary>
        long BestScore { get; }
        
        /// <summary>
        /// Total games played
        /// </summary>
        int GamesPlayed { get; }
        
        /// <summary>
        /// Games won
        /// </summary>
        int GamesWon { get; }
        
        /// <summary>
        /// Win percentage
        /// </summary>
        double WinPercentage { get; }
        
        /// <summary>
        /// Total time played
        /// </summary>
        TimeSpan TotalTimePlayed { get; }
        
        /// <summary>
        /// Current game time
        /// </summary>
        TimeSpan CurrentGameTime { get; }
        
        /// <summary>
        /// Best time (for time-based games)
        /// </summary>
        TimeSpan? BestTime { get; }
        
        /// <summary>
        /// Reset statistics
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Save statistics to persistent storage
        /// </summary>
        void Save();
        
        /// <summary>
        /// Load statistics from persistent storage
        /// </summary>
        void Load();
    }
}
