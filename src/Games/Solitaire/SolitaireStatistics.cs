using GameCore.Base;
using GameCore.Models;
using System.Text.Json;

namespace Solitaire
{
    /// <summary>
    /// Statistics tracking for Solitaire games
    /// </summary>
    public class SolitaireStatistics : BaseGameStatistics
    {
        public new int GamesWon { get; private set; }
        public int GamesLost { get; private set; }
        public new double WinPercentage => GamesPlayed == 0 ? 0 : (double)GamesWon / GamesPlayed * 100;
        
        public int HighestScore { get; private set; }
        public new TimeSpan? BestTime { get; private set; } = null;
        public int TotalCardsPlayed { get; private set; }
        public int TotalMoves { get; private set; }
        
        // Current game tracking
        public int CurrentScore { get; private set; }
        public int CurrentMoves { get; private set; }
        public DateTime GameStartTime { get; private set; }
        
        private readonly string _statsFilePath;
        
        public SolitaireStatistics() : base("solitaire")
        {
            _statsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClassicGamesCollection",
                "solitaire_stats.json"
            );
            LoadStats();
        }
        
        public override void StartNewGame()
        {
            base.StartNewGame();
            CurrentScore = 0;
            CurrentMoves = 0;
            GameStartTime = DateTime.Now;
            SaveStats();
        }
        
        public void RecordMove()
        {
            CurrentMoves++;
            TotalMoves++;
        }
        
        public void UpdateScore(int newScore)
        {
            var oldScore = CurrentScore;
            CurrentScore = newScore;
            OnScoreChanged(oldScore, newScore);
        }
        
        public void EndGame(bool won, int finalScore)
        {
            var gameTime = DateTime.Now - GameStartTime;
            
            if (won)
            {
                GamesWon++;
                
                // Update best score
                if (finalScore > HighestScore)
                {
                    HighestScore = finalScore;
                }
                
                // Update best time
                if (BestTime == null || gameTime < BestTime)
                {
                    BestTime = gameTime;
                }
                
                // OnScoreChanged removed to prevent duplicate high score saves
            }
            else
            {
                GamesLost++;
            }
            
            SaveStats();
        }
        
        private void LoadStats()
        {
            try
            {
                if (File.Exists(_statsFilePath))
                {
                    var json = File.ReadAllText(_statsFilePath);
                    var data = JsonSerializer.Deserialize<SolitaireStatsData>(json);
                    if (data != null)
                    {
                        GamesPlayed = data.TotalGames;
                        GamesWon = data.GamesWon;
                        GamesLost = data.GamesLost;
                        HighestScore = data.HighestScore;
                        BestTime = data.BestTime == TimeSpan.Zero ? null : data.BestTime;
                        TotalMoves = data.TotalMoves;
                        TotalCardsPlayed = data.TotalCardsPlayed;
                    }
                }
            }
            catch
            {
                // If loading fails, start with default values
            }
        }
        
        private void SaveStats()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_statsFilePath)!);
                var data = new SolitaireStatsData
                {
                    TotalGames = GamesPlayed,
                    GamesWon = GamesWon,
                    GamesLost = GamesLost,
                    HighestScore = HighestScore,
                    BestTime = BestTime ?? TimeSpan.Zero,
                    TotalMoves = TotalMoves,
                    TotalCardsPlayed = TotalCardsPlayed
                };
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_statsFilePath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }
        
        private class SolitaireStatsData
        {
            public int TotalGames { get; set; }
            public int GamesWon { get; set; }
            public int GamesLost { get; set; }
            public int HighestScore { get; set; }
            public TimeSpan BestTime { get; set; }
            public int TotalMoves { get; set; }
            public int TotalCardsPlayed { get; set; }
        }
    }
}
