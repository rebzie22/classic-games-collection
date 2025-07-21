namespace GameCore.Models
{
    /// <summary>
    /// Configuration settings for games
    /// </summary>
    public class GameSettings
    {
        /// <summary>
        /// Sound enabled/disabled
        /// </summary>
        public bool SoundEnabled { get; set; } = true;
        
        /// <summary>
        /// Sound volume (0.0 to 1.0)
        /// </summary>
        public float SoundVolume { get; set; } = 0.7f;
        
        /// <summary>
        /// Theme/skin selection
        /// </summary>
        public string Theme { get; set; } = "Default";
        
        /// <summary>
        /// Animation speed multiplier
        /// </summary>
        public float AnimationSpeed { get; set; } = 1.0f;
        
        /// <summary>
        /// Show hints/assistance
        /// </summary>
        public bool ShowHints { get; set; } = true;
        
        /// <summary>
        /// Auto-save game state
        /// </summary>
        public bool AutoSave { get; set; } = true;
        
        /// <summary>
        /// Custom key bindings
        /// </summary>
        public Dictionary<string, Keys> KeyBindings { get; set; } = new();
        
        /// <summary>
        /// Game-specific settings
        /// </summary>
        public Dictionary<string, object> GameSpecificSettings { get; set; } = new();
        
        /// <summary>
        /// Load settings from file
        /// </summary>
        public static GameSettings Load(string gameId)
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClassicGamesCollection",
                $"{gameId}_settings.json"
            );
            
            if (File.Exists(settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(settingsPath);
                    return System.Text.Json.JsonSerializer.Deserialize<GameSettings>(json) ?? new GameSettings();
                }
                catch
                {
                    // If settings are corrupted, return defaults
                    return new GameSettings();
                }
            }
            
            return new GameSettings();
        }
        
        /// <summary>
        /// Save settings to file
        /// </summary>
        public void Save(string gameId)
        {
            var settingsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClassicGamesCollection"
            );
            
            Directory.CreateDirectory(settingsDir);
            
            var settingsPath = Path.Combine(settingsDir, $"{gameId}_settings.json");
            var json = System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(settingsPath, json);
        }
    }
}
