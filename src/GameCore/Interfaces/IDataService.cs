using GameCore.Models;

namespace GameCore.Interfaces
{
    public interface IDataService
    {
        Task<ScoreDatabase> LoadDatabaseAsync();
        Task SaveDatabaseAsync(ScoreDatabase database);
        Task<bool> DatabaseExistsAsync();
        Task CreateBackupAsync(string backupPath);
        Task RestoreFromBackupAsync(string backupPath);
    }
}
