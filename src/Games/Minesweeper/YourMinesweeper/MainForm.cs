using System;
using System.Drawing;
using System.Windows.Forms;
using GameCore.Interfaces;

namespace Minesweeper.YourMinesweeper
{
    public partial class MainForm : Form
    {
        private GameEngine _gameEngine = null!;
        private Button[,] _cellButtons = null!;
        private MenuStrip _menuStrip = null!;
        private ToolStripMenuItem _gameMenu = null!;
        private ToolStripMenuItem _newGameMenuItem = null!;
        private ToolStripMenuItem _beginnerMenuItem = null!;
        private ToolStripMenuItem _intermediateMenuItem = null!;
        private ToolStripMenuItem _expertMenuItem = null!;
        private ToolStripSeparator _separator = null!;
        private ToolStripMenuItem _exitMenuItem = null!;
        
        private Panel _statusPanel = null!;
        private Label _mineCountLabel = null!;
        private Button _faceButton = null!;
        private Label _timerLabel = null!;
        
        private System.Windows.Forms.Timer _gameTimer = null!;
        private int _secondsElapsed;
        private Difficulty _currentDifficulty;

        // Reference to the adapter for high score/time recording
        private Minesweeper.MinesweeperGameAdapter? _adapter;
        private IHighScoreService _highScoreService;

        // Public properties for launcher integration
        public GameEngine GameEngine => _gameEngine;
        public int SecondsElapsed => _secondsElapsed;

        // Constructor for launcher integration
        public MainForm(GameSettings settings, Minesweeper.MinesweeperGameAdapter? adapter = null, IHighScoreService? highScoreService = null)
        {
            _currentDifficulty = GetDifficultyFromSettings(settings);
            _adapter = adapter;
            _highScoreService = highScoreService ?? new StandaloneHighScoreService();
            InitializeComponent();
            InitializeGameWithSettings(settings);
        }

        // Default constructor
        public MainForm() : this(new GameSettings(), null, null) { }

        private Difficulty GetDifficultyFromSettings(GameSettings settings)
        {
            if (settings.Rows == 9 && settings.Columns == 9 && settings.MineCount == 10)
                return Difficulty.Beginner;
            if (settings.Rows == 16 && settings.Columns == 16 && settings.MineCount == 40)
                return Difficulty.Intermediate;
            if (settings.Rows == 16 && settings.Columns == 30 && settings.MineCount == 99)
                return Difficulty.Expert;
            return Difficulty.Beginner; // Default
        }

        private void InitializeComponent()
        {
            // Form properties
            Text = "Minesweeper";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Silver;

            // Create menu
            CreateMenu();

            // Create status panel
            CreateStatusPanel();

            // Initialize timer
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 1000; // 1 second
            _gameTimer.Tick += GameTimer_Tick;
        }

        private void CreateMenu()
        {
            _menuStrip = new MenuStrip();
            _gameMenu = new ToolStripMenuItem("Game");
            
            _newGameMenuItem = new ToolStripMenuItem("New Game", null, NewGame_Click);
            _newGameMenuItem.ShortcutKeys = Keys.F2;
            
            _beginnerMenuItem = new ToolStripMenuItem("Beginner", null, (s, e) => ChangeDifficulty(Difficulty.Beginner));
            _intermediateMenuItem = new ToolStripMenuItem("Intermediate", null, (s, e) => ChangeDifficulty(Difficulty.Intermediate));
            _expertMenuItem = new ToolStripMenuItem("Expert", null, (s, e) => ChangeDifficulty(Difficulty.Expert));
            
            _separator = new ToolStripSeparator();
            _exitMenuItem = new ToolStripMenuItem("Exit", null, (s, e) => Close());

            _gameMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                _newGameMenuItem,
                _separator,
                _beginnerMenuItem,
                _intermediateMenuItem,
                _expertMenuItem,
                new ToolStripSeparator(),
                _exitMenuItem
            });

