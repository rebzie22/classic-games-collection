using GameCore.Interfaces;
using GameCore.Models;
using GameCore.Base;
using System.Drawing;

namespace Tetris
{
    public class TetrisGame : IGame
    {
        private Image? _icon;
        
        public string GameId => "tetris";
        public string DisplayName => "Tetris";
        public string Description => "Classic falling blocks puzzle game - Coming Soon!";
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
        public IReadOnlyList<string> DifficultyLevels => new[] { "Level 1", "Level 5", "Level 10", "Level 15" };
        public GameState State { get; private set; } = GameState.NotInitialized;
        public IGameStatistics Statistics { get; }

#pragma warning disable CS0067 // Event is never used - will be implemented later
        public event EventHandler<GameStateChangedEventArgs>? StateChanged;
        public event EventHandler<ScoreChangedEventArgs>? ScoreChanged;
#pragma warning restore CS0067

        public TetrisGame()
        {
            Statistics = new BaseGameStatistics(GameId);
        }

        public void Initialize(GameSettings settings) => State = GameState.Ready;
        public void StartNew() => StartNew("Level 1");
        public void StartNew(string difficulty) { /* TODO: Implement */ }
        public void Pause() { /* TODO: Implement */ }
        public void Resume() { /* TODO: Implement */ }
        public void Stop() { /* TODO: Implement */ }
        public Form GetGameWindow() => new Form { Text = "Tetris Game - Coming Soon!", Size = new Size(400, 600) };
        public void Dispose() { }
        
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
