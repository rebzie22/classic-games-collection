using GameCore.Interfaces;
using GameCore.Models;
using GameLauncher.Services;

namespace GameLauncher.Forms
{
    partial class MainForm : Form
    {
        private readonly GameDiscoveryService _gameDiscovery;
        private readonly ThemeManager _themeManager;
        private readonly IDataService _dataService;
        private readonly IScoreService _scoreService;
        private readonly ISettingsService _settingsService;
        private Panel _gamesPanel = null!;
        private Label _titleLabel = null!;
        private StatusStrip _statusStrip = null!;
        private ToolStripStatusLabel _statusLabel = null!;
        private MenuStrip _menuStrip = null!;
        
        public MainForm()
        {
            _gameDiscovery = new GameDiscoveryService();
            _themeManager = ThemeManager.Instance;
            _dataService = new JsonDataService();
            _scoreService = new ScoreService(_dataService);
            _settingsService = new SettingsService();
            
            InitializeComponent();
            InitializeAsync();
            
            // Subscribe to theme changes
            _themeManager.ThemeChanged += ApplyTheme;
            
            // Subscribe to score events
            _scoreService.NewHighScore += OnNewHighScore;
            
            // Theme will be loaded in OnLoad, other settings loaded async
        }
        
        private async void InitializeAsync()
        {
            _statusLabel.Text = "Loading games...";
            
            var success = await _gameDiscovery.DiscoverGamesAsync();
            if (success)
            {
                PopulateGames();
                _statusLabel.Text = $"Found {_gameDiscovery.AvailableGames.Count} games";
            }
            else
            {
                _statusLabel.Text = "Failed to load games";
                MessageBox.Show("Failed to discover games. Please check the installation.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void InitializeComponent()
        {
            // Main form properties
            Text = "Classic Games Collection";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(600, 400);
            BackColor = Color.FromArgb(240, 240, 240);
            
            // Create menu strip
            _menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("Exit", null, (s, e) => Close()));
            
            var viewMenu = new ToolStripMenuItem("View");
            viewMenu.DropDownItems.Add(new ToolStripMenuItem("High Scores", null, ShowHighScores));
            viewMenu.DropDownItems.Add(new ToolStripSeparator());
            viewMenu.DropDownItems.Add(new ToolStripMenuItem("Settings", null, ShowSettings));
            
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add(new ToolStripMenuItem("About", null, ShowAbout));
            
            _menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, viewMenu, helpMenu });
            
            // Create title panel with dark mode toggle
            var titlePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80
            };
            
            // Create title label
            _titleLabel = new Label
            {
                Text = "Classic Games Collection",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 0, 0, 0)
            };
            
            titlePanel.Controls.Add(_titleLabel);
            
