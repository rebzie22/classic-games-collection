using GameCore.Interfaces;
using GameCore.Models;
using GameCore.Base;
using System.Drawing;

namespace Snake
{
    public class SnakeGame : IGame
    {
        private Image? _icon;
        
        public string GameId => "snake";
        public string DisplayName => "Snake";
        public string Description => "Classic snake arcade game - Coming Soon!";
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
        public IReadOnlyList<string> DifficultyLevels => new[] { "Slow", "Normal", "Fast", "Extreme" };
        public GameState State { get; private set; } = GameState.NotInitialized;
        public IGameStatistics Statistics { get; }

#pragma warning disable CS0067 // Event is never used - will be implemented later
        public event EventHandler<GameStateChangedEventArgs>? StateChanged;
        public event EventHandler<ScoreChangedEventArgs>? ScoreChanged;
#pragma warning restore CS0067

        public SnakeGame()
        {
            Statistics = new BaseGameStatistics(GameId);
        }

        public void Initialize(GameSettings settings) => State = GameState.Ready;
        public void StartNew() => StartNew("Normal");
        public void StartNew(string difficulty) { /* TODO: Implement */ }
        public void Pause() { /* TODO: Implement */ }
        public void Resume() { /* TODO: Implement */ }
        public void Stop() { /* TODO: Implement */ }
        public Form GetGameWindow() => new Form { Text = "Snake Game - Coming Soon!", Size = new Size(400, 300) };
        public void Dispose() { }
        
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
