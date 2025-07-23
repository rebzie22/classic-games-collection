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
                Width = 350
            };
            
            _exportButton = new Button
            {
                Text = "Export",
                Size = new Size(70, 35),
                Location = new Point(10, 12),
                FlatStyle = FlatStyle.Flat
            };
            _exportButton.Click += ExportScores_Click;
            
            var clearGameButton = new Button
            {
                Text = "Clear Game",
                Size = new Size(80, 35),
                Location = new Point(90, 12),
                FlatStyle = FlatStyle.Flat
            };
            clearGameButton.Click += ClearCurrentGame_Click;
            
            var clearPlayerButton = new Button
            {
                Text = "Clear Player",
                Size = new Size(80, 35),
                Location = new Point(180, 12),
                FlatStyle = FlatStyle.Flat
            };
            clearPlayerButton.Click += ClearPlayer_Click;
            
            _clearButton = new Button
            {
                Text = "Clear All",
                Size = new Size(70, 35),
                Location = new Point(270, 12),
                FlatStyle = FlatStyle.Flat
            };
            _clearButton.Click += ClearScores_Click;
            
            buttonPanel.Controls.AddRange(new Control[] { _exportButton, clearGameButton, clearPlayerButton, _clearButton });
            headerPanel.Controls.AddRange(new Control[] { titleLabel, buttonPanel });
            
            // Create main tab control
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            
            // Create tabs for each game + overview
            CreateOverviewTab();
            CreateGameTab("minesweeper", "Minesweeper");
            CreateGameTab("solitaire", "Solitaire");
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
            
            // Create context menu for individual score deletion
            var contextMenu = new ContextMenuStrip();
            var deleteMenuItem = new ToolStripMenuItem("Delete Score");
            deleteMenuItem.Click += async (s, e) => await DeleteSelectedScore_Click(listView);
            contextMenu.Items.Add(deleteMenuItem);
            listView.ContextMenuStrip = contextMenu;
            
            // Configure columns based on game type
            if (gameId == "minesweeper" || gameId == "solitaire")
            {
                listView.Columns.AddRange(new ColumnHeader[]
                {
                    new ColumnHeader { Text = "Rank", Width = 60 },
                    new ColumnHeader { Text = "Player", Width = 150 },
                    new ColumnHeader { Text = "Score", Width = 100 },
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
                string scoreDisplay = score.Score.ToString("N0");
                var item = new ListViewItem(new string[]
                {
                    GetGameDisplayName(score.GameId),
                    score.PlayerName,
                    scoreDisplay,
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
                item = new ListViewItem(new string[]
                {
                    rank.ToString(),
                    score.PlayerName,
                    score.Score.ToString("N0"),
                    score.Difficulty,
                    score.AchievedAt.ToLocalTime().ToString("yyyy-MM-dd")
                });
                // Store the score entry in the Tag for deletion functionality
                item.Tag = score;
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
                try
                {
                    // Actually clear all scores
                    _clearButton.Enabled = false;
                    _exportButton.Enabled = false;
                    UseWaitCursor = true;
                    Task.Run(async () => {
                        await _scoreService.ClearAllScoresAsync();
                        Invoke(new Action(() => {
                            LoadScoresAsync();
                            UseWaitCursor = false;
                            _clearButton.Enabled = true;
                            _exportButton.Enabled = true;
                            MessageBox.Show("All high scores have been cleared.", "High Scores Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }));
                    });
                }
                catch (Exception ex)
                {
                    UseWaitCursor = false;
                    _clearButton.Enabled = true;
                    _exportButton.Enabled = true;
                    MessageBox.Show($"Failed to clear high scores: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        private async void ClearCurrentGame_Click(object? sender, EventArgs e)
        {
            var currentTab = _tabControl.SelectedTab;
            if (currentTab == null || currentTab.Text == "Overview")
            {
                MessageBox.Show("Please select a specific game tab to clear scores for that game.", 
                    "No Game Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var gameId = GetGameIdFromTabText(currentTab.Text);
            var result = MessageBox.Show(
                $"Are you sure you want to clear all scores for {currentTab.Text}? This action cannot be undone.",
                $"Clear {currentTab.Text} Scores",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
                
            if (result == DialogResult.Yes)
            {
                try
                {
                    await _scoreService.ClearGameScoresAsync(gameId);
                    
                    // Refresh the current game's ListView
                    if (_gameListViews.ContainsKey(gameId))
                    {
                        _gameListViews[gameId].Items.Clear();
                    }
                    
                    // Refresh overview tab as well
                    LoadScoresAsync();
                    
                    MessageBox.Show($"All {currentTab.Text} scores have been cleared successfully.", 
                        "Scores Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing {currentTab.Text} scores: {ex.Message}", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void ClearPlayer_Click(object? sender, EventArgs e)
        {
            // Show input dialog to get player name
            var playerName = ShowInputDialog("Clear Player Scores", "Enter player name to clear all scores for:");
            
            if (string.IsNullOrWhiteSpace(playerName))
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to clear all scores for player '{playerName}'? This action cannot be undone.",
                "Clear Player Scores",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
                
            if (result == DialogResult.Yes)
            {
                try
                {
                    await _scoreService.ClearPlayerScoresAsync(playerName);
                    
                    // Refresh all ListViews
                    LoadScoresAsync();
                    
                    MessageBox.Show($"All scores for player '{playerName}' have been cleared successfully.", 
                        "Scores Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error clearing scores for player '{playerName}': {ex.Message}", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task DeleteSelectedScore_Click(ListView listView)
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a score to delete.", "No Score Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedItem = listView.SelectedItems[0];
            var scoreEntry = selectedItem.Tag as ScoreEntry;
            
            if (scoreEntry == null)
            {
                MessageBox.Show("Unable to identify the selected score.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete this score?\n\nPlayer: {scoreEntry.PlayerName}\nScore: {scoreEntry.Score}\nDate: {scoreEntry.AchievedAt:yyyy-MM-dd}",
                "Delete Score",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
                
            if (result == DialogResult.Yes)
            {
                try
                {
                    await _scoreService.DeleteScoreAsync(scoreEntry);
                    
                    // Refresh all views
                    LoadScoresAsync();
                    
                    MessageBox.Show("Score deleted successfully.", "Score Deleted", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting score: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GetGameIdFromTabText(string tabText)
        {
            return tabText.ToLowerInvariant() switch
            {
                "minesweeper" => "minesweeper",
                "solitaire" => "solitaire", 
                "snake" => "snake",
                "tetris" => "tetris",
                _ => tabText.ToLowerInvariant()
            };
        }

        private string ShowInputDialog(string title, string prompt)
        {
            var inputForm = new Form
            {
                Text = title,
                Size = new Size(400, 150),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label
            {
                Text = prompt,
                Location = new Point(10, 20),
                Size = new Size(350, 20)
            };

            var textBox = new TextBox
            {
                Location = new Point(10, 50),
                Size = new Size(250, 25)
            };

            var okButton = new Button
            {
                Text = "OK",
                Location = new Point(270, 48),
                Size = new Size(50, 30),
                DialogResult = DialogResult.OK
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(330, 48),
                Size = new Size(50, 30),
                DialogResult = DialogResult.Cancel
            };

            inputForm.Controls.AddRange(new Control[] { label, textBox, okButton, cancelButton });
            inputForm.AcceptButton = okButton;
            inputForm.CancelButton = cancelButton;

            return inputForm.ShowDialog(this) == DialogResult.OK ? textBox.Text : string.Empty;
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
