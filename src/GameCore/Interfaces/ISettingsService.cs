using GameCore.Models;

namespace GameCore.Interfaces
{
    public interface ISettingsService
    {
        Task<UserSettings> GetSettingsAsync();
        Task SaveSettingsAsync(UserSettings settings);
        Task ResetSettingsAsync();
        
        // Convenience methods for common settings
        Task<T> GetSettingAsync<T>(string key, T defaultValue = default(T)!);
        Task SetSettingAsync<T>(string key, T value);
        
        Task<UserGameSettings> GetGameSettingsAsync(string gameId);
        Task SaveGameSettingsAsync(string gameId, UserGameSettings settings);
        
        // Events
        event EventHandler<SettingsChangedEventArgs>? SettingsChanged;
    }
    
    public class SettingsChangedEventArgs : EventArgs
    {
        public string SettingKey { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
