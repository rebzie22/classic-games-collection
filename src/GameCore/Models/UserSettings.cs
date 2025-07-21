using System;
using System.Drawing;
using System.Text.Json;

namespace GameCore.Models
{
    public class UserSettings
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        
        // Display Settings
        public bool IsDarkMode { get; set; } = false;
        public string PreferredTheme { get; set; } = "Default";
        
        // Window Settings
        public int WindowWidth { get; set; } = 1024;
        public int WindowHeight { get; set; } = 768;
        public int WindowX { get; set; } = -1; // -1 means not set
        public int WindowY { get; set; } = -1; // -1 means not set
        public bool RememberWindowSize { get; set; } = true;
        public bool StartMaximized { get; set; } = false;
        public bool IsMaximized { get; set; } = false;
        
        // Helper properties for easier access
        public Size? WindowSize
        {
            get => WindowWidth > 0 && WindowHeight > 0 ? new Size(WindowWidth, WindowHeight) : null;
            set
            {
                if (value.HasValue)
                {
                    WindowWidth = value.Value.Width;
                    WindowHeight = value.Value.Height;
                }
            }
        }
        
        public Point? WindowLocation
        {
            get => WindowX >= 0 && WindowY >= 0 ? new Point(WindowX, WindowY) : null;
            set
            {
                if (value.HasValue)
                {
                    WindowX = value.Value.X;
                    WindowY = value.Value.Y;
                }
            }
        }
        
        // Game Settings
        public string DefaultDifficulty { get; set; } = "Intermediate";
        public bool PlaySounds { get; set; } = true;
        public bool ShowAnimations { get; set; } = true;
        public bool AutoSave { get; set; } = true;
        
        // Score Settings
        public string DefaultPlayerName { get; set; } = string.Empty;
        public bool PromptForName { get; set; } = true;
        public bool ShowHighScoreNotifications { get; set; } = true;
        
        // Game-Specific Settings
        public Dictionary<string, UserGameSettings> GameSpecificSettings { get; set; } = new Dictionary<string, UserGameSettings>();
        
        // Advanced Settings
        public bool EnableDebugging { get; set; } = false;
        public int AutoSaveInterval { get; set; } = 300; // seconds
        public string BackupLocation { get; set; } = string.Empty;
    }
    
    public class UserGameSettings
    {
        public string GameId { get; set; } = string.Empty;
        public string PreferredDifficulty { get; set; } = "Normal";
        public Dictionary<string, object> CustomSettings { get; set; } = new Dictionary<string, object>();
        
        // Helper methods for type-safe access
        public T GetSetting<T>(string key, T defaultValue = default(T)!)
        {
            if (CustomSettings.TryGetValue(key, out var value))
            {
                if (value is JsonElement jsonElement)
                {
                    try
                    {
                        return JsonSerializer.Deserialize<T>(jsonElement.GetRawText()) ?? defaultValue;
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
                if (value is T directValue)
                {
                    return directValue;
                }
            }
            return defaultValue;
        }
        
        public void SetSetting<T>(string key, T value)
        {
            CustomSettings[key] = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
