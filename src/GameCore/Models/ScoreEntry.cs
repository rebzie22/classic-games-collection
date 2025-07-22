using System;

namespace GameCore.Models
{
    public class ScoreEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string GameId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public double Score { get; set; }
        public double? Time { get; set; } // Time in seconds (for time-based games like Minesweeper)
        public string Difficulty { get; set; } = string.Empty;
        public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        // Calculated property for display
        public string TimeFormatted => Time.HasValue ? Time.Value.ToString("0.##") + " s" : "";
        
        // Calculated property for sorting (lower time = better for Minesweeper, higher score = better for others)
        public double SortValue => GameId == "minesweeper" ? -(Time ?? double.MaxValue) : Score;
    }
}
