using System;

namespace GameCore.Models
{
    public class ScoreEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string GameId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public double Score { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        // For display, just use Score
        public string ScoreFormatted => Score.ToString("0.##");
        // For sorting, higher score is always better
        public double SortValue => Score;
    }
}
