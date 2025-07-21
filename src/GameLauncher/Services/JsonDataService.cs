using System.Text.Json;
using GameCore.Interfaces;
using GameCore.Models;

namespace GameLauncher.Services
{
    public class JsonDataService : IDataService
    {
        private readonly string _dataDirectory;
        private readonly string _databasePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonDataService()
        {
            _dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClassicGamesCollection"
            );
            _databasePath = Path.Combine(_dataDirectory, "gamedata.json");
            
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            EnsureDataDirectoryExists();
        }

        public async Task<ScoreDatabase> LoadDatabaseAsync()
        {
            try
            {
                if (!await DatabaseExistsAsync())
                {
                    return CreateDefaultDatabase();
                }

                var json = await File.ReadAllTextAsync(_databasePath);
                var database = JsonSerializer.Deserialize<ScoreDatabase>(json, _jsonOptions);
                
                return database ?? CreateDefaultDatabase();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading database: {ex.Message}");
                return CreateDefaultDatabase();
            }
        }

        public async Task SaveDatabaseAsync(ScoreDatabase database)
        {
            try
            {
                database.LastUpdated = DateTime.UtcNow;
                var json = JsonSerializer.Serialize(database, _jsonOptions);
                await File.WriteAllTextAsync(_databasePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving database: {ex.Message}");
                throw;
            }
        }

        public Task<bool> DatabaseExistsAsync()
        {
            return Task.FromResult(File.Exists(_databasePath));
        }

        public async Task CreateBackupAsync(string backupPath)
        {
            try
            {
                if (await DatabaseExistsAsync())
                {
                    using var sourceStream = File.OpenRead(_databasePath);
                    using var destinationStream = File.Create(backupPath);
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating backup: {ex.Message}");
                throw;
            }
        }

        public async Task RestoreFromBackupAsync(string backupPath)
        {
            try
            {
                if (File.Exists(backupPath))
                {
                    using var sourceStream = File.OpenRead(backupPath);
                    using var destinationStream = File.Create(_databasePath);
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring from backup: {ex.Message}");
                throw;
            }
        }

        private void EnsureDataDirectoryExists()
        {
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        private ScoreDatabase CreateDefaultDatabase()
        {
            return new ScoreDatabase
            {
                Version = "1.0",
                LastUpdated = DateTime.UtcNow,
                Scores = new List<ScoreEntry>(),
                Players = new List<PlayerProfile>(),
                Settings = new Dictionary<string, object>
                {
                    { "DefaultPlayerName", Environment.UserName },
                    { "Theme", "Light" },
                    { "AutoSaveScores", true }
                }
            };
        }
    }
}
