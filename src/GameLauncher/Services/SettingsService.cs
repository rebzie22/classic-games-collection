using System.Text.Json;
using GameCore.Interfaces;
using GameCore.Models;

namespace GameLauncher.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string _settingsFilePath;
        
        public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;
        
        public SettingsService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClassicGamesCollection"
            );
            
            Directory.CreateDirectory(appDataPath);
            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
        }
        
        public async Task<UserSettings> GetSettingsAsync()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = await File.ReadAllTextAsync(_settingsFilePath);
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    
                    var settings = JsonSerializer.Deserialize<UserSettings>(json, options);
                    if (settings != null)
                    {
                        return settings;
                    }
                }
                
                // Return default settings if file doesn't exist or failed to deserialize
                var defaultSettings = new UserSettings();
                await SaveSettingsAsync(defaultSettings);
                return defaultSettings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                return new UserSettings();
            }
        }
        
        public async Task SaveSettingsAsync(UserSettings settings)
        {
            try
            {
                settings.LastModified = DateTime.UtcNow;
                
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var json = JsonSerializer.Serialize(settings, options);
                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                throw;
            }
        }
        
        public async Task ResetSettingsAsync()
        {
            var defaultSettings = new UserSettings();
            await SaveSettingsAsync(defaultSettings);
        }
        
        public async Task<T> GetSettingAsync<T>(string key, T defaultValue = default(T)!)
        {
            var settings = await GetSettingsAsync();
            
            // Use reflection to get property value by name
            var property = typeof(UserSettings).GetProperty(key);
            if (property != null && property.CanRead)
            {
                var value = property.GetValue(settings);
                if (value is T typedValue)
                    return typedValue;
            }
            
            return defaultValue;
        }
        
        public async Task SetSettingAsync<T>(string key, T value)
        {
            var settings = await GetSettingsAsync();
            
            // Use reflection to set property value by name
            var property = typeof(UserSettings).GetProperty(key);
            if (property != null && property.CanWrite)
            {
                var oldValue = property.GetValue(settings);
                property.SetValue(settings, value);
                
                await SaveSettingsAsync(settings);
                
                // Fire event
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs
                {
                    SettingKey = key,
                    OldValue = oldValue,
                    NewValue = value
                });
            }
        }
        
        public async Task<UserGameSettings> GetGameSettingsAsync(string gameId)
        {
            var settings = await GetSettingsAsync();
            
            if (settings.GameSpecificSettings.TryGetValue(gameId, out var gameSettings))
            {
                return gameSettings;
            }
            
            // Return default game settings
            var defaultSettings = new UserGameSettings
            {
                GameId = gameId,
                PreferredDifficulty = settings.DefaultDifficulty
            };
            
            settings.GameSpecificSettings[gameId] = defaultSettings;
            await SaveSettingsAsync(settings);
            
            return defaultSettings;
        }
        
        public async Task SaveGameSettingsAsync(string gameId, UserGameSettings gameSettings)
        {
            var settings = await GetSettingsAsync();
            settings.GameSpecificSettings[gameId] = gameSettings;
            await SaveSettingsAsync(settings);
        }
    }
}
