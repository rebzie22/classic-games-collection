using System.Drawing;
using System.Windows.Forms;
using GameCore.Models;

namespace Snake
{
    public partial class SnakeGameForm : Form
    {
        private readonly SnakeStatistics _statistics;
        private readonly System.Windows.Forms.Timer _gameTimer;
        private readonly List<Point> _snake;
        private Point _food;
        private Direction _direction;
        private Direction _nextDirection;
        private readonly Random _random;
        private int _score;
        private bool _gameOver;
        private bool _isPaused;
        private readonly string _difficulty;
        private readonly int _gameSpeed;
        private DateTime _gameStartTime;

        // Game constants
        private const int GridSize = 20;
        private const int GameWidth = 600;
        private const int GameHeight = 400;
        private readonly int _gridWidth = GameWidth / GridSize;
        private readonly int _gridHeight = GameHeight / GridSize;

        // Colors
        private readonly Color _snakeColor = Color.Green;
        private readonly Color _foodColor = Color.Red;
        private readonly Color _backgroundColor = Color.Black;

        // Pre-created brushes and pens for performance
        private readonly SolidBrush _snakeBrush = new SolidBrush(Color.Green);
        private readonly SolidBrush _foodBrush = new SolidBrush(Color.Red);
        private readonly SolidBrush _textBrush = new SolidBrush(Color.White);
        private readonly SolidBrush _hintBrush = new SolidBrush(Color.Gray);
        private readonly Pen _snakePen = new Pen(Color.DarkGreen);
        private readonly Font _scoreFont = new Font("Arial", 12);
        private readonly Font _hintFont = new Font("Arial", 10);

        public event EventHandler<ScoreChangedEventArgs>? ScoreChanged;
        public event EventHandler? GameOver;
        public event EventHandler? ExitRequested;

        public SnakeGameForm(string difficulty, SnakeStatistics statistics)
        {
            _difficulty = difficulty;
            _statistics = statistics;
            _gameSpeed = GetSpeedFromDifficulty(difficulty);
            _random = new Random();
            _snake = new List<Point>();
            
            InitializeComponent();
            InitializeGame();
            
            _gameTimer = new System.Windows.Forms.Timer
            {
                Interval = _gameSpeed
            };
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();
            
            _gameStartTime = DateTime.Now;
            _statistics.StartNewGame();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            
            // Enable double buffering to prevent flickering
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer | 
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
            
            // Form properties
            Text = $"Snake - {_difficulty}";
            Size = new Size(GameWidth + 16, GameHeight + 39); // Account for borders
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            KeyPreview = true;
            BackColor = _backgroundColor;
            ShowInTaskbar = false; // Don't show in taskbar to prevent it from being treated as main form
            TopMost = false; // Ensure it doesn't force itself to top
            
            // Handle events
            KeyDown += SnakeGameForm_KeyDown;
            Paint += SnakeGameForm_Paint;
            FormClosing += SnakeGameForm_FormClosing;
            
            ResumeLayout(false);
        }

        private void InitializeGame()
        {
            _snake.Clear();
            _snake.Add(new Point(_gridWidth / 2, _gridHeight / 2)); // Start in center
            _direction = Direction.Right;
            _nextDirection = Direction.Right;
            _score = 0;
            _gameOver = false;
            _isPaused = false;
            GenerateFood();
        }

        private int GetSpeedFromDifficulty(string difficulty)
        {
            return difficulty switch
            {
                "Beginner" => 200,     // Slower
                "Intermediate" => 150, // Medium speed
                "Expert" => 100,       // Faster
                _ => 150
            };
        }

        private void GenerateFood()
        {
            do
            {
                _food = new Point(_random.Next(0, _gridWidth), _random.Next(0, _gridHeight));
            } while (_snake.Contains(_food));
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (_gameOver || _isPaused) return;

            MoveSnake();
            CheckCollisions();
            CheckFood();
            Invalidate(); // Trigger repaint
        }

        private void MoveSnake()
        {
            _direction = _nextDirection;
            
            Point head = _snake[0];
            Point newHead = _direction switch
            {
                Direction.Up => new Point(head.X, head.Y - 1),
                Direction.Down => new Point(head.X, head.Y + 1),
                Direction.Left => new Point(head.X - 1, head.Y),
                Direction.Right => new Point(head.X + 1, head.Y),
                _ => head
            };

            _snake.Insert(0, newHead);
            
            // Remove tail unless we just ate food
            if (_snake.Count > _score + 1)
            {
                _snake.RemoveAt(_snake.Count - 1);
            }
        }

