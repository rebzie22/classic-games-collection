using GameCore.Interfaces;
using GameCore.Models;
using GameCore.Base;
using System.Drawing;

namespace Tetris
{
    public class TetrisGame : IGame
    {
        private Image? _icon;
        private TetrisGameForm? _gameForm;
        private readonly TetrisStatistics _statistics;
        
        public string GameId => "tetris";
        public string DisplayName => "Tetris";
        public string Description => "Classic block-stacking puzzle game. Clear lines by filling them completely with tetrominoes.";
        public string Version => "1.0.0";
        public Image? Icon 
        { 
            get 
            {
                if (_icon == null)
                {
                    _icon = CreateDefaultTetrisIcon();
                }
                return _icon;
            }
        }
        
        public IReadOnlyList<string> DifficultyLevels => new[] { "Beginner", "Intermediate", "Expert" };
        public GameState State { get; private set; } = GameState.NotInitialized;
        public IGameStatistics Statistics => _statistics;

        public event EventHandler<GameStateChangedEventArgs>? StateChanged;
        public event EventHandler<ScoreChangedEventArgs>? ScoreChanged;

        public TetrisGame()
        {
            _statistics = new TetrisStatistics("tetris");
        }

        public void Initialize(GameSettings settings)
        {
            State = GameState.Ready;
            StateChanged?.Invoke(this, new GameStateChangedEventArgs(GameState.NotInitialized, GameState.Ready));
        }

        public void StartNew() => StartNew("Beginner");

        public void StartNew(string difficulty)
        {
            if (_gameForm != null && !_gameForm.IsDisposed)
            {
                _gameForm.Close();
                _gameForm.Dispose();
                _gameForm = null;
            }

            _gameForm = new TetrisGameForm(difficulty);
            _gameForm.ScoreChanged += OnGameFormScoreChanged;
            _gameForm.ExitRequested += OnGameFormExitRequested;

            _gameForm.Show();
            _gameForm.Focus();

            State = GameState.Running;
            StateChanged?.Invoke(this, new GameStateChangedEventArgs(GameState.Ready, GameState.Running));
        }

        private void OnGameFormScoreChanged(object? sender, ScoreChangedEventArgs e)
        {
            // Re-fire the event with this TetrisGame as the sender
            ScoreChanged?.Invoke(this, e);
        }

        private void OnGameFormExitRequested(object? sender, EventArgs e)
        {
            Stop();
        }

        public void Pause()
        {
            // The pause functionality is handled within the game form
            State = GameState.Paused;
            StateChanged?.Invoke(this, new GameStateChangedEventArgs(GameState.Running, GameState.Paused));
        }

        public void Resume()
        {
            State = GameState.Running;
            StateChanged?.Invoke(this, new GameStateChangedEventArgs(GameState.Paused, GameState.Running));
        }

        public void Stop()
        {
            var oldState = State;
            
            if (_gameForm != null && !_gameForm.IsDisposed)
            {
                _gameForm.ScoreChanged -= OnGameFormScoreChanged;
                _gameForm.ExitRequested -= OnGameFormExitRequested;
                _gameForm.Hide();
                _gameForm.Dispose();
                _gameForm = null;
            }
            
            State = GameState.Ready;
            StateChanged?.Invoke(this, new GameStateChangedEventArgs(oldState, GameState.Ready));
        }

        public Form GetGameWindow()
        {
            if (_gameForm == null || _gameForm.IsDisposed)
            {
                _gameForm = new TetrisGameForm("Beginner");
            }
            return _gameForm;
        }

        public void ShowSettings(Form? parentForm = null)
        {
            var controls = TetrisControls.Load();
            using var controlsForm = new TetrisControlsForm(controls);
            
            if (parentForm != null)
            {
                controlsForm.Owner = parentForm;
            }

            if (controlsForm.ShowDialog() == DialogResult.OK)
            {
                // Controls are automatically saved in the form
            }
        }

        public void Dispose()
        {
            if (_gameForm != null && !_gameForm.IsDisposed)
            {
                _gameForm.ScoreChanged -= OnGameFormScoreChanged;
                _gameForm.ExitRequested -= OnGameFormExitRequested;
                _gameForm.Close();
                _gameForm.Dispose();
                _gameForm = null;
            }
            _icon?.Dispose();
        }

        private Image CreateDefaultTetrisIcon()
        {
            var bitmap = new Bitmap(64, 64);
            using var graphics = Graphics.FromImage(bitmap);
            
            // Black background like classic Tetris
            graphics.FillRectangle(Brushes.Black, 0, 0, 64, 64);
            
            // Draw border
            using var borderPen = new Pen(Color.White, 2);
            graphics.DrawRectangle(borderPen, 1, 1, 62, 62);
            
            // Draw some colorful Tetris blocks
            var colors = new[] { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple, Color.Orange, Color.Cyan };
            
            // Bottom row - filled
            for (int i = 0; i < 7; i++)
            {
                using var brush = new SolidBrush(colors[i]);
                graphics.FillRectangle(brush, 8 + i * 7, 50, 6, 6);
                graphics.DrawRectangle(Pens.White, 8 + i * 7, 50, 6, 6);
            }
            
            // Second row - partial
            for (int i = 0; i < 5; i++)
            {
                using var brush = new SolidBrush(colors[i]);
                graphics.FillRectangle(brush, 8 + i * 7, 43, 6, 6);
                graphics.DrawRectangle(Pens.White, 8 + i * 7, 43, 6, 6);
            }
            
            // Falling T-piece in purple
            using var tBrush = new SolidBrush(Color.Purple);
            graphics.FillRectangle(tBrush, 22, 15, 6, 6); // Top
            graphics.FillRectangle(tBrush, 15, 22, 6, 6); // Left
            graphics.FillRectangle(tBrush, 22, 22, 6, 6); // Center
            graphics.FillRectangle(tBrush, 29, 22, 6, 6); // Right
            
            // Draw grid lines for the T-piece
            graphics.DrawRectangle(Pens.White, 22, 15, 6, 6);
            graphics.DrawRectangle(Pens.White, 15, 22, 6, 6);
            graphics.DrawRectangle(Pens.White, 22, 22, 6, 6);
            graphics.DrawRectangle(Pens.White, 29, 22, 6, 6);
            
            return bitmap;
        }
    }
}
