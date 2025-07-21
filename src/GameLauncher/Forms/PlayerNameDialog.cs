using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameLauncher.Forms
{
    public partial class PlayerNameDialog : Form
    {
        private TextBox _nameTextBox = null!;
        private Button _okButton = null!;
        private Button _cancelButton = null!;
        
        public string PlayerName => _nameTextBox.Text;
        
        public PlayerNameDialog()
        {
            InitializeComponent();
            InitializeDialog();
        }
        
        private void InitializeDialog()
        {
            Text = "Enter Player Name";
            Size = new Size(320, 150);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            
            // Label
            var label = new Label
            {
                Text = "Enter your name for the high score:",
                Location = new Point(12, 15),
                Size = new Size(280, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(label);
            
            // TextBox
            _nameTextBox = new TextBox
            {
                Location = new Point(12, 40),
                Size = new Size(280, 20),
                MaxLength = 50
            };
            _nameTextBox.KeyDown += OnTextBoxKeyDown;
            Controls.Add(_nameTextBox);
            
            // OK Button
            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(137, 75),
                Size = new Size(75, 23),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OnOkClick;
            Controls.Add(_okButton);
            
            // Cancel Button
            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(217, 75),
                Size = new Size(75, 23),
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(_cancelButton);
            
            AcceptButton = _okButton;
            CancelButton = _cancelButton;
            
            // Set default name and select all text
            _nameTextBox.Text = Environment.UserName;
            _nameTextBox.SelectAll();
            _nameTextBox.Focus();
        }
        
        private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        
        private void OnOkClick(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show("Please enter a valid name.", "Invalid Name", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _nameTextBox.Focus();
                return;
            }
            
            DialogResult = DialogResult.OK;
            Close();
        }
        
        private void InitializeComponent()
        {
            SuspendLayout();
            ResumeLayout(false);
        }
    }
}
