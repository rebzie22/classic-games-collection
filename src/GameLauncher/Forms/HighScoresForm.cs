using GameCore.Interfaces;
using GameCore.Models;
using GameLauncher.Services;

namespace GameLauncher.Forms
{
    public partial class HighScoresForm : Form
    {
        private readonly IScoreService _scoreService;
        private readonly ThemeManager _themeManager;
        private TabControl _tabControl = null!;
        private Dictionary<string, ListView> _gameListViews = new Dictionary<string, ListView>();
        private Button _exportButton = null!;
        private Button _clearButton = null!;

        public HighScoresForm(IScoreService scoreService)
        {
            _scoreService = scoreService;
            _themeManager = ThemeManager.Instance;
            
            InitializeComponent();
            LoadScoresAsync();
            
            // Subscribe to theme changes
            _themeManager.ThemeChanged += ApplyTheme;
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            Text = "High Scores";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(600, 400);
            
            // Create header panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20, 10, 20, 10)
            };
            
            var titleLabel = new Label
            {
                Text = "🏆 High Scores",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Dock = DockStyle.Left,
                Width = 200,
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            // Create button panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 200
            };
            
            _exportButton = new Button
            {
                Text = "Export",
                Size = new Size(80, 35),
                Location = new Point(10, 12),
                FlatStyle = FlatStyle.Flat
            };
            _exportButton.Click += ExportScores_Click;
            
            _clearButton = new Button
            {
                Text = "Clear All",
                Size = new Size(80, 35),
                Location = new Point(100, 12),
                FlatStyle = FlatStyle.Flat
            };
            _clearButton.Click += ClearScores_Click;
            
            buttonPanel.Controls.AddRange(new Control[] { _exportButton, _clearButton });
            headerPanel.Controls.AddRange(new Control[] { titleLabel, buttonPanel });
            
            // Create main tab control
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            
            // Create tabs for each game + overview
            CreateOverviewTab();
            CreateGameTab("minesweeper", "Minesweeper");
            CreateGameTab("snake", "Snake");
            CreateGameTab("tetris", "Tetris");
            
            Controls.AddRange(new Control[] { _tabControl, headerPanel });
        }

        private void CreateOverviewTab()
        {
            var tabPage = new TabPage("Overview");
            
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300
            };
            
            // Top panel - Recent scores
            var recentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            
            var recentLabel = new Label
            {
                Text = "Recent Scores",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            
            var recentListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            recentListView.Columns.AddRange(new ColumnHeader[]
            {
                new ColumnHeader { Text = "Game", Width = 120 },
                new ColumnHeader { Text = "Player", Width = 120 },
                new ColumnHeader { Text = "Score", Width = 100 },
                new ColumnHeader { Text = "Time", Width = 100 },
                new ColumnHeader { Text = "Date", Width = 150 }
            });
            
            recentPanel.Controls.AddRange(new Control[] { recentListView, recentLabel });
            splitContainer.Panel1.Controls.Add(recentPanel);
            
            // Bottom panel - Statistics
            var statsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            
            var statsLabel = new Label
            {
                Text = "Statistics",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30
            };
            
            var statsListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            statsListView.Columns.AddRange(new ColumnHeader[]
            {
                new ColumnHeader { Text = "Game", Width = 120 },
                new ColumnHeader { Text = "Total Scores", Width = 100 },
                new ColumnHeader { Text = "Top Player", Width = 120 },
                new ColumnHeader { Text = "Best Score", Width = 100 }
            });
            
            statsPanel.Controls.AddRange(new Control[] { statsListView, statsLabel });
            splitContainer.Panel2.Controls.Add(statsPanel);
            
            tabPage.Controls.Add(splitContainer);
            _tabControl.TabPages.Add(tabPage);
            
            // Store references for later updates
            _gameListViews["recent"] = recentListView;
            _gameListViews["stats"] = statsListView;
        }

        private void CreateGameTab(string gameId, string displayName)
        {
            var tabPage = new TabPage(displayName);
            
            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Margin = new Padding(20)
            };
            
            // Configure columns based on game type
            if (gameId == "minesweeper")
            {
                listView.Columns.AddRange(new ColumnHeader[]
                {
                    new ColumnHeader { Text = "Rank", Width = 60 },
                    new ColumnHeader { Text = "Player", Width = 150 },
                    new ColumnHeader { Text = "Time", Width = 100 },
                    new ColumnHeader { Text = "Difficulty", Width = 120 },
                    new ColumnHeader { Text = "Date", Width = 150 }
                });
            }
            else
            {
                listView.Columns.AddRange(new ColumnHeader[]
                {
                    new ColumnHeader { Text = "Rank", Width = 60 },
                    new ColumnHeader { Text = "Player", Width = 150 },
                    new ColumnHeader { Text = "Score", Width = 100 },
                    new ColumnHeader { Text = "Level", Width = 100 },
                    new ColumnHeader { Text = "Date", Width = 150 }
                });
            }
            
