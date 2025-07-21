using GameCore.Interfaces;
using GameCore.Models;
using GameCore.Base;
using System.Drawing;

namespace Snake
{
    public class SnakeGame : IGame
    {
        private Image? _icon;
        private SnakeGameForm? _gameForm;
        private readonly SnakeStatistics _statistics;
        
        public string GameId => "snake";
        public string DisplayName => "Snake";
        public string Description => "Classic snake arcade game with increasingly challenging difficulty levels";
        public string Version => "1.0.0";
        public Image? Icon 
        { 
            get 
            {
                if (_icon == null)
                {
                    _icon = CreateDefaultSnakeIcon();
                }
                return _icon;
            }
        }
        
        public IReadOnlyList<string> DifficultyLevels => new[] { "Beginner", "Intermediate", "Expert" };
        public GameState State { get; private set; } = GameState.NotInitialized;
        public IGameStatistics Statistics => _statistics;

        public event EventHandler<GameStateChangedEventArgs>? StateChanged;
        public event EventHandler<ScoreChangedEventArgs>? ScoreChanged;

        public SnakeGame()
        {
            _statistics = new SnakeStatistics(GameId);
        }

        public void Initialize(GameSettings settings) 
        {
            State = GameState.Ready;
            StateChanged?.Invoke(this, new GameStateChangedEventArgs(GameState.NotInitialized, State));
        }
        
        public void StartNew() => StartNew("Intermediate");
        
        public void StartNew(string difficulty) 
        {
            Stop(); // Stop any existing game
            
            var previousState = State;
            State = GameState.Running;
            StateChanged?.Invoke(this, new GameStateChangedEventArgs(previousState, State));
            
            _gameForm = new SnakeGameForm(difficulty, _statistics);
            _gameForm.ScoreChanged += (s, e) => ScoreChanged?.Invoke(this, e);
            _gameForm.GameOver += OnGameOver;
            _gameForm.ExitRequested += OnExitRequested;
            _gameForm.Show();
        }
        
        public void Pause() 
        {
            if (State == GameState.Running && _gameForm != null)
            {
                _gameForm.PauseGame();
                var previousState = State;
                State = GameState.Paused;
                StateChanged?.Invoke(this, new GameStateChangedEventArgs(previousState, State));
            }
        }
        
        public void Resume() 
        {
            if (State == GameState.Paused && _gameForm != null)
            {
                _gameForm.ResumeGame();
                var previousState = State;
                State = GameState.Running;
                StateChanged?.Invoke(this, new GameStateChangedEventArgs(previousState, State));
            }
        }
        
        public void Stop() 
        {
            if (_gameForm != null)
            {
                _gameForm.GameOver -= OnGameOver;
                _gameForm.ExitRequested -= OnExitRequested;
                
                // Hide instead of close to prevent app shutdown
                if (_gameForm.Visible)
                {
                    _gameForm.Hide();
                }
                
                // Only dispose, don't close
                _gameForm.Dispose();
                _gameForm = null;
            }
            
            var previousState = State;
            State = GameState.Ready;
            StateChanged?.Invoke(this, new GameStateChangedEventArgs(previousState, State));
        }
        
        public Form GetGameWindow() => _gameForm ?? new Form { Text = "Snake Game", Size = new Size(400, 300) };
        
        private void OnGameOver(object? sender, EventArgs e)
        {
            var previousState = State;
            State = GameState.Lost;
            StateChanged?.Invoke(this, new GameStateChangedEventArgs(previousState, State));
        }
        
        private void OnExitRequested(object? sender, EventArgs e)
        {
            // Stop the game gracefully when user requests to exit
            Stop();
        }
        
        public void Dispose() 
        {
            Stop();
            _icon?.Dispose();
        }
        
        private Image CreateDefaultSnakeIcon()
        {
            var bitmap = new Bitmap(64, 64);
            using var graphics = Graphics.FromImage(bitmap);
            
            // Dark green background
            graphics.FillRectangle(new SolidBrush(Color.DarkGreen), 0, 0, 64, 64);
            
            // Draw border
            using var borderPen = new Pen(Color.LimeGreen, 2);
            graphics.DrawRectangle(borderPen, 1, 1, 62, 62);
            
            // Draw snake segments (simplified)
            using var snakeBrush = new SolidBrush(Color.LimeGreen);
            
            // Snake head
            graphics.FillRectangle(snakeBrush, 10, 10, 8, 8);
            
            // Snake body segments
            graphics.FillRectangle(snakeBrush, 18, 10, 8, 8);
            graphics.FillRectangle(snakeBrush, 26, 10, 8, 8);
            graphics.FillRectangle(snakeBrush, 34, 10, 8, 8);
            graphics.FillRectangle(snakeBrush, 42, 10, 8, 8);
            graphics.FillRectangle(snakeBrush, 42, 18, 8, 8);
            graphics.FillRectangle(snakeBrush, 42, 26, 8, 8);
            graphics.FillRectangle(snakeBrush, 34, 26, 8, 8);
            graphics.FillRectangle(snakeBrush, 26, 26, 8, 8);
            
            // Draw food (red dot)
            using var foodBrush = new SolidBrush(Color.Red);
            graphics.FillEllipse(foodBrush, 46, 46, 6, 6);
            
            return bitmap;
        }
    }
}