            // Create games panel
            _gamesPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };
            
            // Create status strip
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Ready");
            _statusStrip.Items.Add(_statusLabel);
            
            // Add controls to form
            Controls.Add(_gamesPanel);
            Controls.Add(titlePanel);
            Controls.Add(_statusStrip);
            Controls.Add(_menuStrip);
            MainMenuStrip = _menuStrip;
        }
        
        private void PopulateGames()
        {
            _gamesPanel.Controls.Clear();
            
            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(10)
            };
            
            // Subscribe to resize events to update card sizes dynamically with debouncing
            var resizeTimer = new System.Windows.Forms.Timer { Interval = 100 };
            resizeTimer.Tick += (s, e) => {
                resizeTimer.Stop();
                UpdateGameCardSizes(flowPanel);
            };
            
            flowPanel.Resize += (s, e) => {
                resizeTimer.Stop();
                resizeTimer.Start();
            };
            
            foreach (var game in _gameDiscovery.AvailableGames)
            {
                var gameCard = CreateGameCard(game);
                flowPanel.Controls.Add(gameCard);
            }
            
            _gamesPanel.Controls.Add(flowPanel);
            
            // Initial card size calculation
            flowPanel.BeginInvoke(new Action(() => UpdateGameCardSizes(flowPanel)));
        }
        
        private void UpdateGameCardSizes(FlowLayoutPanel flowPanel)
        {
            if (flowPanel.Width <= 0) return;
            
            // Calculate optimal card size based on available space
            var availableWidth = flowPanel.ClientSize.Width - flowPanel.Padding.Horizontal;
            var minCardWidth = 180;
            var maxCardWidth = 280;
            var cardMargin = 20; // Total margin (10 per side)
            
            // Calculate how many cards can fit per row
            var cardsPerRow = Math.Max(1, availableWidth / (minCardWidth + cardMargin));
            var cardWidth = Math.Min(maxCardWidth, (availableWidth - (cardsPerRow * cardMargin)) / cardsPerRow);
            var cardHeight = (int)(cardWidth * 1.4); // Maintain aspect ratio
            
            // Update all game cards
            foreach (Control control in flowPanel.Controls)
            {
                if (control is Panel gameCard)
                {
                    gameCard.Size = new Size((int)cardWidth, cardHeight);
                    UpdateGameCardLayout(gameCard, (int)cardWidth, cardHeight);
                }
            }
        }
        
        private void UpdateGameCardLayout(Panel card, int cardWidth, int cardHeight)
        {
            var padding = 10;
            var iconHeight = (int)(cardHeight * 0.45); // 45% for icon
            var titleHeight = 25;
            var buttonHeight = 30;
            var descriptionHeight = cardHeight - iconHeight - titleHeight - buttonHeight - (padding * 4);
            
            // Update icon panel
            if (card.Controls.Count > 0 && card.Controls[0] is Panel iconPanel)
            {
                iconPanel.Size = new Size(cardWidth - (padding * 2), iconHeight);
                iconPanel.Location = new Point(padding, padding);
            }
            
            // Update title label
            if (card.Controls.Count > 1 && card.Controls[1] is Label titleLabel)
            {
                titleLabel.Size = new Size(cardWidth - (padding * 2), titleHeight);
                titleLabel.Location = new Point(padding, iconHeight + padding);
            }
            
            // Update description label
            if (card.Controls.Count > 2 && card.Controls[2] is Label descLabel)
            {
                descLabel.Size = new Size(cardWidth - (padding * 2), descriptionHeight);
                descLabel.Location = new Point(padding, iconHeight + titleHeight + padding);
            }
            
            // Update play button
            if (card.Controls.Count > 3 && card.Controls[3] is Button playButton)
            {
                var buttonWidth = Math.Min(100, cardWidth - (padding * 2));
                playButton.Size = new Size(buttonWidth, buttonHeight);
                playButton.Location = new Point((cardWidth - buttonWidth) / 2, 
                                              cardHeight - buttonHeight - padding);
            }
        }
        
        private Panel CreateGameCard(IGame game)
        {
            var card = new Panel
            {
                Size = new Size(200, 280), // Initial size, will be updated by resize logic
                BackColor = _themeManager.CardBackColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                Cursor = Cursors.Hand
            };
            
            // Game icon panel
            var iconPanel = new Panel
            {
                Size = new Size(180, 100), // Initial size, will be updated
                Location = new Point(10, 10),
                BackColor = _themeManager.PanelColor
            };
            
            if (game.Icon != null)
            {
                var pictureBox = new PictureBox
                {
                    Image = game.Icon,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Dock = DockStyle.Fill
                };
                iconPanel.Controls.Add(pictureBox);
            }
            else
            {
                var placeholderLabel = new Label
                {
                    Text = game.DisplayName[0].ToString(),
                    Font = new Font("Segoe UI", 36, FontStyle.Bold),
                    ForeColor = _themeManager.TextColor,
                    BackColor = _themeManager.PanelColor,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                iconPanel.Controls.Add(placeholderLabel);
            }
            
            card.Controls.Add(iconPanel);
            
            // Game title
            var titleLabel = new Label
            {
                Text = game.DisplayName,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = _themeManager.TextColor,
                BackColor = _themeManager.CardBackColor,
                Location = new Point(10, 120), // Initial position, will be updated
                Size = new Size(180, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            card.Controls.Add(titleLabel);
            
            // Game description
            var descLabel = new Label
            {
                Text = game.Description,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(_themeManager.IsDarkMode ? 160 : 100, 
                                         _themeManager.IsDarkMode ? 160 : 100, 
                                         _themeManager.IsDarkMode ? 160 : 100),
                BackColor = _themeManager.CardBackColor,
                Location = new Point(10, 155), // Initial position, will be updated
                Size = new Size(180, 60), // Initial size, will be updated
                TextAlign = ContentAlignment.TopCenter
            };
            
            card.Controls.Add(descLabel);
            
            // Play button - will be positioned dynamically
            var playButton = new Button
            {
                Text = "Play",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(100, 30),
                Location = new Point(50, 225), // Initial position, will be updated
                BackColor = _themeManager.ButtonColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            playButton.FlatAppearance.BorderSize = 0;
            playButton.Click += (s, e) => LaunchGame(game);
            
            // Add hover effects for button
            playButton.MouseEnter += (s, e) => playButton.BackColor = _themeManager.ButtonHoverColor;
            playButton.MouseLeave += (s, e) => playButton.BackColor = _themeManager.ButtonColor;
            
            card.Controls.Add(playButton);
            
            // Add hover effects for card
            var normalCardColor = _themeManager.CardBackColor;
            var hoverCardColor = _themeManager.IsDarkMode ? 
                Color.FromArgb(65, 65, 65) : Color.FromArgb(248, 248, 248);
                
            card.MouseEnter += (s, e) => card.BackColor = hoverCardColor;
            card.MouseLeave += (s, e) => card.BackColor = normalCardColor;
            card.Click += (s, e) => LaunchGame(game);
            
            return card;
        }
        
        private async void LaunchGame(IGame gameTemplate)
        {
            try
            {
                // Create a new instance of the game
                var game = _gameDiscovery.CreateGameInstance(gameTemplate.GameId);
                if (game == null)
                {
                    MessageBox.Show($"Failed to create instance of {gameTemplate.DisplayName}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _statusLabel.Text = $"Launching {game.DisplayName}...";

                // Get settings from the new settings service
                var userSettings = await _settingsService.GetSettingsAsync();
                var gameSettings = await _settingsService.GetGameSettingsAsync(game.GameId);

                // Use the default difficulty from user settings if no game-specific setting
                var difficulty = !string.IsNullOrEmpty(gameSettings.PreferredDifficulty)
                    ? gameSettings.PreferredDifficulty
                    : userSettings.DefaultDifficulty;

                // Map legacy difficulty names to current standard
                difficulty = difficulty switch
                {
                    "Easy" => "Beginner",
                    "Normal" => "Intermediate",
                    "Hard" => "Expert",
                    _ => difficulty
                };

                // Initialize the game with settings
                var settings = GameCore.Models.GameSettings.Load(game.GameId);
                game.Initialize(settings);

                // --- Robust event relay for Minesweeper ---
                if (game is Minesweeper.MinesweeperGameAdapter minesweeperAdapter)
                {
                    minesweeperAdapter.SetPreferredDifficulty(difficulty);
                    // Ensure event is always hooked (defensive)
                    minesweeperAdapter.ScoreChanged -= OnGameScoreChanged;
                    minesweeperAdapter.ScoreChanged += OnGameScoreChanged;
                }
                else
                {
                    // Subscribe to score changes for all other games
                    game.ScoreChanged -= OnGameScoreChanged;
                    game.ScoreChanged += OnGameScoreChanged;
                }

                // Start the game with the specified difficulty
                game.StartNew(difficulty);

                // Get the game window
                var gameWindow = game.GetGameWindow();
                if (gameWindow != null)
                {
                    // Set this form as the owner to prevent application shutdown when game closes
                    if (gameWindow.Owner == null)
                    {
                        gameWindow.Owner = this;
                    }

                    gameWindow.FormClosed += (s, e) =>
                    {
                        _statusLabel.Text = "Ready";
                        if (game is Minesweeper.MinesweeperGameAdapter msAdapter)
                        {
                            msAdapter.ScoreChanged -= OnGameScoreChanged;
                        }
                        else
                        {
                            game.ScoreChanged -= OnGameScoreChanged;
                        }
                        game.Dispose();
                    };

                    _statusLabel.Text = $"{game.DisplayName} is running";
                }
                else
                {
                    // For games that don't return a window (they show themselves)
                    _statusLabel.Text = $"{game.DisplayName} is running";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching game: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _statusLabel.Text = "Ready";
            }
        }
        
        private void ApplyTheme()
        {
            // Apply theme to main form
            BackColor = _themeManager.BackgroundColor;
            
            // Apply theme to title
            _titleLabel.BackColor = _themeManager.TitleBackColor;
            _titleLabel.ForeColor = _themeManager.TextColor;
            
            // Apply theme to games panel
            _gamesPanel.BackColor = _themeManager.PanelColor;
            
            // Apply theme to menu strip
            _menuStrip.BackColor = _themeManager.TitleBackColor;
            _menuStrip.ForeColor = _themeManager.TextColor;
            foreach (ToolStripItem item in _menuStrip.Items)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    menuItem.BackColor = _themeManager.TitleBackColor;
                    menuItem.ForeColor = _themeManager.TextColor;
                    foreach (ToolStripItem subItem in menuItem.DropDownItems)
                    {
                        if (subItem is ToolStripMenuItem subMenuItem)
                        {
                            subMenuItem.BackColor = _themeManager.TitleBackColor;
                            subMenuItem.ForeColor = _themeManager.TextColor;
                        }
                    }
                }
            }
            
            // Apply theme to status strip
            _statusStrip.BackColor = _themeManager.TitleBackColor;
            _statusLabel.ForeColor = _themeManager.TextColor;
            
            // Re-populate games to apply theme to cards
            if (_gameDiscovery.AvailableGames.Count > 0)
            {
                PopulateGames();
            }
        }
        
        private void ShowAbout(object? sender, EventArgs e)
        {
            var aboutText = $@"Classic Games Collection v1.0

A modern implementation of classic games built with .NET and Windows Forms.

Features:
• Modular game architecture
• Plugin system for easy game addition
• Persistent statistics and settings
• Modern UI with classic gameplay

Games Available: {_gameDiscovery.AvailableGames.Count}

Built as a portfolio project showcasing:
• Clean architecture principles
• Plugin/modular design patterns
• Windows Forms development
• C# and .NET best practices";

            MessageBox.Show(aboutText, "About Classic Games Collection", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void ShowHighScores(object? sender, EventArgs e)
        {
            try
            {
                using var highScoresForm = new HighScoresForm(_scoreService);
                highScoresForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening high scores: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private async void OnGameScoreChanged(object? sender, ScoreChangedEventArgs e)
        {
            if (sender is IGame game && e.NewScore > 0)
            {
                // Prompt for player name
                using var dialog = new PlayerNameDialog();
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.PlayerName))
                {
                    var scoreEntry = new ScoreEntry
                    {
                        GameId = game.GameId,
                        PlayerName = dialog.PlayerName.Trim(),
                        Score = e.NewScore,
                        AchievedAt = DateTime.UtcNow,
                        Difficulty = GetCurrentDifficulty(game),
                        // Time property removed from ScoreEntry
                    };
                    await _scoreService.AddScoreAsync(scoreEntry);
                }
            }
            // Fallback: If sender is not IGame but this is a minesweeper score event, try to prompt anyway
            else if (e.NewScore > 0 && e.GetType().GetProperty("GameId") != null && (string?)e.GetType().GetProperty("GameId")?.GetValue(e) == "minesweeper")
            {
                // Prompt for player name
                using var dialog = new PlayerNameDialog();
                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.PlayerName))
                {
                    var scoreEntry = new ScoreEntry
                    {
                        GameId = "minesweeper",
                        PlayerName = dialog.PlayerName.Trim(),
                        Score = e.NewScore,
                        AchievedAt = DateTime.UtcNow,
                        Difficulty = "Unknown",
                        // Time property removed from ScoreEntry
                    };
                    await _scoreService.AddScoreAsync(scoreEntry);
                }
            }
        }
        
        private string GetCurrentDifficulty(IGame game)
        {
            // For now, default to the first difficulty level
            // Games could expose their current difficulty if needed
            return game.DifficultyLevels.FirstOrDefault() ?? "Normal";
        }
        
        private void OnNewHighScore(object? sender, ScoreEntry score)
        {
            // Show notification for new high score
            var message = $"🎉 New High Score!\n\n" +
                         $"Game: {GetGameDisplayName(score.GameId)}\n" +
                         $"Player: {score.PlayerName}\n" +
                         $"Score: {score.Score.ToString("N0")}";
            
            MessageBox.Show(message, "Congratulations!", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private string GetGameDisplayName(string gameId)
        {
            return gameId switch
            {
                "minesweeper" => "Minesweeper",
                "snake" => "Snake", 
                "tetris" => "Tetris",
                "solitaire" => "Solitaire",
                _ => gameId
            };
        }
        
        
        private async void ShowSettings(object? sender, EventArgs e)
        {
            try
            {
                using var settingsForm = new SettingsForm(_settingsService, _themeManager);
                var result = settingsForm.ShowDialog(this);
                
                if (result == DialogResult.OK)
                {
                    // Settings were saved, reload and apply changes
                    // But don't interfere with theme that was already applied in SettingsForm
                    var settings = await _settingsService.GetSettingsAsync();
                    
                    // Apply window settings only (theme is already handled by SettingsForm)
                    if (settings.RememberWindowSize && settings.WindowSize.HasValue)
                    {
                        Size = settings.WindowSize.Value;
                    }
                    
                    if (settings.RememberWindowSize && settings.WindowLocation.HasValue)
                    {
                        Location = settings.WindowLocation.Value;
                        StartPosition = FormStartPosition.Manual;
                    }
                    
                    if (settings.StartMaximized)
                    {
                        WindowState = FormWindowState.Maximized;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        protected override void OnLoad(EventArgs e)
        {
            // Load theme settings synchronously before form is shown
            LoadThemeSettingsSync();
            
            base.OnLoad(e);
        }
        
        private void LoadThemeSettingsSync()
        {
            try
            {
                // Load settings synchronously for theme only - use Task.Run to avoid deadlock
                var settings = Task.Run(async () => await _settingsService.GetSettingsAsync()).Result;
                
                // Apply theme settings - simplified to just Default and Dark
                _themeManager.IsDarkMode = settings.PreferredTheme == "Dark";
                ApplyTheme();
                
                // Also apply window settings here to avoid conflicts
                if (settings.RememberWindowSize && settings.WindowSize.HasValue)
                {
                    Size = settings.WindowSize.Value;
                }
                
                if (settings.RememberWindowSize && settings.WindowLocation.HasValue)
                {
                    Location = settings.WindowLocation.Value;
                    StartPosition = FormStartPosition.Manual;
                }
                
                if (settings.StartMaximized)
                {
                    WindowState = FormWindowState.Maximized;
                }
            }
            catch (Exception ex)
            {
                // Theme loading failed, use defaults
                System.Diagnostics.Debug.WriteLine($"Failed to load theme settings: {ex.Message}");
                ApplyTheme();
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _themeManager.ThemeChanged -= ApplyTheme;
                _scoreService.NewHighScore -= OnNewHighScore;
            }
            base.Dispose(disposing);
        }
    }
}
