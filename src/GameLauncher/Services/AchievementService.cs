using GameCore.Interfaces;
using GameCore.Models;
using System.Text.Json;

namespace GameLauncher.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly IDataService _dataService;
        private ScoreDatabase? _database;
        private readonly List<Achievement> _defaultAchievements;
        
        public event EventHandler<AchievementUnlockedEventArgs>? AchievementUnlocked;
        
        public AchievementService(IDataService dataService)
        {
            _dataService = dataService;
            _defaultAchievements = CreateDefaultAchievements();
        }
        
        private async Task EnsureDatabaseLoaded()
        {
            _database ??= await _dataService.LoadDatabaseAsync();
        }
        
        public async Task<IEnumerable<Achievement>> GetAllAchievementsAsync()
        {
            await EnsureDatabaseLoaded();
            
            // Return default achievements (can be expanded to include custom ones later)
            return _defaultAchievements;
        }
        
        public async Task<IEnumerable<Achievement>> GetPlayerAchievementsAsync(string playerName)
        {
            await EnsureDatabaseLoaded();
            
            var player = _database?.Players.FirstOrDefault(p => 
                p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));
            
            if (player?.Achievements == null)
                return Enumerable.Empty<Achievement>();
                
            var allAchievements = await GetAllAchievementsAsync();
            
            // Update achievement progress with player data
            foreach (var achievement in allAchievements)
            {
                if (player.Achievements.TryGetValue(achievement.Id, out var playerAchievement))
                {
                    achievement.IsUnlocked = playerAchievement.IsUnlocked;
                    achievement.UnlockedAt = playerAchievement.UnlockedAt;
                    achievement.CurrentProgress = playerAchievement.CurrentProgress;
                }
            }
            
            return allAchievements;
        }
        
        public async Task<IEnumerable<Achievement>> GetUnlockedAchievementsAsync(string playerName)
        {
            var playerAchievements = await GetPlayerAchievementsAsync(playerName);
            return playerAchievements.Where(a => a.IsUnlocked);
        }
        
        public async Task<IEnumerable<Achievement>> GetGameAchievementsAsync(string gameId)
        {
            var allAchievements = await GetAllAchievementsAsync();
            return allAchievements.Where(a => a.GameId.Equals(gameId, StringComparison.OrdinalIgnoreCase));
        }
        
        public async Task<Achievement?> GetAchievementAsync(string achievementId)
        {
            var allAchievements = await GetAllAchievementsAsync();
            return allAchievements.FirstOrDefault(a => a.Id.Equals(achievementId, StringComparison.OrdinalIgnoreCase));
        }
        
        public async Task<bool> UnlockAchievementAsync(string playerName, string achievementId)
        {
            await EnsureDatabaseLoaded();
            
            var player = _database!.Players.FirstOrDefault(p => 
                p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));
            
            if (player == null)
                return false;
                
            player.Achievements ??= new Dictionary<string, Achievement>();
            
            if (!player.Achievements.ContainsKey(achievementId))
            {
                var achievement = await GetAchievementAsync(achievementId);
                if (achievement != null)
                {
                    var playerAchievement = new Achievement
                    {
                        Id = achievement.Id,
                        IsUnlocked = true,
                        UnlockedAt = DateTime.UtcNow,
                        CurrentProgress = achievement.RequiredProgress
                    };
                    
                    player.Achievements[achievementId] = playerAchievement;
                    await _dataService.SaveDatabaseAsync(_database);
                    
                    // Fire event
                    AchievementUnlocked?.Invoke(this, new AchievementUnlockedEventArgs
                    {
                        PlayerName = playerName,
                        Achievement = achievement,
                        UnlockedAt = playerAchievement.UnlockedAt ?? DateTime.UtcNow
                    });
                    
                    return true;
                }
            }
            
            return false;
        }
        
        public async Task UpdateProgressAsync(string playerName, string achievementId, int progress)
        {
            await EnsureDatabaseLoaded();
            
            var player = _database!.Players.FirstOrDefault(p => 
                p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));
            
            if (player == null)
                return;
                
            player.Achievements ??= new Dictionary<string, Achievement>();
            
            var achievement = await GetAchievementAsync(achievementId);
            if (achievement == null)
                return;
                
            if (!player.Achievements.ContainsKey(achievementId))
            {
                player.Achievements[achievementId] = new Achievement
                {
                    Id = achievementId,
                    CurrentProgress = 0
                };
            }
            
            var playerAchievement = player.Achievements[achievementId];
            playerAchievement.CurrentProgress = Math.Max(playerAchievement.CurrentProgress, progress);
            
            // Check if achievement should be unlocked
            if (!playerAchievement.IsUnlocked && playerAchievement.CurrentProgress >= achievement.RequiredProgress)
            {
                await UnlockAchievementAsync(playerName, achievementId);
            }
            else
            {
                await _dataService.SaveDatabaseAsync(_database);
            }
        }
        
        public async Task CheckAndUnlockAchievementsAsync(string playerName, string gameId, ScoreEntry scoreEntry)
        {
            var gameAchievements = await GetGameAchievementsAsync(gameId);
            var playerScores = await GetPlayerGameScores(playerName, gameId);
            
            foreach (var achievement in gameAchievements)
            {
                if (await ShouldUnlockAchievement(achievement, playerName, gameId, scoreEntry, playerScores))
                {
                    await UnlockAchievementAsync(playerName, achievement.Id);
                }
            }
        }
        
        public async Task<int> GetPlayerAchievementPointsAsync(string playerName)
        {
            var unlockedAchievements = await GetUnlockedAchievementsAsync(playerName);
            return unlockedAchievements.Sum(a => a.Points);
        }
        
        public async Task<int> GetPlayerAchievementCountAsync(string playerName)
        {
            var unlockedAchievements = await GetUnlockedAchievementsAsync(playerName);
            return unlockedAchievements.Count();
        }
        
        private async Task<List<ScoreEntry>> GetPlayerGameScores(string playerName, string gameId)
        {
            await EnsureDatabaseLoaded();
            return _database?.Scores
                .Where(s => s.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase) &&
                           s.GameId.Equals(gameId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.AchievedAt)
                .ToList() ?? new List<ScoreEntry>();
        }
        
        private async Task<bool> ShouldUnlockAchievement(Achievement achievement, string playerName, 
            string gameId, ScoreEntry scoreEntry, List<ScoreEntry> playerScores)
        {
            return achievement.Type switch
            {
                AchievementType.FirstWin => playerScores.Count == 1, // First ever score
                AchievementType.ScoreBased => scoreEntry.Score >= GetCriteriaValue<double>(achievement, "targetScore"),
                AchievementType.TimeBased => scoreEntry.Time.HasValue && 
                    scoreEntry.Time.Value <= GetCriteriaValue<double>(achievement, "targetTime"),
                AchievementType.Streak => await CheckStreakAchievement(achievement, playerName, gameId),
                AchievementType.Collection => await CheckCollectionAchievement(achievement, playerName),
                AchievementType.Mastery => await CheckMasteryAchievement(achievement, playerName, gameId),
                _ => false
            };
        }
        
        private T GetCriteriaValue<T>(Achievement achievement, string key)
        {
            if (achievement.Criteria.TryGetValue(key, out var value))
            {
                if (value is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize<T>(jsonElement.GetRawText()) ?? default(T)!;
                }
                if (value is T directValue)
                {
                    return directValue;
                }
            }
            return default(T)!;
        }
        
        private async Task<bool> CheckStreakAchievement(Achievement achievement, string playerName, string gameId)
        {
            // Implementation for streak checking - count consecutive wins
            var recentScores = await GetPlayerGameScores(playerName, gameId);
            var streakRequired = GetCriteriaValue<int>(achievement, "streakLength");
            
            // For simplicity, count recent scores as wins (in real implementation, you'd track wins/losses)
            return recentScores.Take(streakRequired).Count() >= streakRequired;
        }
        
        private async Task<bool> CheckCollectionAchievement(Achievement achievement, string playerName)
        {
            // Check if player has played all games, reached certain total score, etc.
            await EnsureDatabaseLoaded();
            
            var playerScores = _database?.Scores
                .Where(s => s.PlayerName.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<ScoreEntry>();
                
            var uniqueGames = playerScores.Select(s => s.GameId).Distinct().Count();
            var requiredGames = GetCriteriaValue<int>(achievement, "gamesRequired");
            
            return uniqueGames >= requiredGames;
        }
        
        private async Task<bool> CheckMasteryAchievement(Achievement achievement, string playerName, string gameId)
        {
            // Check for high-level achievements like perfect scores, expert difficulty wins, etc.
            var playerScores = await GetPlayerGameScores(playerName, gameId);
            var masteryScore = GetCriteriaValue<double>(achievement, "masteryScore");
            
            return playerScores.Any(s => s.Score >= masteryScore);
        }
        
        private List<Achievement> CreateDefaultAchievements()
        {
            return new List<Achievement>
            {
                // Universal achievements
                new Achievement
                {
                    Id = "first_win",
                    Name = "Getting Started",
                    Description = "Win your first game",
                    Points = 10,
                    Type = AchievementType.FirstWin,
                    GameId = "any"
                },
                new Achievement
                {
                    Id = "collection_master",
                    Name = "Collection Master",
                    Description = "Play all available games",
                    Points = 50,
                    Type = AchievementType.Collection,
                    GameId = "any",
                    Criteria = new Dictionary<string, object> { { "gamesRequired", 4 } }
                },
                
                // Solitaire achievements
                new Achievement
                {
                    Id = "solitaire_first_win",
                    Name = "Patience Pays Off",
                    Description = "Win your first Solitaire game",
                    Points = 15,
                    Type = AchievementType.FirstWin,
                    GameId = "solitaire"
                },
                new Achievement
                {
                    Id = "solitaire_speed_demon",
                    Name = "Speed Demon",
                    Description = "Win a Solitaire game in under 2 minutes",
                    Points = 25,
                    Type = AchievementType.TimeBased,
                    GameId = "solitaire",
                    Criteria = new Dictionary<string, object> { { "targetTime", 120.0 } }
                },
                new Achievement
                {
                    Id = "solitaire_high_score",
                    Name = "Card Master",
                    Description = "Score over 500 points in Solitaire",
                    Points = 30,
                    Type = AchievementType.ScoreBased,
                    GameId = "solitaire",
                    Criteria = new Dictionary<string, object> { { "targetScore", 500.0 } }
                },
                
                // Snake achievements
                new Achievement
                {
                    Id = "snake_first_win",
                    Name = "Slithering Start",
                    Description = "Score your first points in Snake",
                    Points = 15,
                    Type = AchievementType.FirstWin,
                    GameId = "snake"
                },
                new Achievement
                {
                    Id = "snake_high_score",
                    Name = "Python Pro",
                    Description = "Score over 1000 points in Snake",
                    Points = 30,
                    Type = AchievementType.ScoreBased,
                    GameId = "snake",
                    Criteria = new Dictionary<string, object> { { "targetScore", 1000.0 } }
                },
                
                // Tetris achievements
                new Achievement
                {
                    Id = "tetris_first_win",
                    Name = "Block Breaker",
                    Description = "Score your first points in Tetris",
                    Points = 15,
                    Type = AchievementType.FirstWin,
                    GameId = "tetris"
                },
                new Achievement
                {
                    Id = "tetris_high_score",
                    Name = "Tetris Master",
                    Description = "Score over 5000 points in Tetris",
                    Points = 30,
                    Type = AchievementType.ScoreBased,
                    GameId = "tetris",
                    Criteria = new Dictionary<string, object> { { "targetScore", 5000.0 } }
                }
            };
        }
    }
}