        private void CheckCollisions()
        {
            Point head = _snake[0];
            
            // Check wall collisions
            if (head.X < 0 || head.X >= _gridWidth || head.Y < 0 || head.Y >= _gridHeight)
            {
                EndGame();
                return;
            }
            
            // Check self collision
            for (int i = 1; i < _snake.Count; i++)
            {
                if (_snake[i] == head)
                {
                    EndGame();
                    return;
                }
            }
        }

        private void CheckFood()
        {
            if (_snake[0] == _food)
            {
                _score++;
                _statistics.UpdateFoodEaten(1);
                _statistics.UpdateLongestSnake(_snake.Count);
                
                // Don't trigger ScoreChanged here - only when game ends
                GenerateFood();
            }
        }

        private void EndGame()
        {
            _gameOver = true;
            _gameTimer.Stop();
            
            var gameTime = DateTime.Now - _gameStartTime;
            _statistics.UpdateFastestTime(gameTime);
            _statistics.EndGame(false); // Snake is always "lost" when it ends
            
            // Fire ScoreChanged event only when game ends for high score tracking
            ScoreChanged?.Invoke(this, new ScoreChangedEventArgs(0, _score));
            
            GameOver?.Invoke(this, EventArgs.Empty);
            
            var result = MessageBox.Show(
                $"Game Over!\n\nScore: {_score}\nLength: {_snake.Count}\nTime: {gameTime:mm\\:ss}\n\nPlay again?",
                "Snake - Game Over",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            
            if (result == DialogResult.Yes)
            {
                RestartGame();
            }
            else
            {
                // Instead of closing or signaling exit, just hide the form
                // The parent will handle cleanup when the main app closes
                Hide();
                _gameTimer.Stop();
                
                // Fire the exit event for cleanup but don't close
                ExitRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void RestartGame()
        {
            InitializeGame();
            _gameTimer.Start();
            _gameStartTime = DateTime.Now;
        }

        public void PauseGame()
        {
            _isPaused = true;
            _gameTimer.Stop();
        }

        public void ResumeGame()
        {
            _isPaused = false;
            if (!_gameOver)
            {
                _gameTimer.Start();
            }
        }

        private void SnakeGameForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_gameOver) return;

            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    if (_direction != Direction.Down)
                        _nextDirection = Direction.Up;
                    break;
                case Keys.Down:
                case Keys.S:
                    if (_direction != Direction.Up)
                        _nextDirection = Direction.Down;
                    break;
                case Keys.Left:
                case Keys.A:
                    if (_direction != Direction.Right)
                        _nextDirection = Direction.Left;
                    break;
                case Keys.Right:
                case Keys.D:
                    if (_direction != Direction.Left)
                        _nextDirection = Direction.Right;
                    break;
                case Keys.Space:
                    if (_isPaused)
                        ResumeGame();
                    else
                        PauseGame();
                    break;
                case Keys.Escape:
                    // Ask for confirmation before exiting
                    var result = MessageBox.Show(
                        "Are you sure you want to quit the game?",
                        "Quit Game",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        Hide();
                        _gameTimer.Stop();
                        ExitRequested?.Invoke(this, EventArgs.Empty);
                    }
                    break;
            }
        }

        private void SnakeGameForm_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            
            // Improve rendering quality
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None; // Faster for pixel graphics
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            
            // Draw snake
            foreach (Point segment in _snake)
            {
                Rectangle rect = new Rectangle(segment.X * GridSize, segment.Y * GridSize, GridSize, GridSize);
                g.FillRectangle(_snakeBrush, rect);
                g.DrawRectangle(_snakePen, rect);
            }
            
            // Draw food
            Rectangle foodRect = new Rectangle(_food.X * GridSize, _food.Y * GridSize, GridSize, GridSize);
            g.FillRectangle(_foodBrush, foodRect);
            
            // Draw score
            string scoreText = $"Score: {_score}  Length: {_snake.Count}";
            if (_isPaused)
                scoreText += "  [PAUSED - Press Space to Resume]";
                
            g.DrawString(scoreText, _scoreFont, _textBrush, 10, 10);
            
            // Draw controls hint
            if (_score == 0)
            {
                string hint = "Use Arrow Keys or WASD to move. Space to pause. ESC to exit.";
                g.DrawString(hint, _hintFont, _hintBrush, 10, GameHeight - 25);
            }
        }

        private void SnakeGameForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Prevent the form from actually closing unless we're disposing
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                _gameTimer?.Stop();
                ExitRequested?.Invoke(this, EventArgs.Empty);
                return;
            }
            
            _gameTimer?.Stop();
            _gameTimer?.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gameTimer?.Dispose();
                
                // Dispose graphics resources
                _snakeBrush?.Dispose();
                _foodBrush?.Dispose();
                _textBrush?.Dispose();
                _hintBrush?.Dispose();
                _snakePen?.Dispose();
                _scoreFont?.Dispose();
                _hintFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}
