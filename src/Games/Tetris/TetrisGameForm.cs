using System.Drawing;
using GameCore.Models;

namespace Tetris
{
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }
    }

    public partial class TetrisGameForm : Form
    {
        private readonly TetrisBoard _board;
        private readonly TetrisStatistics _statistics;
        private TetrisControls _controls;
        private readonly Random _random;
        private readonly System.Windows.Forms.Timer _gameTimer;
        private readonly System.Windows.Forms.Timer _dropTimer;

        private Tetromino? _currentPiece;
        private Tetromino? _nextPiece;
        private Tetromino? _heldPiece;
        private bool _canHold = true;
        private bool _isPaused;
        private int _level = 1;
        private int _linesUntilNextLevel = 10;
        private DateTime _gameStartTime;

        // UI Controls
        private Panel _gamePanel = null!;
        private Panel _sidePanel = null!;
        private Label _scoreLabel = null!;
        private Label _levelLabel = null!;
        private Label _linesLabel = null!;
        private Panel _nextPiecePanel = null!;
        private Panel _heldPiecePanel = null!;
        private Button _pauseButton = null!;
        private Button _controlsButton = null!;

        // Drawing constants
        private const int BlockSize = 25;
        private const int BoardWidth = TetrisBoard.Width * BlockSize;
        private const int BoardHeight = TetrisBoard.Height * BlockSize;

        public event EventHandler<ScoreChangedEventArgs>? ScoreChanged;
        public event EventHandler? ExitRequested;

        public TetrisGameForm(string difficulty)
        {
            _board = new TetrisBoard();
            _statistics = new TetrisStatistics("tetris");
            _controls = TetrisControls.Load();
            _random = new Random();

            // Set up timers based on difficulty
            var dropInterval = difficulty switch
            {
                "Beginner" => 1000,
                "Intermediate" => 750,
                "Expert" => 500,
                _ => 1000
            };

            _gameTimer = new System.Windows.Forms.Timer { Interval = 16 }; // ~60 FPS for input handling
            _dropTimer = new System.Windows.Forms.Timer { Interval = dropInterval };

            _gameTimer.Tick += GameTimer_Tick;
            _dropTimer.Tick += DropTimer_Tick;

            InitializeComponent();
            SetupDoubleBuffering();
            StartNewGame();
        }

        private void InitializeComponent()
        {
            Text = "Tetris";
            Size = new Size(BoardWidth + 200, BoardHeight + 100);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            KeyPreview = true;

            // Main layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, BoardWidth + 20));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Game panel (left side)
            _gamePanel = new DoubleBufferedPanel
            {
                Size = new Size(BoardWidth, BoardHeight),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black
            };
            _gamePanel.Paint += GamePanel_Paint;

            var gamePanelContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };
            gamePanelContainer.Controls.Add(_gamePanel);

            // Side panel (right side)
            _sidePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            CreateSidePanel();

            mainLayout.Controls.Add(gamePanelContainer, 0, 0);
            mainLayout.Controls.Add(_sidePanel, 1, 0);

            Controls.Add(mainLayout);

            // Event handlers
            KeyDown += TetrisGameForm_KeyDown;
            FormClosing += TetrisGameForm_FormClosing;
        }

        private void CreateSidePanel()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 8,
                ColumnCount = 1
            };

            // Score display
            _scoreLabel = new Label
            {
                Text = "Score: 0",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _levelLabel = new Label
            {
                Text = "Level: 1",
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _linesLabel = new Label
            {
                Text = "Lines: 0",
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Next piece panel
            var nextLabel = new Label
            {
                Text = "Next:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _nextPiecePanel = new Panel
            {
                Size = new Size(100, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black
            };
            _nextPiecePanel.Paint += NextPiecePanel_Paint;

            // Held piece panel
            var heldLabel = new Label
            {
                Text = "Hold:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _heldPiecePanel = new Panel
            {
                Size = new Size(100, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.Black
            };
            _heldPiecePanel.Paint += HeldPiecePanel_Paint;

            // Control buttons
            _pauseButton = new Button
            {
                Text = "Pause",
                Dock = DockStyle.Fill,
                Height = 30
            };
            _pauseButton.Click += PauseButton_Click;

            _controlsButton = new Button
            {
                Text = "Controls",
                Dock = DockStyle.Fill,
                Height = 30
            };
            _controlsButton.Click += ControlsButton_Click;

            // Add to layout
            layout.Controls.Add(_scoreLabel, 0, 0);
            layout.Controls.Add(_levelLabel, 0, 1);
            layout.Controls.Add(_linesLabel, 0, 2);
            layout.Controls.Add(nextLabel, 0, 3);
            layout.Controls.Add(_nextPiecePanel, 0, 4);
            layout.Controls.Add(heldLabel, 0, 5);
            layout.Controls.Add(_heldPiecePanel, 0, 6);
            layout.Controls.Add(_pauseButton, 0, 7);
            layout.Controls.Add(_controlsButton, 0, 8);

            // Set row styles
            for (int i = 0; i < 9; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            _sidePanel.Controls.Add(layout);
        }

        private void SetupDoubleBuffering()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
        }

        private void StartNewGame()
        {
            _board.Clear();
            _statistics.StartNewGame();
            _level = 1;
            _statistics.UpdateLevel(_level);
            _linesUntilNextLevel = 10;
            _gameStartTime = DateTime.Now;
            _canHold = true;
            _heldPiece = null;
            _isPaused = false;

            SpawnNewPiece();
            SpawnNextPiece();

            _gameTimer.Start();
            _dropTimer.Start();

            UpdateUI();
        }

        private void SpawnNewPiece()
        {
            _currentPiece = _nextPiece ?? GenerateRandomPiece();
            SpawnNextPiece();
            _canHold = true;

            if (!_board.IsValidPosition(_currentPiece))
            {
                GameOver();
            }
        }

        private void SpawnNextPiece()
        {
            _nextPiece = GenerateRandomPiece();
        }

        private Tetromino GenerateRandomPiece()
        {
            var types = Enum.GetValues<TetrominoType>();
            var randomType = types[_random.Next(types.Length)];
            return new Tetromino(randomType);
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isPaused)
            {
                // Only invalidate the game panel, not the entire form
                _gamePanel.Invalidate();
                _nextPiecePanel.Invalidate();
                _heldPiecePanel.Invalidate();
                UpdateUI();
            }
        }

        private void DropTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isPaused && _currentPiece != null)
            {
                MovePiece(0, 1);
            }
        }

        private void TetrisGameForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_isPaused || _currentPiece == null) return;

            if (e.KeyCode == _controls.MoveLeft)
            {
                MovePiece(-1, 0);
            }
            else if (e.KeyCode == _controls.MoveRight)
            {
                MovePiece(1, 0);
            }
            else if (e.KeyCode == _controls.MoveDown)
            {
                MovePiece(0, 1);
            }
            else if (e.KeyCode == _controls.HardDrop)
            {
                HardDrop();
            }
            else if (e.KeyCode == _controls.RotateClockwise)
            {
                RotatePiece(true);
            }
            else if (e.KeyCode == _controls.RotateCounterClockwise)
            {
                RotatePiece(false);
            }
            else if (e.KeyCode == _controls.Hold)
            {
                HoldPiece();
            }
            else if (e.KeyCode == _controls.Pause)
            {
                TogglePause();
            }

            e.Handled = true;
        }

        private void MovePiece(int deltaX, int deltaY)
        {
            if (_currentPiece == null) return;

            var newPosition = new Point(_currentPiece.Position.X + deltaX, _currentPiece.Position.Y + deltaY);
            var testPiece = _currentPiece.Clone();
            testPiece.Position = newPosition;

            if (_board.IsValidPosition(testPiece))
            {
                _currentPiece.Position = newPosition;
            }
            else if (deltaY > 0) // Piece hit bottom or another piece
            {
                PlacePiece();
            }
        }

        private void HardDrop()
        {
            if (_currentPiece == null) return;

            while (_board.IsValidPosition(_currentPiece))
            {
                _currentPiece.Position = new Point(_currentPiece.Position.X, _currentPiece.Position.Y + 1);
            }

            // Move back one position
            _currentPiece.Position = new Point(_currentPiece.Position.X, _currentPiece.Position.Y - 1);
            PlacePiece();
        }

        private void RotatePiece(bool clockwise)
        {
            if (_currentPiece == null) return;

            var testPiece = _currentPiece.Clone();
            if (clockwise)
                testPiece.RotateClockwise();
            else
                testPiece.RotateCounterClockwise();

            if (_board.IsValidPosition(testPiece))
            {
                if (clockwise)
                    _currentPiece.RotateClockwise();
                else
                    _currentPiece.RotateCounterClockwise();
            }
        }

        private void HoldPiece()
        {
            if (_currentPiece == null || !_canHold) return;

            if (_heldPiece == null)
            {
                _heldPiece = new Tetromino(_currentPiece.Type);
                SpawnNewPiece();
            }
            else
            {
                var temp = _heldPiece;
                _heldPiece = new Tetromino(_currentPiece.Type);
                _currentPiece = new Tetromino(temp.Type);
            }

            _canHold = false;
        }

        private void PlacePiece()
        {
            if (_currentPiece == null) return;

            _board.PlaceTetromino(_currentPiece);
            _statistics.UpdateTetrominoes(1);

            var linesCleared = _board.ClearLines();
            if (linesCleared > 0)
            {
                _statistics.UpdateLinesCleared(linesCleared);
                _linesUntilNextLevel -= linesCleared;

                if (_linesUntilNextLevel <= 0)
                {
                    _level++;
                    _statistics.UpdateLevel(_level);
                    _linesUntilNextLevel = 10;
                    
                    // Increase speed as level progresses
                    var newInterval = Math.Max(50, _dropTimer.Interval - 50);
                    _dropTimer.Interval = newInterval;
                }
            }

            if (_board.IsGameOver())
            {
                GameOver();
            }
            else
            {
                SpawnNewPiece();
            }
        }

        /// <summary>
        /// Handles game over state, updates statistics, and fires score changed event
        /// for the centralized high score system to process.
        /// </summary>
        private void GameOver()
        {
            _gameTimer.Stop();
            _dropTimer.Stop();

            var currentScore = _statistics.Score;
            
            // End the game session first to update statistics
            _statistics.EndGame(false);

            // Fire ScoreChanged event for any score > 0 to allow the centralized 
            // high score system to determine if it's worth recording
            if (currentScore > 0)
            {
                ScoreChanged?.Invoke(this, new ScoreChangedEventArgs(0, currentScore));
            }

            var result = MessageBox.Show(
                $"Game Over!\n\nFinal Score: {_statistics.Score:N0}\nLines Cleared: {_statistics.LinesCleared}\nLevel Reached: {_level}\n\nPlay again?",
                "Game Over",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                StartNewGame();
            }
            else
            {
                Hide();
                ExitRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;
            _pauseButton.Text = _isPaused ? "Resume" : "Pause";

            if (_isPaused)
            {
                _gameTimer.Stop();
                _dropTimer.Stop();
            }
            else
            {
                _gameTimer.Start();
                _dropTimer.Start();
            }
        }

        private void PauseButton_Click(object? sender, EventArgs e)
        {
            TogglePause();
        }

        private void ControlsButton_Click(object? sender, EventArgs e)
        {
            using var controlsForm = new TetrisControlsForm(_controls);
            if (controlsForm.ShowDialog() == DialogResult.OK)
            {
                _controls = controlsForm.Controls;
            }
        }

        private void UpdateUI()
        {
            _scoreLabel.Text = $"Score: {_statistics.Score:N0}";
            _levelLabel.Text = $"Level: {_level}";
            _linesLabel.Text = $"Lines: {_statistics.LinesCleared}";
        }

        private void GamePanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);

            // Draw placed blocks
            var grid = _board.GetVisibleGrid();
            for (int row = 0; row < TetrisBoard.Height; row++)
            {
                for (int col = 0; col < TetrisBoard.Width; col++)
                {
                    var color = grid[row, col];
                    if (color != Color.Empty)
                    {
                        DrawBlock(g, col, row, color);
                    }
                }
            }

            // Draw current piece
            if (_currentPiece != null && !_isPaused)
            {
                foreach (var block in _currentPiece.GetBlockPositions())
                {
                    if (block.Y >= TetrisBoard.BufferHeight) // Only draw visible part
                    {
                        DrawBlock(g, block.X, block.Y - TetrisBoard.BufferHeight, _currentPiece.Color);
                    }
                }
            }

            // Draw grid lines
            using var gridPen = new Pen(Color.DarkGray, 1);
            for (int x = 0; x <= TetrisBoard.Width; x++)
            {
                g.DrawLine(gridPen, x * BlockSize, 0, x * BlockSize, BoardHeight);
            }
            for (int y = 0; y <= TetrisBoard.Height; y++)
            {
                g.DrawLine(gridPen, 0, y * BlockSize, BoardWidth, y * BlockSize);
            }

            // Draw pause overlay
            if (_isPaused)
            {
                using var pauseBrush = new SolidBrush(Color.FromArgb(128, Color.Black));
                g.FillRectangle(pauseBrush, 0, 0, BoardWidth, BoardHeight);
                
                var pauseText = "PAUSED";
                using var font = new Font("Segoe UI", 24, FontStyle.Bold);
                var textSize = g.MeasureString(pauseText, font);
                var x = (BoardWidth - textSize.Width) / 2;
                var y = (BoardHeight - textSize.Height) / 2;
                
                g.DrawString(pauseText, font, Brushes.White, x, y);
            }
        }

        private void NextPiecePanel_Paint(object? sender, PaintEventArgs e)
        {
            if (_nextPiece == null) return;

            var g = e.Graphics;
            g.Clear(Color.Black);

            var offsetX = (_nextPiecePanel.Width - _nextPiece.Shape.GetLength(1) * BlockSize) / 2;
            var offsetY = (_nextPiecePanel.Height - _nextPiece.Shape.GetLength(0) * BlockSize) / 2;

            for (int row = 0; row < _nextPiece.Shape.GetLength(0); row++)
            {
                for (int col = 0; col < _nextPiece.Shape.GetLength(1); col++)
                {
                    if (_nextPiece.Shape[row, col])
                    {
                        var x = offsetX + col * BlockSize;
                        var y = offsetY + row * BlockSize;
                        using var brush = new SolidBrush(_nextPiece.Color);
                        g.FillRectangle(brush, x, y, BlockSize, BlockSize);
                        g.DrawRectangle(Pens.Black, x, y, BlockSize, BlockSize);
                    }
                }
            }
        }

        private void HeldPiecePanel_Paint(object? sender, PaintEventArgs e)
        {
            if (_heldPiece == null) return;

            var g = e.Graphics;
            g.Clear(Color.Black);

            var offsetX = (_heldPiecePanel.Width - _heldPiece.Shape.GetLength(1) * BlockSize) / 2;
            var offsetY = (_heldPiecePanel.Height - _heldPiece.Shape.GetLength(0) * BlockSize) / 2;

            for (int row = 0; row < _heldPiece.Shape.GetLength(0); row++)
            {
                for (int col = 0; col < _heldPiece.Shape.GetLength(1); col++)
                {
                    if (_heldPiece.Shape[row, col])
                    {
                        var x = offsetX + col * BlockSize;
                        var y = offsetY + row * BlockSize;
                        using var brush = new SolidBrush(_heldPiece.Color);
                        g.FillRectangle(brush, x, y, BlockSize, BlockSize);
                        g.DrawRectangle(Pens.Black, x, y, BlockSize, BlockSize);
                    }
                }
            }
        }

        private void DrawBlock(Graphics g, int col, int row, Color color)
        {
            var x = col * BlockSize;
            var y = row * BlockSize;
            
            using var brush = new SolidBrush(color);
            g.FillRectangle(brush, x, y, BlockSize, BlockSize);
            g.DrawRectangle(Pens.Black, x, y, BlockSize, BlockSize);
        }

        private void TetrisGameForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _gameTimer?.Stop();
            _dropTimer?.Stop();
            e.Cancel = true;
            Hide();
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gameTimer?.Stop();
                _dropTimer?.Stop();
                _gameTimer?.Dispose();
                _dropTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
