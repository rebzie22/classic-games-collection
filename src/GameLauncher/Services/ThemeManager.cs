using System.Drawing;

namespace GameLauncher.Services
{
    public class ThemeManager
    {
        public static ThemeManager Instance { get; } = new ThemeManager();
        
        private bool _isDarkMode = false;
        
        public bool IsDarkMode 
        { 
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    ThemeChanged?.Invoke();
                }
            }
        }
        
        public event Action? ThemeChanged;
        
        // Light theme colors
        public Color LightBackgroundColor => Color.FromArgb(240, 240, 240);
        public Color LightPanelColor => Color.FromArgb(250, 250, 250);
        public Color LightTitleBackColor => Color.White;
        public Color LightTextColor => Color.FromArgb(64, 64, 64);
        public Color LightCardBackColor => Color.White;
        public Color LightCardBorderColor => Color.FromArgb(200, 200, 200);
        public Color LightButtonColor => Color.FromArgb(0, 120, 215);
        public Color LightButtonHoverColor => Color.FromArgb(0, 90, 158);
        public Color LightFirstPlaceColor => Color.Gold;
        public Color LightSecondPlaceColor => Color.Silver;
        public Color LightThirdPlaceColor => Color.FromArgb(205, 127, 50); // Bronze
        
        // Dark theme colors
        public Color DarkBackgroundColor => Color.FromArgb(32, 32, 32);
        public Color DarkPanelColor => Color.FromArgb(45, 45, 45);
        public Color DarkTitleBackColor => Color.FromArgb(28, 28, 28);
        public Color DarkTextColor => Color.FromArgb(220, 220, 220);
        public Color DarkCardBackColor => Color.FromArgb(55, 55, 55);
        public Color DarkCardBorderColor => Color.FromArgb(80, 80, 80);
        public Color DarkButtonColor => Color.FromArgb(0, 120, 215);
        public Color DarkButtonHoverColor => Color.FromArgb(0, 140, 255);
        public Color DarkFirstPlaceColor => Color.FromArgb(255, 215, 0); // Darker gold for better contrast
        public Color DarkSecondPlaceColor => Color.FromArgb(192, 192, 192); // Darker silver
        public Color DarkThirdPlaceColor => Color.FromArgb(184, 115, 51); // Darker bronze
        
        // Current theme properties
        public Color BackgroundColor => IsDarkMode ? DarkBackgroundColor : LightBackgroundColor;
        public Color PanelColor => IsDarkMode ? DarkPanelColor : LightPanelColor;
        public Color TitleBackColor => IsDarkMode ? DarkTitleBackColor : LightTitleBackColor;
        public Color TextColor => IsDarkMode ? DarkTextColor : LightTextColor;
        public Color CardBackColor => IsDarkMode ? DarkCardBackColor : LightCardBackColor;
        public Color CardBorderColor => IsDarkMode ? DarkCardBorderColor : LightCardBorderColor;
        public Color ButtonColor => IsDarkMode ? DarkButtonColor : LightButtonColor;
        public Color ButtonHoverColor => IsDarkMode ? DarkButtonHoverColor : LightButtonHoverColor;
        public Color FirstPlaceColor => IsDarkMode ? DarkFirstPlaceColor : LightFirstPlaceColor;
        public Color SecondPlaceColor => IsDarkMode ? DarkSecondPlaceColor : LightSecondPlaceColor;
        public Color ThirdPlaceColor => IsDarkMode ? DarkThirdPlaceColor : LightThirdPlaceColor;
        
        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }
    }
}
