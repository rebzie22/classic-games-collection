using System;

namespace GameCore.Models
{
    public class PlayerProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastPlayedAt { get; set; } = DateTime.UtcNow;
        public int TotalGamesPlayed { get; set; }
        public Dictionary<string, PlayerGameStats> GameStats { get; set; } = new Dictionary<string, PlayerGameStats>();
    }
    
    public class PlayerGameStats
    {
        public string GameId { get; set; } = string.Empty;
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }
        public double BestScore { get; set; }
        public double? BestTime { get; set; }
        public double AverageScore { get; set; }
        public double? AverageTime { get; set; }
        public DateTime FirstPlayedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastPlayedAt { get; set; } = DateTime.UtcNow;
        
        public double WinRate => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed * 100 : 0;
    }
}
