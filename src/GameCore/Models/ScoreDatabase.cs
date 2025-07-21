using System;

namespace GameCore.Models
{
    public class ScoreDatabase
    {
        public string Version { get; set; } = "1.0";
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public List<ScoreEntry> Scores { get; set; } = new List<ScoreEntry>();
        public List<PlayerProfile> Players { get; set; } = new List<PlayerProfile>();
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }
}
