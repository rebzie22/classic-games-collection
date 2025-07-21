using GameCore.Models;

namespace GameCore.Interfaces
{
    public interface IAchievementService
    {
        Task<IEnumerable<Achievement>> GetAllAchievementsAsync();
        Task<IEnumerable<Achievement>> GetPlayerAchievementsAsync(string playerName);
        Task<IEnumerable<Achievement>> GetUnlockedAchievementsAsync(string playerName);
        Task<IEnumerable<Achievement>> GetGameAchievementsAsync(string gameId);
        Task<Achievement?> GetAchievementAsync(string achievementId);
        
        Task<bool> UnlockAchievementAsync(string playerName, string achievementId);
        Task UpdateProgressAsync(string playerName, string achievementId, int progress);
        Task CheckAndUnlockAchievementsAsync(string playerName, string gameId, ScoreEntry scoreEntry);
        
        Task<int> GetPlayerAchievementPointsAsync(string playerName);
        Task<int> GetPlayerAchievementCountAsync(string playerName);
        
        // Events
        event EventHandler<AchievementUnlockedEventArgs>? AchievementUnlocked;
    }
    
    public class AchievementUnlockedEventArgs : EventArgs
    {
        public string PlayerName { get; set; } = string.Empty;
        public Achievement Achievement { get; set; } = new Achievement();
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    }
}