            _menuStrip.Items.Add(_gameMenu);
            MainMenuStrip = _menuStrip;
            Controls.Add(_menuStrip);
        }

        private void CreateStatusPanel()
        {
            _statusPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.Silver,
                BorderStyle = BorderStyle.Fixed3D
            };

            // Mine count display
            _mineCountLabel = new Label
            {
                Text = "010",
                Font = new Font("Courier New", 14, FontStyle.Bold),
                ForeColor = Color.Red,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(50, 25),
                Location = new Point(10, 12)
            };

            // Face button (smiley)
            _faceButton = new Button
            {
                Text = "🙂",
                Font = new Font("Segoe UI Emoji", 14),
                Size = new Size(35, 35),
                FlatStyle = FlatStyle.Popup,
                BackColor = Color.Silver
            };
            _faceButton.Click += FaceButton_Click;

            // Timer display
            _timerLabel = new Label
            {
                Text = "000",
                Font = new Font("Courier New", 14, FontStyle.Bold),
                ForeColor = Color.Red,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(50, 25)
            };

            _statusPanel.Controls.AddRange(new Control[] { _mineCountLabel, _faceButton, _timerLabel });
            Controls.Add(_statusPanel);

            #if DEBUG
            // Add a Simulate Win button for debugging
            var simulateWinButton = new Button
            {
                Text = "Simulate Win",
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                Size = new Size(90, 25),
                Location = new Point(120, 12),
                BackColor = Color.LightGreen
            };
            simulateWinButton.Click += (s, e) =>
            {
                _gameEngine.SimulateWinForDebug();
            };
            _statusPanel.Controls.Add(simulateWinButton);
            #endif
        }

        private void InitializeGameWithSettings(GameSettings settings)
        {
            _gameEngine = new GameEngine(settings);
            _gameEngine.GameStateChanged += GameEngine_GameStateChanged;
            CreateGameGrid();
            UpdateStatusPanel();
            ResizeForm();
            
            _secondsElapsed = 0;
            _gameTimer.Stop();
            UpdateTimer();
            UpdateMineCount();
            _faceButton.Text = "🙂";
        }

        private void InitializeGame()
        {
            var settings = GameSettings.GetSettings(_currentDifficulty);
            InitializeGameWithSettings(settings);
        }

        private void CreateGameGrid()
        {
            // Remove existing buttons
            if (_cellButtons != null)
            {
                foreach (var button in _cellButtons)
                {
                    Controls.Remove(button);
                    button?.Dispose();
                }
            }

            _cellButtons = new Button[_gameEngine.Rows, _gameEngine.Columns];
            
            const int buttonSize = 30;
            const int buttonSpacing = 1; // Small gap between buttons
            const int startX = 24; // Match the margin used in ResizeForm
            int startY = _statusPanel.Bottom + 40; // Extra spacing to clear menu and status panel

            for (int row = 0; row < _gameEngine.Rows; row++)
            {
                for (int col = 0; col < _gameEngine.Columns; col++)
                {
                    var button = new Button
                    {
                        Size = new Size(buttonSize, buttonSize),
                        Location = new Point(startX + col * (buttonSize + buttonSpacing), startY + row * (buttonSize + buttonSpacing)),
                        Font = new Font("Arial", 9, FontStyle.Bold),
                        BackColor = Color.Silver,
                        FlatStyle = FlatStyle.Popup,
                        Tag = new Point(row, col),
                        Text = ""
                    };

                    button.MouseDown += CellButton_MouseDown;
                    button.MouseUp += CellButton_MouseUp;
                    
                    _cellButtons[row, col] = button;
                    Controls.Add(button);
                }
            }
        }

        private void ResizeForm()
        {
            const int buttonSize = 30;
            const int buttonSpacing = 1;
            const int margin = 24;
            const int gridTopMargin = 40; // Extra space between status panel and grid
            const int gridBottomMargin = 12; // Match left/right margin for symmetry
            
            int gridWidth = _gameEngine.Columns * buttonSize + (_gameEngine.Columns - 1) * buttonSpacing;
            int gridHeight = _gameEngine.Rows * buttonSize + (_gameEngine.Rows - 1) * buttonSpacing;
            
            int width = gridWidth + margin * 2;
            int height = _menuStrip.Height + _statusPanel.Height + gridTopMargin + gridHeight + gridBottomMargin;
            
            ClientSize = new Size(width, height);
            
            // Center face button
            _faceButton.Location = new Point((width - _faceButton.Width) / 2, 8);
            
            // Position timer on the right
            _timerLabel.Location = new Point(width - _timerLabel.Width - 10, 12);
        }

        private void CellButton_MouseDown(object? sender, MouseEventArgs e)
        {
            if (_gameEngine.State == GameState.Lost || _gameEngine.State == GameState.Won)
                return;

            var button = (Button)sender!;
            var position = (Point)button.Tag!;

            if (e.Button == MouseButtons.Left)
            {
                _faceButton.Text = "😮";
                if (_gameEngine.State == GameState.NotStarted)
                {
                    _gameTimer.Start();
                }
            }
        }

        private void CellButton_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_gameEngine.State == GameState.Lost || _gameEngine.State == GameState.Won)
                return;

            var button = (Button)sender!;
            var position = (Point)button.Tag!;
            int row = position.X;
            int col = position.Y;

            if (e.Button == MouseButtons.Left)
            {
                _gameEngine.LeftClick(row, col);
                _faceButton.Text = "🙂";
            }
            else if (e.Button == MouseButtons.Right)
            {
                _gameEngine.RightClick(row, col);
            }

            UpdateGameDisplay();
            UpdateMineCount();
        }

        private void UpdateGameDisplay()
        {
            for (int row = 0; row < _gameEngine.Rows; row++)
            {
                for (int col = 0; col < _gameEngine.Columns; col++)
                {
                    var cell = _gameEngine.GetCell(row, col);
                    var button = _cellButtons[row, col];
                    if (cell == null)
                    {
                        button.Text = "";
                        button.BackColor = Color.Silver;
                        button.Enabled = false;
                        continue;
                    }

                    if (cell.IsRevealed)
                    {
                        button.FlatStyle = FlatStyle.Flat;
                        button.BackColor = Color.LightGray;
                        button.Enabled = false;

                        if (cell.IsMine)
                        {
                            button.Text = "💣";
                            button.BackColor = Color.Red;
                        }
                        else if (cell.AdjacentMines > 0)
                        {
                            button.Text = cell.AdjacentMines.ToString();
                            button.ForeColor = GetNumberColor(cell.AdjacentMines);
                        }
                        else
                        {
                            button.Text = "";
                        }
                    }
                    else if (cell.IsFlagged)
                    {
                        button.Text = "🚩";
                        button.BackColor = Color.Silver;
                    }
                    else
                    {
                        button.Text = "";
                        button.BackColor = Color.Silver;
                        button.Enabled = true;
                    }
                }
            }
        }

        private Color GetNumberColor(int number)
        {
            return number switch
            {
                1 => Color.Blue,
                2 => Color.Green,
                3 => Color.Red,
                4 => Color.Purple,
                5 => Color.Maroon,
                6 => Color.Turquoise,
                7 => Color.Black,
                8 => Color.Gray,
                _ => Color.Black
            };
        }

        private void GameEngine_GameStateChanged(object? sender, GameStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case GameState.Playing:
                    _faceButton.Text = "🙂";
                    break;
                case GameState.Won:
                    _faceButton.Text = "😎";
                    _gameTimer.Stop();
                    // Only show name prompt in standalone mode (not when running under launcher)
                    if (IsStandaloneMode())
                    {
                        var playerName = _highScoreService.PromptForPlayerName();
                        if (string.IsNullOrWhiteSpace(playerName)) break;
                        var entry = new GameCore.Models.ScoreEntry
                        {
                            GameId = "minesweeper",
                            PlayerName = playerName,
                            Score = Math.Max(0, 999 - _secondsElapsed),
                            Difficulty = _currentDifficulty.ToString(),
                            AchievedAt = DateTime.UtcNow
                        };
                        try
                        {
                            _highScoreService.SaveScore(entry);
                            MessageBox.Show($"High score saved!\nName: {playerName}\nTime: {_secondsElapsed} seconds\nScore: {entry.Score}", "High Score", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Could not save high score: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else if (_adapter != null)
                    {
                        _adapter.OnGameWon();
                    }
                    break;
                case GameState.Lost:
                    _faceButton.Text = "😵";
                    _gameTimer.Stop();
                    if (_adapter != null)
                        _adapter.OnGameLost();
                    UpdateGameDisplay(); // Show all mines
                    break;
            }
        }

        // Helper to detect if running in standalone mode (not under launcher)
        private bool IsStandaloneMode()
        {
            // Standalone mode: adapter is null
            return _adapter == null;
        }

        private void UpdateStatusPanel()
        {
            UpdateMineCount();
            UpdateTimer();
        }

        private void UpdateMineCount()
        {
            int remainingMines = _gameEngine.MineCount - _gameEngine.FlaggedMines;
            _mineCountLabel.Text = remainingMines.ToString("000");
        }

        private void UpdateTimer()
        {
            _timerLabel.Text = Math.Min(_secondsElapsed, 999).ToString("000");
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            _secondsElapsed++;
            UpdateTimer();
        }

        private void FaceButton_Click(object? sender, EventArgs e)
        {
            InitializeGame();
        }

        private void NewGame_Click(object? sender, EventArgs e)
        {
            InitializeGame();
        }

        private void ChangeDifficulty(Difficulty difficulty)
        {
            _currentDifficulty = difficulty;
            
            // Update menu check marks
            _beginnerMenuItem.Checked = difficulty == Difficulty.Beginner;
            _intermediateMenuItem.Checked = difficulty == Difficulty.Intermediate;
            _expertMenuItem.Checked = difficulty == Difficulty.Expert;
            
            InitializeGame();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _beginnerMenuItem.Checked = true; // Default to beginner
        }
    }
}
