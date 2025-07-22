using System;
using System.Drawing;
using System.Windows.Forms;
using GameCore.Interfaces;
using GameCore.Models;
using GameLauncher.Services;

namespace GameLauncher.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly ISettingsService _settingsService;
        private readonly ThemeManager _themeManager;
        private UserSettings _currentSettings;
        
        // Controls
        private TabControl _tabControl = null!;
        private Button _okButton = null!;
        private Button _cancelButton = null!;
        private Button _applyButton = null!;
        private Button _resetButton = null!;
        
        // General tab controls
        private ComboBox _themeComboBox = null!;
        private CheckBox _rememberWindowSizeCheckBox = null!;
        private CheckBox _startMaximizedCheckBox = null!;
        
        // Audio/Visual tab controls
        private CheckBox _playSoundsCheckBox = null!;
        private CheckBox _showAnimationsCheckBox = null!;
        private CheckBox _showNotificationsCheckBox = null!;
        
        // Game tab controls
        private ComboBox _defaultDifficultyComboBox = null!;
        private TextBox _defaultPlayerNameTextBox = null!;
        private CheckBox _promptForNameCheckBox = null!;
        private CheckBox _autoSaveCheckBox = null!;
        private NumericUpDown _autoSaveIntervalNumeric = null!;
        
        public SettingsForm(ISettingsService settingsService, ThemeManager themeManager)
        {
            _settingsService = settingsService;
            _themeManager = themeManager;
            _currentSettings = new UserSettings();
            
            InitializeComponent();
            LoadCurrentSettings();
            ApplyTheme();
        }
        
        private void InitializeComponent()
        {
            SuspendLayout();
            
            // Form setup
            Text = "Settings";
            Size = new Size(500, 400);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            
            // Create main layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            
            // Create tab control
            CreateTabControl();
            mainPanel.Controls.Add(_tabControl, 0, 0);
            
            // Create button panel
            var buttonPanel = CreateButtonPanel();
            mainPanel.Controls.Add(buttonPanel, 0, 1);
            
            Controls.Add(mainPanel);
            ResumeLayout(false);
        }
        
        private void CreateTabControl()
        {
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            
            // General tab
            var generalTab = new TabPage("General");
            CreateGeneralTab(generalTab);
            _tabControl.TabPages.Add(generalTab);
            
            
            // Game Settings tab
            var gameTab = new TabPage("Game Settings");
            CreateGameTab(gameTab);
            _tabControl.TabPages.Add(gameTab);
            
        }
        
        private void CreateGeneralTab(TabPage tab)
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10),
                RowCount = 4
            };
            
            // Configure column styles
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            
            // Theme selection
            panel.Controls.Add(new Label { Text = "Theme:", Anchor = AnchorStyles.Left }, 0, 0);
            _themeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            _themeComboBox.Items.AddRange(new[] { "Default", "Dark" });
            panel.Controls.Add(_themeComboBox, 1, 0);
            
            // Remember window size
            _rememberWindowSizeCheckBox = new CheckBox
            {
                Text = "Remember window size and position",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            };
            panel.Controls.Add(new Label { Text = "Window:", Anchor = AnchorStyles.Left }, 0, 1);
            panel.Controls.Add(_rememberWindowSizeCheckBox, 1, 1);
            
            // Start maximized
            _startMaximizedCheckBox = new CheckBox
            {
                Text = "Start maximized",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            };
            panel.Controls.Add(_startMaximizedCheckBox, 1, 2);
            panel.SetColumnSpan(_startMaximizedCheckBox, 1);
            
            tab.Controls.Add(panel);
        }
        
        private void CreateAudioVisualTab(TabPage tab)
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                Padding = new Padding(10)
            };
            
            _playSoundsCheckBox = new CheckBox
            {
                Text = "Enable sound effects",
                Anchor = AnchorStyles.Left
            };
            panel.Controls.Add(_playSoundsCheckBox, 0, 0);
            
            _showAnimationsCheckBox = new CheckBox
            {
                Text = "Enable animations",
                Anchor = AnchorStyles.Left
            };
            panel.Controls.Add(_showAnimationsCheckBox, 0, 1);
            
            _showNotificationsCheckBox = new CheckBox
            {
                Text = "Show high score notifications",
                Anchor = AnchorStyles.Left
            };
            panel.Controls.Add(_showNotificationsCheckBox, 0, 2);
            
            tab.Controls.Add(panel);
        }
        
        private void CreateGameTab(TabPage tab)
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            
            // Default difficulty
            panel.Controls.Add(new Label { Text = "Default Difficulty:", Anchor = AnchorStyles.Left }, 0, 0);
            _defaultDifficultyComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            _defaultDifficultyComboBox.Items.AddRange(new[] { "Beginner", "Intermediate", "Expert" });
            panel.Controls.Add(_defaultDifficultyComboBox, 1, 0);
            
            // Default player name
            panel.Controls.Add(new Label { Text = "Default Player Name:", Anchor = AnchorStyles.Left }, 0, 1);
            _defaultPlayerNameTextBox = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Controls.Add(_defaultPlayerNameTextBox, 1, 1);
            
            // Prompt for name
            _promptForNameCheckBox = new CheckBox
            {
                Text = "Always prompt for player name",
                Anchor = AnchorStyles.Left
            };
            panel.Controls.Add(_promptForNameCheckBox, 0, 2);
            panel.SetColumnSpan(_promptForNameCheckBox, 2);
            
            // Auto save
            _autoSaveCheckBox = new CheckBox
            {
                Text = "Enable auto-save",
                Anchor = AnchorStyles.Left
            };
            panel.Controls.Add(_autoSaveCheckBox, 0, 3);
            panel.SetColumnSpan(_autoSaveCheckBox, 2);
            
            // Auto save interval
            panel.Controls.Add(new Label { Text = "Auto-save interval (seconds):", Anchor = AnchorStyles.Left }, 0, 4);
            _autoSaveIntervalNumeric = new NumericUpDown
            {
                Minimum = 30,
                Maximum = 3600,
                Value = 300,
                Anchor = AnchorStyles.Left
            };
            panel.Controls.Add(_autoSaveIntervalNumeric, 1, 4);
            
            tab.Controls.Add(panel);
        }
        
        private void CreateAdvancedTab(TabPage tab)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            
            var label = new Label
            {
                Text = "Advanced settings for power users.\nModify these settings carefully.",
                Location = new Point(10, 10),
                Size = new Size(400, 40)
            };
            
            panel.Controls.Add(label);
            tab.Controls.Add(panel);
        }
        
        private Panel CreateButtonPanel()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10, 5, 10, 5)
            };
            
            _cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel
            };
            _cancelButton.Click += CancelButton_Click;
            
            _okButton = new Button
            {
                Text = "OK",
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;
            
            _applyButton = new Button
            {
                Text = "Apply",
                Size = new Size(75, 30)
            };
            _applyButton.Click += ApplyButton_Click;
            
            _resetButton = new Button
            {
                Text = "Reset",
                Size = new Size(75, 30)
            };
            _resetButton.Click += ResetButton_Click;
            
            panel.Controls.AddRange(new Control[] { _cancelButton, _okButton, _applyButton, _resetButton });
            
            AcceptButton = _okButton;
            CancelButton = _cancelButton;
            
            return panel;
        }
        
        private async void LoadCurrentSettings()
        {
            try
            {
                _currentSettings = await _settingsService.GetSettingsAsync();
                
                // Load values into controls
                // Use the PreferredTheme directly, and set IsDarkMode based on theme
                if (!string.IsNullOrEmpty(_currentSettings.PreferredTheme))
                {
                    _themeComboBox.Text = _currentSettings.PreferredTheme;
                }
                else
                {
                    _themeComboBox.Text = _currentSettings.IsDarkMode ? "Dark" : "Default";
                }
                
                _rememberWindowSizeCheckBox.Checked = _currentSettings.RememberWindowSize;
                _startMaximizedCheckBox.Checked = _currentSettings.StartMaximized;
                
                _playSoundsCheckBox.Checked = _currentSettings.PlaySounds;
                _showAnimationsCheckBox.Checked = _currentSettings.ShowAnimations;
                _showNotificationsCheckBox.Checked = _currentSettings.ShowHighScoreNotifications;
                
                // Handle difficulty mapping for legacy settings
                var difficulty = _currentSettings.DefaultDifficulty;
                if (difficulty == "Easy") difficulty = "Beginner";
                else if (difficulty == "Normal") difficulty = "Intermediate"; // Map Normal to Intermediate
                else if (difficulty == "Hard") difficulty = "Expert";
                _defaultDifficultyComboBox.Text = difficulty;
                
                _defaultPlayerNameTextBox.Text = _currentSettings.DefaultPlayerName;
                _promptForNameCheckBox.Checked = _currentSettings.PromptForName;
                _autoSaveCheckBox.Checked = _currentSettings.AutoSave;
                _autoSaveIntervalNumeric.Value = _currentSettings.AutoSaveInterval;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void SaveSettingsToObject()
        {
            // Map theme dropdown to both PreferredTheme and IsDarkMode
            _currentSettings.PreferredTheme = _themeComboBox.Text;
            
            // Set IsDarkMode based on theme selection - simplified to just two options
            _currentSettings.IsDarkMode = _themeComboBox.Text == "Dark";
            
            _currentSettings.RememberWindowSize = _rememberWindowSizeCheckBox.Checked;
            _currentSettings.StartMaximized = _startMaximizedCheckBox.Checked;
            
            _currentSettings.PlaySounds = _playSoundsCheckBox.Checked;
            _currentSettings.ShowAnimations = _showAnimationsCheckBox.Checked;
            _currentSettings.ShowHighScoreNotifications = _showNotificationsCheckBox.Checked;
            
            _currentSettings.DefaultDifficulty = _defaultDifficultyComboBox.Text;
            _currentSettings.DefaultPlayerName = _defaultPlayerNameTextBox.Text;
            _currentSettings.PromptForName = _promptForNameCheckBox.Checked;
            _currentSettings.AutoSave = _autoSaveCheckBox.Checked;
            _currentSettings.AutoSaveInterval = (int)_autoSaveIntervalNumeric.Value;
        }
        
        private async void OkButton_Click(object? sender, EventArgs e)
        {
            await ApplySettings();
            DialogResult = DialogResult.OK;
            Close();
        }
        
        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        
        private async void ApplyButton_Click(object? sender, EventArgs e)
        {
            await ApplySettings();
        }
        
        private async void ResetButton_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all settings to their default values?",
                "Reset Settings",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
                
            if (result == DialogResult.Yes)
            {
                await _settingsService.ResetSettingsAsync();
                LoadCurrentSettings();
            }
        }
        
        private async Task ApplySettings()
        {
            try
            {
                SaveSettingsToObject();
                
                // Update game-specific settings to match the new default difficulty
                // This ensures existing game preferences are updated when global default changes
                foreach (var gameSettings in _currentSettings.GameSpecificSettings.Values)
                {
                    gameSettings.PreferredDifficulty = _currentSettings.DefaultDifficulty;
                }
                
                await _settingsService.SaveSettingsAsync(_currentSettings);
                
                // Apply theme changes immediately
                _themeManager.IsDarkMode = _currentSettings.IsDarkMode;
                ApplyTheme();
                
                MessageBox.Show("Settings saved successfully!", "Settings", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (control is TabControl || control is TabPage)
                {
                    control.BackColor = _themeManager.PanelColor;
                    control.ForeColor = _themeManager.TextColor;
                }
                else if (control is Button button)
                {
                    button.BackColor = _themeManager.ButtonColor;
                    button.ForeColor = Color.White;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = _themeManager.ButtonColor;
                }
                else if (control is TextBox || control is ComboBox || control is NumericUpDown)
                {
                    control.BackColor = _themeManager.CardBackColor;
                    control.ForeColor = _themeManager.TextColor;
                }
                else if (control is Label || control is CheckBox)
                {
                    control.ForeColor = _themeManager.TextColor;
                    if (control.Parent != null)
                        control.BackColor = Color.Transparent;
                }
                
                if (control.HasChildren)
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }
    }
}
