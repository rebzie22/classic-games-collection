using GameCore.Interfaces;
using GameCore.Models;

namespace GameLauncher.Services
{
    public class ScoreService : IScoreService
    {
        private readonly IDataService _dataService;
        private ScoreDatabase? _database;
        
        public event EventHandler<ScoreEntry>? NewHighScore;
        public event EventHandler<ScoreEntry>? ScoreAdded;

        public ScoreService(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<IEnumerable<ScoreEntry>> GetTopScoresAsync(string gameId, int count = 10)
        {
            await EnsureDatabaseLoaded();
            
            return _database!.Scores
                .Where(s => s.GameId == gameId)
                .OrderByDescending(s => s.SortValue)
                .Take(count)
                .ToList();
        }

        public async Task<IEnumerable<ScoreEntry>> GetAllScoresAsync()
        {
            await EnsureDatabaseLoaded();
            
            return _database!.Scores
                .OrderByDescending(s => s.AchievedAt)
                .ToList();
        }

        public async Task<IEnumerable<ScoreEntry>> GetPlayerScoresAsync(string playerName)
        {
            await EnsureDatabaseLoaded();
            
            return _database!.Scores
                .Where(s => s.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.AchievedAt)
                .ToList();
        }

        public async Task AddScoreAsync(ScoreEntry score)
        {
            await EnsureDatabaseLoaded();
            
            // Check if this is a high score
            var isHighScore = await IsHighScoreAsync(score.GameId, score.Score, score.Time);
            
            // Add the score
            _database!.Scores.Add(score);
            
            // Update player statistics
            await UpdatePlayerStatsAsync(score);
            
            // Save database
            await _dataService.SaveDatabaseAsync(_database);
            
            // Raise events
            ScoreAdded?.Invoke(this, score);
            if (isHighScore)
            {
                NewHighScore?.Invoke(this, score);
            }
        }

        public async Task<bool> IsHighScoreAsync(string gameId, double score, double? time = null)
        {
            var topScores = await GetTopScoresAsync(gameId, 10);
            
            if (!topScores.Any())
                return true; // First score is always a high score
                
            var newSortValue = gameId == "minesweeper" ? -(time ?? double.MaxValue) : score;
            return newSortValue > topScores.Last().SortValue || topScores.Count() < 10;
        }

        public async Task<int> GetPlayerRankAsync(string gameId, string playerName)
        {
            await EnsureDatabaseLoaded();
            
            var gameScores = _database!.Scores
                .Where(s => s.GameId == gameId)
                .GroupBy(s => s.PlayerName)
                .Select(g => g.OrderByDescending(s => s.SortValue).First())
                .OrderByDescending(s => s.SortValue)
                .ToList();
                
            var playerScore = gameScores.FirstOrDefault(s => 
                s.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase));
                
            return playerScore != null ? gameScores.IndexOf(playerScore) + 1 : -1;
        }

        private async Task EnsureDatabaseLoaded()
        {
            if (_database == null)
            {
                _database = await _dataService.LoadDatabaseAsync();
            }
        }

        private async Task UpdatePlayerStatsAsync(ScoreEntry score)
        {
            await EnsureDatabaseLoaded();
            
            var player = _database!.Players.FirstOrDefault(p => 
                p.Name.Equals(score.PlayerName, StringComparison.OrdinalIgnoreCase));
                
            if (player == null)
            {
                player = new PlayerProfile
                {
                    Name = score.PlayerName,
                    CreatedAt = DateTime.UtcNow
                };
                _database.Players.Add(player);
            }
            
            player.LastPlayedAt = DateTime.UtcNow;
            player.TotalGamesPlayed++;
            
            // Update game-specific stats
            if (!player.GameStats.ContainsKey(score.GameId))
            {
                player.GameStats[score.GameId] = new PlayerGameStats
                {
                    GameId = score.GameId,
                    FirstPlayedAt = DateTime.UtcNow
                };
            }
            
            var gameStats = player.GameStats[score.GameId];
            gameStats.GamesPlayed++;
            gameStats.LastPlayedAt = DateTime.UtcNow;
            
            // Update best scores
            if (score.GameId == "minesweeper")
            {
                if (score.Time.HasValue && (!gameStats.BestTime.HasValue || score.Time < gameStats.BestTime))
                {
                    gameStats.BestTime = score.Time;
                    gameStats.BestScore = score.Score;
                }
            }
            else
            {
                if (score.Score > gameStats.BestScore)
                {
                    gameStats.BestScore = score.Score;
                    if (score.Time.HasValue)
                        gameStats.BestTime = score.Time;
                }
            }
            
            // Calculate averages
            var playerScores = _database.Scores
                .Where(s => s.GameId == score.GameId && 
                           s.PlayerName.Equals(score.PlayerName, StringComparison.OrdinalIgnoreCase))
                .ToList();
                
            gameStats.AverageScore = playerScores.Average(s => s.Score);
            if (playerScores.Any(s => s.Time.HasValue))
            {
                gameStats.AverageTime = playerScores.Where(s => s.Time.HasValue).Average(s => s.Time!.Value);
            }
        }
    }
}
