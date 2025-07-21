using GameCore.Interfaces;
using GameCore.Models;
using GameCore.Base;
using System.Drawing;

namespace Solitaire
{
    /// <summary>
    /// Classic Klondike Solitaire implementation for the Classic Games Collection
    /// </summary>
    public class SolitaireGame : IGame
    {
        private SolitaireGameForm? _gameForm;
        private SolitaireStatistics _statistics;
        private GameState _state = GameState.Stopped;
        
        public string GameId => "solitaire";
        public string DisplayName => "Solitaire";
        public string Description => "Classic Klondike Solitaire card game with drag-and-drop functionality";
        public string Version => "1.0.0";
        
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
        
        public event EventHandler<GameStateChangedEventArgs>? StateChanged;
        public event EventHandler<ScoreChangedEventArgs>? ScoreChanged;
        
        public Image? Icon 
        { 
            get 
            {
                // Create a simple card-themed icon
                var bitmap = new Bitmap(64, 64);
                using var g = Graphics.FromImage(bitmap);
                
                // Background
                g.FillRectangle(Brushes.DarkGreen, 0, 0, 64, 64);
                
                // Card outline
                g.FillRectangle(Brushes.White, 15, 10, 20, 30);
                g.DrawRectangle(Pens.Black, 15, 10, 20, 30);
                
                // Spade symbol
                g.FillEllipse(Brushes.Black, 22, 18, 6, 8);
                g.FillPolygon(Brushes.Black, new Point[] { 
                    new(25, 26), new(22, 30), new(28, 30) 
                });
                g.FillRectangle(Brushes.Black, 24, 28, 2, 4);
                
                // Second card (slightly offset)
                g.FillRectangle(Brushes.White, 25, 20, 20, 30);
                g.DrawRectangle(Pens.Black, 25, 20, 20, 30);
                
                // Heart symbol
                g.FillEllipse(Brushes.Red, 30, 28, 4, 4);
                g.FillEllipse(Brushes.Red, 34, 28, 4, 4);
                g.FillPolygon(Brushes.Red, new Point[] { 
                    new(30, 32), new(32, 36), new(34, 36), new(38, 32) 
                });
                
                return bitmap;
            }
        }
        
        public IReadOnlyList<string> DifficultyLevels => new[] { "Beginner", "Intermediate", "Expert" };
        
        public IGameStatistics Statistics => _statistics;
        
        public SolitaireGame()
        {
            _statistics = new SolitaireStatistics();
            _statistics.ScoreChanged += (s, e) => ScoreChanged?.Invoke(this, e);
        }
        
        public void Initialize(GameCore.Models.GameSettings settings)
        {
            // Load game settings
        }
        
        public void StartNew()
        {
            StartNew("Normal");
        }
        
        public void StartNew(string difficulty)
        {
            // Clean up existing game
            if (_gameForm != null)
            {
                _gameForm.Close();
                _gameForm.Dispose();
            }
            
            // Create new game form
            _gameForm = new SolitaireGameForm(difficulty, _statistics);
            _gameForm.FormClosed += (s, e) => 
            {
                _gameForm = null;
                State = GameState.Stopped;
            };
            
            _statistics.StartNewGame();
            State = GameState.Running;
            
            _gameForm.Show();
        }
        
        public void Pause()
        {
            if (State == GameState.Running)
            {
                State = GameState.Paused;
                _gameForm?.PauseGame();
            }
        }
        
        public void Resume()
        {
            if (State == GameState.Paused)
            {
                State = GameState.Running;
                _gameForm?.ResumeGame();
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
            _gameForm?.Dispose();
            _statistics.Save();
        }
    }
}
