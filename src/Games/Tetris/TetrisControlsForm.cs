using System.Drawing;

namespace Tetris
{
    public partial class TetrisControlsForm : Form
    {
        private TetrisControls _controls;
        private TableLayoutPanel _mainPanel = null!;
        private Dictionary<string, Label> _keyLabels = null!;
        private string? _waitingForKey;

        public new TetrisControls Controls => _controls;

        public TetrisControlsForm(TetrisControls controls)
        {
            _controls = new TetrisControls
            {
                MoveLeft = controls.MoveLeft,
                MoveRight = controls.MoveRight,
                MoveDown = controls.MoveDown,
                HardDrop = controls.HardDrop,
                RotateClockwise = controls.RotateClockwise,
                RotateCounterClockwise = controls.RotateCounterClockwise,
                Hold = controls.Hold,
                Pause = controls.Pause
            };

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Tetris Controls";
            Size = new Size(400, 350);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            KeyPreview = true;

            _keyLabels = new Dictionary<string, Label>();

            // Main panel
            _mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                RowCount = 10,
                ColumnCount = 2,
                AutoSize = true
            };

            // Configure columns
            _mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            _mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            // Add title
            var titleLabel = new Label
            {
                Text = "Click on a key to change it",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _mainPanel.Controls.Add(titleLabel, 0, 0);
            _mainPanel.SetColumnSpan(titleLabel, 2);

            // Add controls
            var controls = _controls.GetAllControls();
            int row = 1;

            foreach (var control in controls)
            {
                var actionLabel = new Label
                {
                    Text = control.Key + ":",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(0, 5, 0, 5)
                };

                var keyLabel = new Label
                {
                    Text = control.Value.ToString(),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.LightGray,
                    Cursor = Cursors.Hand,
                    Padding = new Padding(5),
                    Tag = control.Key
                };

                keyLabel.Click += KeyLabel_Click;
                _keyLabels[control.Key] = keyLabel;

                _mainPanel.Controls.Add(actionLabel, 0, row);
                _mainPanel.Controls.Add(keyLabel, 1, row);

                // Configure row style
                _mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                row++;
            }

            // Add buttons
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(75, 25),
                DialogResult = DialogResult.Cancel
            };

            var okButton = new Button
            {
                Text = "OK",
                Size = new Size(75, 25),
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;

            var resetButton = new Button
            {
                Text = "Reset to Defaults",
                Size = new Size(120, 25)
            };
            resetButton.Click += ResetButton_Click;

            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(resetButton);

            _mainPanel.Controls.Add(buttonPanel, 0, row);
            _mainPanel.SetColumnSpan(buttonPanel, 2);
            _mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            base.Controls.Add(_mainPanel);

            // Event handlers
            KeyDown += TetrisControlsForm_KeyDown;
        }

        private void KeyLabel_Click(object? sender, EventArgs e)
        {
            if (sender is Label label && label.Tag is string action)
            {
                // Reset previous selection
                if (!string.IsNullOrEmpty(_waitingForKey) && _keyLabels.ContainsKey(_waitingForKey))
                {
                    _keyLabels[_waitingForKey].BackColor = Color.LightGray;
                }

                _waitingForKey = action;
                label.BackColor = Color.Yellow;
                label.Text = "Press a key...";
            }
        }

        private void TetrisControlsForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!string.IsNullOrEmpty(_waitingForKey) && _keyLabels.ContainsKey(_waitingForKey))
            {
                // Don't allow modifier keys by themselves
                if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.Alt || 
                    e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Escape)
                {
                    return;
                }

                _controls.SetControl(_waitingForKey, e.KeyCode);
                _keyLabels[_waitingForKey].Text = e.KeyCode.ToString();
                _keyLabels[_waitingForKey].BackColor = Color.LightGray;
                _waitingForKey = null;

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            _controls.Save();
        }

        private void ResetButton_Click(object? sender, EventArgs e)
        {
            _controls = new TetrisControls();
            
            foreach (var control in _controls.GetAllControls())
            {
                if (_keyLabels.ContainsKey(control.Key))
                {
                    _keyLabels[control.Key].Text = control.Value.ToString();
                    _keyLabels[control.Key].BackColor = Color.LightGray;
                }
            }

            _waitingForKey = null;
        }
    }
}
