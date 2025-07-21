using GameCore.Models;

namespace GameCore.Interfaces
{
    public interface IScoreService
    {
        Task<IEnumerable<ScoreEntry>> GetTopScoresAsync(string gameId, int count = 10);
        Task<IEnumerable<ScoreEntry>> GetAllScoresAsync();
        Task<IEnumerable<ScoreEntry>> GetPlayerScoresAsync(string playerName);
        Task AddScoreAsync(ScoreEntry score);
        Task<bool> IsHighScoreAsync(string gameId, double score, double? time = null);
        Task<int> GetPlayerRankAsync(string gameId, string playerName);
        Task ClearAllScoresAsync();
        Task ClearGameScoresAsync(string gameId);
        Task ClearPlayerScoresAsync(string playerName);
        Task DeleteScoreAsync(ScoreEntry score);
        Task DeleteScoreAsync(string gameId, string playerName, double score, DateTime achievedAt);
        
        // Events
        event EventHandler<ScoreEntry>? NewHighScore;
        event EventHandler<ScoreEntry>? ScoreAdded;
    }
}
