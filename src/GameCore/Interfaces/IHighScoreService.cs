using GameCore.Models;

namespace GameCore.Interfaces
{
    public interface IHighScoreService
    {
        string? PromptForPlayerName();
        void SaveScore(ScoreEntry entry);
    }
}
