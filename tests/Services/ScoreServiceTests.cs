using Xunit;
using FluentAssertions;
using Moq;
using GameCore.Interfaces;
using GameCore.Models;
using GameLauncher.Services;

namespace ClassicGamesCollection.Tests.Services
{
    public class ScoreServiceTests
    {
        private readonly Mock<IDataService> _mockDataService;
        private readonly ScoreService _scoreService;
        private readonly ScoreDatabase _testDatabase;
        
        public ScoreServiceTests()
        {
            _mockDataService = new Mock<IDataService>();
            _testDatabase = new ScoreDatabase
            {
                Players = new List<PlayerProfile>(),
                Scores = new List<ScoreEntry>()
            };
            
            _mockDataService.Setup(x => x.LoadDatabaseAsync())
                .ReturnsAsync(_testDatabase);
            
            _scoreService = new ScoreService(_mockDataService.Object);
        }
        
        [Fact]
        public async Task GetTopScoresAsync_ShouldReturnScoresInCorrectOrder()
        {
            // Arrange
            _testDatabase.Scores.AddRange(new[]
            {
                new ScoreEntry { GameId = "test", PlayerName = "Player1", Score = 100 },
                new ScoreEntry { GameId = "test", PlayerName = "Player2", Score = 200 },
                new ScoreEntry { GameId = "test", PlayerName = "Player3", Score = 150 },
                new ScoreEntry { GameId = "other", PlayerName = "Player4", Score = 300 }
            });
            
            // Act
            var result = await _scoreService.GetTopScoresAsync("test", 10);
            var scores = result.ToList();
            
            // Assert
            scores.Should().HaveCount(3);
            scores[0].Score.Should().Be(200);
            scores[1].Score.Should().Be(150);
            scores[2].Score.Should().Be(100);
        }
        
        [Fact]
        public async Task AddScoreAsync_ShouldAddScoreAndCreatePlayer()
        {
            // Arrange
            var scoreEntry = new ScoreEntry
            {
                GameId = "test",
                PlayerName = "NewPlayer",
                Score = 500,
                Difficulty = "Normal"
            };
            
            // Act
            await _scoreService.AddScoreAsync(scoreEntry);
            
            // Assert
            _testDatabase.Scores.Should().Contain(scoreEntry);
            _testDatabase.Players.Should().ContainSingle(p => p.Name == "NewPlayer");
            
            var player = _testDatabase.Players.First(p => p.Name == "NewPlayer");
            player.GameStats.Should().ContainKey("test");
            player.GameStats["test"].GamesPlayed.Should().Be(1);
            player.GameStats["test"].BestScore.Should().Be(500);
        }
        
        [Fact]
        public async Task IsHighScoreAsync_ShouldReturnTrueForTopScore()
        {
            // Arrange
            _testDatabase.Scores.AddRange(new[]
            {
                new ScoreEntry { GameId = "test", Score = 100 },
                new ScoreEntry { GameId = "test", Score = 200 },
                new ScoreEntry { GameId = "test", Score = 150 }
            });
            
            // Act
            var isHighScore = await _scoreService.IsHighScoreAsync("test", 250);
            var isNotHighScore = await _scoreService.IsHighScoreAsync("test", 50);
            
            // Assert
            isHighScore.Should().BeTrue();
            isNotHighScore.Should().BeFalse();
        }
        
        [Fact]
        public async Task ClearAllScoresAsync_ShouldClearAllData()
        {
            // Arrange
            _testDatabase.Scores.Add(new ScoreEntry { GameId = "test", Score = 100 });
            _testDatabase.Players.Add(new PlayerProfile { Name = "Player1" });
            
            // Act
            await _scoreService.ClearAllScoresAsync();
            
            // Assert
            _testDatabase.Scores.Should().BeEmpty();
            _testDatabase.Players.Should().ContainSingle();
            _testDatabase.Players.First().GameStats.Should().BeEmpty();
        }
        
        [Fact]
        public async Task ClearGameScoresAsync_ShouldOnlyClearSpecificGame()
        {
            // Arrange
            var player = new PlayerProfile
            {
                Name = "Player1",
                GameStats = new Dictionary<string, PlayerGameStats>
                {
                    { "game1", new PlayerGameStats { GamesPlayed = 5 } },
                    { "game2", new PlayerGameStats { GamesPlayed = 3 } }
                },
                TotalGamesPlayed = 8
            };
            
            _testDatabase.Players.Add(player);
            _testDatabase.Scores.AddRange(new[]
            {
                new ScoreEntry { GameId = "game1", PlayerName = "Player1", Score = 100 },
                new ScoreEntry { GameId = "game2", PlayerName = "Player1", Score = 200 }
            });
            
            // Act
            await _scoreService.ClearGameScoresAsync("game1");
            
            // Assert
            _testDatabase.Scores.Should().ContainSingle(s => s.GameId == "game2");
            player.GameStats.Should().NotContainKey("game1");
            player.GameStats.Should().ContainKey("game2");
            player.TotalGamesPlayed.Should().Be(3);
        }
        
        [Fact]
        public async Task DeleteScoreAsync_ShouldRemoveSpecificScore()
        {
            // Arrange
            var scoreToDelete = new ScoreEntry
            {
                GameId = "test",
                PlayerName = "Player1",
                Score = 100,
                AchievedAt = DateTime.Now
            };
            
            var scoreToKeep = new ScoreEntry
            {
                GameId = "test",
                PlayerName = "Player1",
                Score = 200,
                AchievedAt = DateTime.Now.AddMinutes(-1)
            };
            
            _testDatabase.Scores.AddRange(new[] { scoreToDelete, scoreToKeep });
            
            var player = new PlayerProfile
            {
                Name = "Player1",
                GameStats = new Dictionary<string, PlayerGameStats>
                {
                    { "test", new PlayerGameStats { GamesPlayed = 2, BestScore = 200 } }
                },
                TotalGamesPlayed = 2
            };
            _testDatabase.Players.Add(player);
            
            // Act
            await _scoreService.DeleteScoreAsync(scoreToDelete);
            
            // Assert
            _testDatabase.Scores.Should().ContainSingle();
            _testDatabase.Scores.Should().Contain(scoreToKeep);
            player.GameStats["test"].GamesPlayed.Should().Be(1);
            player.TotalGamesPlayed.Should().Be(1);
        }
    }
}
