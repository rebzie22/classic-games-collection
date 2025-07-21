using System;
using System.Drawing;
using System.Windows.Forms;
using GameCore.Models;
using GameLauncher.Services;

namespace GameLauncher.Forms
{
    public partial class AchievementNotificationForm : Form
    {
        private readonly Achievement _achievement;
        private readonly System.Windows.Forms.Timer _autoCloseTimer;
        private readonly ThemeManager _themeManager;
        private int _animationStep = 0;
        private const int AnimationSteps = 20;
        private const int DisplayTimeMs = 3000;
        
        public AchievementNotificationForm(Achievement achievement, ThemeManager themeManager)
        {
            _achievement = achievement;
            _themeManager = themeManager;
            
            InitializeComponent();
            
            // Setup auto-close timer
            _autoCloseTimer = new System.Windows.Forms.Timer();
            _autoCloseTimer.Interval = DisplayTimeMs;
            _autoCloseTimer.Tick += (s, e) => {
                _autoCloseTimer.Stop();
                AnimateOut();
            };
            
            // Start animation and timer
            AnimateIn();
            _autoCloseTimer.Start();
        }
        
        private void InitializeComponent()
        {
            SuspendLayout();
            
            // Form setup
            Size = new Size(350, 100);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            
            // Position at bottom-right of screen
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            Location = new Point(
                workingArea.Right - Width - 20,
                workingArea.Bottom - Height - 20
            );
            
            // Enable double buffering
            SetStyle(ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.UserPaint | 
                     ControlStyles.DoubleBuffer, true);
            
            // Apply theme
            BackColor = _themeManager.CardBackColor;
            
            // Add border
            Paint += OnPaint;
            Click += (s, e) => Close();
            
            ResumeLayout(false);
        }
        
        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Draw background with border
            using var backgroundBrush = new SolidBrush(_themeManager.CardBackColor);
            using var borderPen = new Pen(_themeManager.ButtonColor, 2);
            
            var rect = new Rectangle(1, 1, Width - 2, Height - 2);
            g.FillRectangle(backgroundBrush, rect);
            g.DrawRectangle(borderPen, rect);
            
            // Draw achievement icon (simple colored circle for now)
            var iconRect = new Rectangle(10, 10, 30, 30);
            using var iconBrush = new SolidBrush(Color.Gold);
            g.FillEllipse(iconBrush, iconRect);
            
            // Draw trophy symbol
            using var trophyBrush = new SolidBrush(Color.DarkGoldenrod);
            using var trophyFont = new Font("Arial", 16, FontStyle.Bold);
            g.DrawString("🏆", trophyFont, trophyBrush, iconRect.X + 3, iconRect.Y + 3);
            
            // Draw "Achievement Unlocked!" text
            using var headerFont = new Font("Arial", 10, FontStyle.Bold);
            using var headerBrush = new SolidBrush(_themeManager.TextColor);
            g.DrawString("Achievement Unlocked!", headerFont, headerBrush, 50, 10);
            
            // Draw achievement name
            using var nameFont = new Font("Arial", 9, FontStyle.Bold);
            using var nameBrush = new SolidBrush(_themeManager.ButtonColor);
            g.DrawString(_achievement.Name, nameFont, nameBrush, 50, 28);
            
            // Draw achievement description
            using var descFont = new Font("Arial", 8);
            using var descBrush = new SolidBrush(_themeManager.TextColor);
            g.DrawString(_achievement.Description, descFont, descBrush, 50, 45);
            
            // Draw points
            using var pointsFont = new Font("Arial", 8);
            using var pointsBrush = new SolidBrush(Color.Gold);
            g.DrawString($"+{_achievement.Points} points", pointsFont, pointsBrush, 50, 62);
        }
        
        private void AnimateIn()
        {
            var animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 16; // ~60 FPS
            
            var startY = Location.Y + Height;
            var targetY = Location.Y;
            
            animationTimer.Tick += (s, e) =>
            {
                _animationStep++;
                
                // Ease-out animation
                var progress = (double)_animationStep / AnimationSteps;
                var easedProgress = 1 - Math.Pow(1 - progress, 3);
                
                var currentY = (int)(startY + (targetY - startY) * easedProgress);
                Location = new Point(Location.X, currentY);
                
                if (_animationStep >= AnimationSteps)
                {
                    animationTimer.Stop();
                    animationTimer.Dispose();
                }
            };
            
            Location = new Point(Location.X, startY);
            animationTimer.Start();
        }
        
        private void AnimateOut()
        {
            var animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 16; // ~60 FPS
            _animationStep = 0;
            
            var startY = Location.Y;
            var targetY = Location.Y + Height;
            
            animationTimer.Tick += (s, e) =>
            {
                _animationStep++;
                
                // Ease-in animation
                var progress = (double)_animationStep / AnimationSteps;
                var easedProgress = Math.Pow(progress, 3);
                
                var currentY = (int)(startY + (targetY - startY) * easedProgress);
                Location = new Point(Location.X, currentY);
                
                if (_animationStep >= AnimationSteps)
                {
                    animationTimer.Stop();
                    animationTimer.Dispose();
                    Close();
                }
            };
            
            animationTimer.Start();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoCloseTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
