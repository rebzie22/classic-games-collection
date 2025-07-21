using System.Text.Json;

namespace Tetris
{
    public class TetrisControls
    {
        public Keys MoveLeft { get; set; } = Keys.Left;
        public Keys MoveRight { get; set; } = Keys.Right;
        public Keys MoveDown { get; set; } = Keys.Down;
        public Keys HardDrop { get; set; } = Keys.Up;
        public Keys RotateClockwise { get; set; } = Keys.Space;
        public Keys RotateCounterClockwise { get; set; } = Keys.Alt;
        public Keys Hold { get; set; } = Keys.C;
        public Keys Pause { get; set; } = Keys.P;

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClassicGamesCollection",
            "tetris_controls.json");

        public static TetrisControls Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var controls = JsonSerializer.Deserialize<TetrisControls>(json, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    return controls ?? new TetrisControls();
                }
            }
            catch (Exception ex)
            {
                // Log error but return defaults
                System.Diagnostics.Debug.WriteLine($"Error loading Tetris controls: {ex.Message}");
            }

            return new TetrisControls();
        }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                System.Diagnostics.Debug.WriteLine($"Error saving Tetris controls: {ex.Message}");
            }
        }

        public bool IsControlKey(Keys key)
        {
            return key == MoveLeft || key == MoveRight || key == MoveDown || 
                   key == HardDrop || key == RotateClockwise || key == RotateCounterClockwise ||
                   key == Hold || key == Pause;
        }

        public string GetActionForKey(Keys key)
        {
            if (key == MoveLeft) return "Move Left";
            if (key == MoveRight) return "Move Right";
            if (key == MoveDown) return "Move Down";
            if (key == HardDrop) return "Hard Drop";
            if (key == RotateClockwise) return "Rotate Clockwise";
            if (key == RotateCounterClockwise) return "Rotate Counter-Clockwise";
            if (key == Hold) return "Hold";
            if (key == Pause) return "Pause";
            return string.Empty;
        }

        public void SetControl(string action, Keys key)
        {
            switch (action)
            {
                case "Move Left":
                    MoveLeft = key;
                    break;
                case "Move Right":
                    MoveRight = key;
                    break;
                case "Move Down":
                    MoveDown = key;
                    break;
                case "Hard Drop":
                    HardDrop = key;
                    break;
                case "Rotate Clockwise":
                    RotateClockwise = key;
                    break;
                case "Rotate Counter-Clockwise":
                    RotateCounterClockwise = key;
                    break;
                case "Hold":
                    Hold = key;
                    break;
                case "Pause":
                    Pause = key;
                    break;
            }
        }

        public Dictionary<string, Keys> GetAllControls()
        {
            return new Dictionary<string, Keys>
            {
                ["Move Left"] = MoveLeft,
                ["Move Right"] = MoveRight,
                ["Move Down"] = MoveDown,
                ["Hard Drop"] = HardDrop,
                ["Rotate Clockwise"] = RotateClockwise,
                ["Rotate Counter-Clockwise"] = RotateCounterClockwise,
                ["Hold"] = Hold,
                ["Pause"] = Pause
            };
        }
    }
}
