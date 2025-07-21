using GameCore.Interfaces;
using GameCore.Models;

namespace GameCore.Base
{
    /// <summary>
    /// Base implementation of IGameStatistics with common functionality
    /// </summary>
    public class BaseGameStatistics : IGameStatistics
    {
        private readonly string _gameId;
        private long _score;
        private DateTime _gameStartTime;
        private TimeSpan _totalTimePlayed;
        
        public long Score 
        { 
            get => _score;
            protected set
            {
                if (_score != value)
                {
                    var oldScore = _score;
                    _score = value;
                    OnScoreChanged(oldScore, value);
                }
            }
        }
        
        public long BestScore { get; protected set; }
        public int GamesPlayed { get; protected set; }
        public int GamesWon { get; protected set; }
        
        public double WinPercentage => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed * 100 : 0;
        
        public TimeSpan TotalTimePlayed 
        { 
            get => _totalTimePlayed; 
            protected set => _totalTimePlayed = value; 
        }
        
        public TimeSpan CurrentGameTime 
        { 
            get => DateTime.Now - _gameStartTime; 
        }
        
        public TimeSpan? BestTime { get; protected set; }
        
        public event EventHandler<ScoreChangedEventArgs>? ScoreChanged;
        
        public BaseGameStatistics(string gameId)
        {
            _gameId = gameId;
            Load();
        }
        
        public virtual void StartNewGame()
        {
            _gameStartTime = DateTime.Now;
            _score = 0;
            GamesPlayed++;
        }
        
        public virtual void EndGame(bool won)
        {
            var gameTime = CurrentGameTime;
            _totalTimePlayed += gameTime;
            
            if (won)
            {
                GamesWon++;
                if (BestTime == null || gameTime < BestTime)
                {
                    BestTime = gameTime;
                }
            }
            
            if (_score > BestScore)
            {
                BestScore = _score;
            }
            
            Save();
        }
        
        public virtual void AddScore(long points)
        {
            Score += points;
        }
        
        public virtual void Reset()
        {
            _score = 0;
            BestScore = 0;
            GamesPlayed = 0;
            GamesWon = 0;
            _totalTimePlayed = TimeSpan.Zero;
            BestTime = null;
            Save();
        }
        
        public virtual void Save()
        {
            var statsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClassicGamesCollection",
                "Statistics"
            );
            
            Directory.CreateDirectory(statsDir);
            
            var statsPath = Path.Combine(statsDir, $"{_gameId}_stats.json");
            var statsData = new
            {
                BestScore,
                GamesPlayed,
                GamesWon,
                TotalTimePlayed = _totalTimePlayed.Ticks,
                BestTime = BestTime?.Ticks
            };
            
            var json = System.Text.Json.JsonSerializer.Serialize(statsData, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(statsPath, json);
        }
        
        public virtual void Load()
        {
            var statsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClassicGamesCollection",
                "Statistics",
                $"{_gameId}_stats.json"
            );
            
            if (File.Exists(statsPath))
            {
                try
                {
                    var json = File.ReadAllText(statsPath);
                    using var doc = System.Text.Json.JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("BestScore", out var bestScore))
                        BestScore = bestScore.GetInt64();
                    
                    if (root.TryGetProperty("GamesPlayed", out var gamesPlayed))
                        GamesPlayed = gamesPlayed.GetInt32();
                    
                    if (root.TryGetProperty("GamesWon", out var gamesWon))
                        GamesWon = gamesWon.GetInt32();
                    
                    if (root.TryGetProperty("TotalTimePlayed", out var totalTime))
                        _totalTimePlayed = new TimeSpan(totalTime.GetInt64());
                    
                    if (root.TryGetProperty("BestTime", out var bestTime) && bestTime.ValueKind != System.Text.Json.JsonValueKind.Null)
                        BestTime = new TimeSpan(bestTime.GetInt64());
                }
                catch
                {
                    // If stats are corrupted, start fresh
                    Reset();
                }
            }
        }
        
        protected virtual void OnScoreChanged(long oldScore, long newScore)
        {
            ScoreChanged?.Invoke(this, new GameCore.Models.ScoreChangedEventArgs(oldScore, newScore));
        }
    }
}
