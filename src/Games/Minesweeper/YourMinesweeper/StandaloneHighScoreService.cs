using System;
using System.Windows.Forms;
using GameCore.Interfaces;
using GameCore.Models;
using System.IO;
using System.Text.Json;

namespace Minesweeper.YourMinesweeper
{
    public class StandaloneHighScoreService : IHighScoreService
    {
        // The prompt is handled by the UI (MainForm). This method is only here to fulfill the interface.
        public string? PromptForPlayerName()
        {
            // Should never be called from SaveScore. Only MainForm should call this.
            return null;
        }

        public void SaveScore(ScoreEntry entry)
        {
            // Save to %APPDATA%\ClassicGamesCollection\gamedata.json
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClassicGamesCollection");
            var file = Path.Combine(dir, "gamedata.json");
            Directory.CreateDirectory(dir);
            ScoreDatabase db;
            if (File.Exists(file))
            {
                var json = File.ReadAllText(file);
                db = JsonSerializer.Deserialize<ScoreDatabase>(json) ?? new ScoreDatabase();
            }
            else
            {
                db = new ScoreDatabase();
            }
            if (db.Scores == null)
                db.Scores = new System.Collections.Generic.List<ScoreEntry>();
            // No player name prompt here! Only save the entry provided.
            db.Scores.Add(entry);
            File.WriteAllText(file, JsonSerializer.Serialize(db, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
