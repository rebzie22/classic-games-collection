using GameCore.Interfaces;
using GameCore.Models;
using GameCore.Base;
using System.Drawing;

namespace Minesweeper
{
    /// <summary>
    /// Adapter that wraps Jordan's existing Minesweeper implementation 
    /// to work with the Classic Games Collection launcher
    /// </summary>
    public class MinesweeperGameAdapter : IGame
    {
        private YourMinesweeper.MainForm? _gameForm;
        private MinesweeperStatistics _statistics;
        private GameCore.Models.GameSettings _launcherSettings;
        private string _preferredDifficulty = "Beginner";
        private GameState _state;
        private Image? _icon;
        
        public string GameId => "minesweeper";
        public string DisplayName => "Minesweeper (Jordan's Implementation)";
        public string Description => "Classic mine detection puzzle game - authentic Windows recreation";
        public string Version => "2.0.0";
        public Image? Icon 
        { 
            get 
            {
                if (_icon == null)
                {
                    // Try to load your minesweeper.png from embedded resources first
                    _icon = LoadMinesweeperImage() ?? CreateDefaultMinesweeperIcon();
                }
                return _icon;
            }
        } 
        
        public IReadOnlyList<string> DifficultyLevels => new[] { "Beginner", "Intermediate", "Expert" };
        
        public GameState State 
        { 
            get => _state;
            private set
            {
                if (_state != value)
                {
                    var oldState = _state;
                    _state = value;
                    StateChanged?.Invoke(this, new GameStateChangedEventArgs(oldState, value));
                }
            }
        }
        
        public IGameStatistics Statistics => _statistics;
        
        public event EventHandler<GameStateChangedEventArgs>? StateChanged;
        public event EventHandler<ScoreChangedEventArgs>? ScoreChanged;
        
        public MinesweeperGameAdapter()
        {
            _statistics = new MinesweeperStatistics(GameId);
            _statistics.ScoreChanged += (s, e) => {
                ScoreChanged?.Invoke(this, e);
            };
            _launcherSettings = new GameCore.Models.GameSettings();
            _state = GameState.NotInitialized;
        }
        
        public void Initialize(GameCore.Models.GameSettings settings)
        {
            _launcherSettings = settings;
            State = GameState.Ready;
        }
        
        public void StartNew()
        {
            StartNew(_preferredDifficulty);
        }
        
        public void SetPreferredDifficulty(string difficulty)
        {
            _preferredDifficulty = difficulty ?? "Beginner";
        }
        
        public void StartNew(string difficulty)
        {
            // Remember the difficulty for future StartNew() calls
            difficulty = difficulty ?? "Beginner";
            _preferredDifficulty = difficulty;
            
            if (_gameForm != null)
            {
                _gameForm.Close();
                _gameForm.Dispose();
            }
            
            // Convert launcher difficulty to your game's difficulty
            var gameSettings = ConvertToGameSettings(difficulty);
            
            // Create your existing MainForm with the settings
            _gameForm = new YourMinesweeper.MainForm(gameSettings);
            _gameForm.FormClosed += (s, e) => 
            {
                _gameForm = null;
                State = GameState.Stopped;
            };
            
            // Hook into your game's events if available
            // You might need to modify your MainForm to expose these events
            HookIntoGameEvents();
            
            _statistics.StartNewGame();
            State = GameState.Running;
            
            _gameForm.Show();
        }
        
        public void Pause()
        {
            if (State == GameState.Running)
            {
                State = GameState.Paused;
                // You might need to add pause functionality to your MainForm
            }
        }
        
        public void Resume()
        {
            if (State == GameState.Paused)
            {
                State = GameState.Running;
                // You might need to add resume functionality to your MainForm
            }
        }
        
        public void Stop()
        {
            State = GameState.Stopped;
            _gameForm?.Close();
        }
        
        public Form GetGameWindow()
        {
            if (_gameForm == null)
            {
                StartNew();
            }
            return _gameForm!;
        }
        
        public void Dispose()
        {
            if (_gameForm?.GameEngine != null)
            {
                _gameForm.GameEngine.GameStateChanged -= OnGameEngineStateChanged;
            }
            _gameForm?.Dispose();
            _statistics.Save();
        }
        
        private YourMinesweeper.GameSettings ConvertToGameSettings(string difficulty)
        {
            // Convert our launcher difficulty to your game's settings format
            return difficulty.ToLower() switch
            {
                "beginner" => YourMinesweeper.GameSettings.Beginner(),
                "intermediate" => YourMinesweeper.GameSettings.Intermediate(),
                "expert" => YourMinesweeper.GameSettings.Expert(),
                _ => YourMinesweeper.GameSettings.Beginner()
            };
        }
        
        private void HookIntoGameEvents()
        {
            if (_gameForm?.GameEngine != null)
            {
                _gameForm.GameEngine.GameStateChanged += OnGameEngineStateChanged;
            }
        }
        
        private void OnGameEngineStateChanged(object? sender, YourMinesweeper.GameStateChangedEventArgs e)
        {
            // Map the Minesweeper game states to our launcher states
            switch (e.NewState)
            {
                case YourMinesweeper.GameState.Playing:
                    State = GameState.Running;
                    break;
                case YourMinesweeper.GameState.Won:
                    OnGameWon();
                    break;
                case YourMinesweeper.GameState.Lost:
                    OnGameLost();
                    break;
                case YourMinesweeper.GameState.NotStarted:
                    State = GameState.Stopped;
                    break;
            }
        }
        