            tabPage.Controls.Add(listView);
            _tabControl.TabPages.Add(tabPage);
            _gameListViews[gameId] = listView;
        }

        private async void LoadScoresAsync()
        {
            try
            {
                // Load recent scores for overview
                var allScores = await _scoreService.GetAllScoresAsync();
                var recentScores = allScores.Take(20).ToList();
                
                var recentListView = _gameListViews["recent"];
                recentListView.Items.Clear();
                
                foreach (var score in recentScores)
                {
                    var item = new ListViewItem(new string[]
                    {
                        GetGameDisplayName(score.GameId),
                        score.PlayerName,
                        score.GameId == "minesweeper" ? score.TimeFormatted : score.Score.ToString("N0"),
                        score.TimeFormatted,
                        score.AchievedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                    });
                    recentListView.Items.Add(item);
                }
                
                // Load game-specific scores
                foreach (var gameId in new[] { "minesweeper", "snake", "tetris", "solitaire" })
                {
                    await LoadGameScoresAsync(gameId);
                }
                
                // Load statistics
                await LoadStatisticsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading scores: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadGameScoresAsync(string gameId)
        {
            var scores = await _scoreService.GetTopScoresAsync(gameId, 50);
            var listView = _gameListViews[gameId];
            
            listView.Items.Clear();
            int rank = 1;
            
            foreach (var score in scores)
            {
                ListViewItem item;
                
                if (gameId == "minesweeper")
                {
                    item = new ListViewItem(new string[]
                    {
                        rank.ToString(),
                        score.PlayerName,
                        score.TimeFormatted,
                        score.Difficulty,
                        score.AchievedAt.ToLocalTime().ToString("yyyy-MM-dd")
                    });
                }
                else
                {
                    item = new ListViewItem(new string[]
                    {
                        rank.ToString(),
                        score.PlayerName,
                        score.Score.ToString("N0"),
                        score.Difficulty,
                        score.AchievedAt.ToLocalTime().ToString("yyyy-MM-dd")
                    });
                }
                
                // Highlight top 3
                if (rank <= 3)
                {
                    item.BackColor = rank == 1 ? _themeManager.FirstPlaceColor : 
                                   rank == 2 ? _themeManager.SecondPlaceColor : 
                                   _themeManager.ThirdPlaceColor;
                    
                    // Ensure text is visible on highlight colors
                    item.ForeColor = _themeManager.IsDarkMode ? Color.Black : Color.Black;
                }
                
                listView.Items.Add(item);
                rank++;
            }
        }

        private async Task LoadStatisticsAsync()
        {
            var statsListView = _gameListViews["stats"];
            statsListView.Items.Clear();
            
            foreach (var gameId in new[] { "minesweeper", "snake", "tetris", "solitaire" })
            {
                var scores = await _scoreService.GetTopScoresAsync(gameId, 1);
                var allGameScores = (await _scoreService.GetAllScoresAsync())
                    .Where(s => s.GameId == gameId).ToList();
                
                var topScore = scores.FirstOrDefault();
                
                var item = new ListViewItem(new string[]
                {
                    GetGameDisplayName(gameId),
                    allGameScores.Count.ToString(),
                    topScore?.PlayerName ?? "None",
                    topScore?.Score.ToString("N0") ?? "0"
                });
                
                statsListView.Items.Add(item);
            }
        }

        private string GetGameDisplayName(string gameId)
        {
            return gameId switch
            {
                "minesweeper" => "Minesweeper",
                "snake" => "Snake",
                "tetris" => "Tetris",
                _ => gameId
            };
        }

        private async void ExportScores_Click(object? sender, EventArgs e)
        {
            try
            {
                using var saveDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = "json",
                    FileName = $"HighScores_{DateTime.Now:yyyyMMdd}.json"
                };
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var scores = await _scoreService.GetAllScoresAsync();
                    var json = System.Text.Json.JsonSerializer.Serialize(scores, 
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    
                    await File.WriteAllTextAsync(saveDialog.FileName, json);
                    MessageBox.Show("Scores exported successfully!", "Export Complete", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting scores: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearScores_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all high scores? This action cannot be undone.",
                "Clear All Scores",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
                
            if (result == DialogResult.Yes)
            {
                // TODO: Implement clear functionality
                MessageBox.Show("Clear functionality will be implemented with the data service integration.", 
                    "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ApplyTheme()
        {
            BackColor = _themeManager.BackgroundColor;
            ForeColor = _themeManager.TextColor;
            
            // Apply theme to all controls recursively
            ApplyThemeToControls(Controls);
        }

        private void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is Panel panel)
                {
                    panel.BackColor = _themeManager.PanelColor;
                }
                else if (control is ListView listView)
                {
                    listView.BackColor = _themeManager.CardBackColor;
                    listView.ForeColor = _themeManager.TextColor;
                }
                else if (control is TabControl tabControl)
                {
                    tabControl.BackColor = _themeManager.PanelColor;
                }
                else if (control is TabPage tabPage)
                {
                    tabPage.BackColor = _themeManager.PanelColor;
                    tabPage.ForeColor = _themeManager.TextColor;
                }
                else if (control is Button button)
                {
                    button.BackColor = _themeManager.ButtonColor;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderColor = _themeManager.ButtonColor;
                }
                else if (control is Label label)
                {
                    label.ForeColor = _themeManager.TextColor;
                    if (label.Parent != null)
                        label.BackColor = Color.Transparent;
                }
                
                if (control.HasChildren)
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _themeManager.ThemeChanged -= ApplyTheme;
            }
            base.Dispose(disposing);
        }
    }
}
