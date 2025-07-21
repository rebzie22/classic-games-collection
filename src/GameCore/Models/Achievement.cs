using System;

namespace GameCore.Models
{
    public class Achievement
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public int Points { get; set; }
        public AchievementType Type { get; set; }
        public string GameId { get; set; } = string.Empty;
        public bool IsSecret { get; set; } = false;
        public Dictionary<string, object> Criteria { get; set; } = new Dictionary<string, object>();
        
        // Progress tracking
        public bool IsUnlocked { get; set; } = false;
        public DateTime? UnlockedAt { get; set; }
        public int CurrentProgress { get; set; } = 0;
        public int RequiredProgress { get; set; } = 1;
        
        // Calculated properties
        public double ProgressPercentage => RequiredProgress > 0 ? (double)CurrentProgress / RequiredProgress * 100 : 0;
        public bool IsProgressBased => RequiredProgress > 1;
    }
    
    public enum AchievementType
    {
        FirstWin,           // Win your first game
        ScoreBased,         // Reach a certain score
        TimeBased,          // Complete within time limit
        Streak,             // Win multiple games in a row
        Mastery,            // Complete difficult challenges
        Collection,         // Collect/complete multiple items
        Special             // Special achievements (Easter eggs, etc.)
    }
}