        internal void OnGameWon()
        {
            State = GameState.Won;
            ((MinesweeperStatistics)_statistics).EndGame(true);
            // Calculate score based on time and difficulty
            int timeScore = CalculateTimeScore();
            int difficultyMultiplier = GetDifficultyMultiplier();
            int finalScore = timeScore * difficultyMultiplier;

            // Time property removed from ScoreEntry; no longer set here

            // Trigger score changed event (previous score was 0, new score is calculated)
            ScoreChanged?.Invoke(this, new ScoreChangedEventArgs(0, finalScore));
        }
        
        internal void OnGameLost()
        {
            State = GameState.Lost;
            ((MinesweeperStatistics)_statistics).EndGame(false);
            
            // No score for lost games, but still trigger event with 0 (previous 0, new 0)
            ScoreChanged?.Invoke(this, new ScoreChangedEventArgs(0, 0));
        }
        
        private int CalculateTimeScore()
        {
            // Get elapsed time from the main form
            if (_gameForm != null)
            {
                // If SecondsElapsed is in tenths or hundredths of a second, convert to seconds
                // Try dividing by 10 first (tenths), if still too high, try 100 or 1000 (hundredths/milliseconds)
                int elapsedSeconds = _gameForm.SecondsElapsed;
                if (elapsedSeconds > 300) // If it's suspiciously high, try dividing
                {
                    if (elapsedSeconds > 10000) // likely milliseconds
                        elapsedSeconds = elapsedSeconds / 1000;
                    else // likely tenths of a second
                        elapsedSeconds = elapsedSeconds / 10;
                }
                // Higher score for faster completion (max 1000, decreases with time)
                return Math.Max(1000 - elapsedSeconds, 100);
            }
            return 100; // Default score if we can't get time
        }
        
        private int GetDifficultyMultiplier()
        {
            // Determine difficulty based on game board size and mine count
            if (_gameForm?.GameEngine != null)
            {
                var rows = _gameForm.GameEngine.Rows;
                var cols = _gameForm.GameEngine.Columns; 
                var mines = _gameForm.GameEngine.MineCount;
                
                // Match the settings from GameSettings.GetSettings()
                if (rows == 9 && cols == 9 && mines == 10)
                    return 1; // Beginner
                else if (rows == 16 && cols == 16 && mines == 40)
                    return 2; // Intermediate  
                else if (rows == 16 && cols == 30 && mines == 99)
                    return 3; // Expert
            }
            
            return 1; // Default to beginner multiplier
        }
        
        private Image? LoadMinesweeperImage()
        {
            try
            {
                // Try to load from GameLauncher's embedded resources
                var assembly = System.Reflection.Assembly.LoadFrom(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameLauncher.exe"));
                
                string resourceName = "GameLauncher.Assets.Images.minesweeper.png";
                using var stream = assembly.GetManifestResourceStream(resourceName);
                
                if (stream != null)
                {
                    return Image.FromStream(stream);
                }
            }
            catch
            {
                // If loading fails, we'll use the default icon
            }
            
            return null;
        }
        
        private Image CreateDefaultMinesweeperIcon()
        {
            var bitmap = new Bitmap(64, 64);
            using var graphics = Graphics.FromImage(bitmap);
            
            // Gray background like classic Minesweeper
            graphics.FillRectangle(Brushes.Silver, 0, 0, 64, 64);
            
            // Draw border (raised button effect)
            using var lightPen = new Pen(Color.White, 2);
            using var darkPen = new Pen(Color.Gray, 2);
            
            // Top and left - light
            graphics.DrawLine(lightPen, 0, 0, 63, 0);
            graphics.DrawLine(lightPen, 0, 0, 0, 63);
            
            // Bottom and right - dark  
            graphics.DrawLine(darkPen, 63, 0, 63, 63);
            graphics.DrawLine(darkPen, 0, 63, 63, 63);
            
            // Draw mine symbol (circle with spikes)
            using var mineBrush = new SolidBrush(Color.Black);
            graphics.FillEllipse(mineBrush, 22, 22, 20, 20);
            
            // Draw spikes
            using var spikePen = new Pen(Color.Black, 2);
            // Top
            graphics.DrawLine(spikePen, 32, 15, 32, 22);
            // Bottom  
            graphics.DrawLine(spikePen, 32, 42, 32, 49);
            // Left
            graphics.DrawLine(spikePen, 15, 32, 22, 32);
            // Right
            graphics.DrawLine(spikePen, 42, 32, 49, 32);
            // Diagonals
            graphics.DrawLine(spikePen, 24, 24, 18, 18);
            graphics.DrawLine(spikePen, 40, 24, 46, 18);
            graphics.DrawLine(spikePen, 24, 40, 18, 46);
            graphics.DrawLine(spikePen, 40, 40, 46, 46);
            
            return bitmap;
        }
    }
    
    public class MinesweeperStatistics : BaseGameStatistics
    {
        public GameCore.Models.ScoreEntry? LatestScoreEntry { get; private set; }

        public void SetLatestScoreEntry(GameCore.Models.ScoreEntry entry)
        {
            LatestScoreEntry = entry;
        }

        public int MinesFound { get; private set; }
        public int TotalMines { get; private set; }

        public MinesweeperStatistics(string gameId) : base(gameId)
        {
        }

        public void UpdateMineStats(int minesFound, int totalMines)
        {
            MinesFound = minesFound;
            TotalMines = totalMines;

            // Score based on efficiency and time
            if (totalMines > 0)
            {
                var efficiency = (double)minesFound / totalMines * 100;
                var timeBonus = Math.Max(0, 1000 - (int)CurrentGameTime.TotalSeconds);
                AddScore((long)(efficiency + timeBonus));
            }
        }
    }
}
