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

        public async Task ClearAllScoresAsync()
        {
            await EnsureDatabaseLoaded();
            
            // Clear all scores
            _database!.Scores.Clear();
            
            // Reset player statistics
            foreach (var player in _database.Players)
            {
                player.GameStats.Clear();
                player.TotalGamesPlayed = 0;
                // Reset LastPlayedAt to CreatedAt (original date when player was first created)
                player.LastPlayedAt = player.CreatedAt;
            }
            
            // Save the cleared database
            await _dataService.SaveDatabaseAsync(_database);
        }

        public async Task ClearGameScoresAsync(string gameId)
        {
            await EnsureDatabaseLoaded();
            
            // Remove all scores for the specified game
            _database!.Scores.RemoveAll(s => s.GameId.Equals(gameId, StringComparison.OrdinalIgnoreCase));
            
            // Update player statistics - remove this game's stats
            foreach (var player in _database.Players)
            {
                if (player.GameStats.ContainsKey(gameId))
                {
                    var gameStats = player.GameStats[gameId];
                    player.TotalGamesPlayed -= gameStats.GamesPlayed;
                    player.GameStats.Remove(gameId);
                }
            }
            
            // Save the updated database
            await _dataService.SaveDatabaseAsync(_database);
        }

        public async Task ClearPlayerScoresAsync(string playerName)
        {
            await EnsureDatabaseLoaded();
            
            // Remove all scores for the specified player
            _database!.Scores.RemoveAll(s => s.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase));
            
            // Remove the player from the database entirely
            var playerToRemove = _database.Players.FirstOrDefault(p => 
                p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));
            
            if (playerToRemove != null)
            {
                _database.Players.Remove(playerToRemove);
            }
            
            // Save the updated database
            await _dataService.SaveDatabaseAsync(_database);
        }

        public async Task DeleteScoreAsync(ScoreEntry score)
        {
            await DeleteScoreAsync(score.GameId, score.PlayerName, score.Score, score.AchievedAt);
        }

        public async Task DeleteScoreAsync(string gameId, string playerName, double score, DateTime achievedAt)
        {
            await EnsureDatabaseLoaded();
            
            // Find and remove the specific score entry
            var scoreToRemove = _database!.Scores.FirstOrDefault(s => 
                s.GameId.Equals(gameId, StringComparison.OrdinalIgnoreCase) &&
                s.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase) &&
                Math.Abs(s.Score - score) < 0.001 && // Handle floating point comparison
                s.AchievedAt == achievedAt);
            
            if (scoreToRemove != null)
            {
                _database.Scores.Remove(scoreToRemove);
                
                // Update player statistics
                var player = _database.Players.FirstOrDefault(p => 
                    p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));
                
                if (player != null && player.GameStats.ContainsKey(gameId))
                {
                    var gameStats = player.GameStats[gameId];
                    gameStats.GamesPlayed = Math.Max(0, gameStats.GamesPlayed - 1);
                    player.TotalGamesPlayed = Math.Max(0, player.TotalGamesPlayed - 1);
                    
                    // Recalculate best score from remaining scores
                    var remainingScores = _database.Scores
                        .Where(s => s.GameId.Equals(gameId, StringComparison.OrdinalIgnoreCase) &&
                                   s.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                        .Select(s => s.Score);
                    
                    gameStats.BestScore = remainingScores.Any() ? remainingScores.Max() : 0;
                    
                    // If no more games played for this game, remove the game stats
                    if (gameStats.GamesPlayed == 0)
                    {
                        player.GameStats.Remove(gameId);
                    }
                    
                    // If player has no more games, remove them entirely
                    if (player.TotalGamesPlayed == 0)
                    {
                        _database.Players.Remove(player);
                    }
                }
                
                // Save the updated database
                await _dataService.SaveDatabaseAsync(_database);
            }
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
