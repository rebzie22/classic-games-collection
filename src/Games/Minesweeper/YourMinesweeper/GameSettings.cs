using System;

namespace Minesweeper.YourMinesweeper
{
    public enum GameState
    {
        NotStarted,
        Playing,
        Won,
        Lost
    }

    public enum Difficulty
    {
        Beginner,     // 9x9, 10 mines
        Intermediate, // 16x16, 40 mines
        Expert        // 30x16, 99 mines
    }

    public class GameSettings
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int MineCount { get; set; }

        // Static factory methods for the adapter
        public static GameSettings Beginner() => new GameSettings { Rows = 9, Columns = 9, MineCount = 10 };
        public static GameSettings Intermediate() => new GameSettings { Rows = 16, Columns = 16, MineCount = 40 };
        public static GameSettings Expert() => new GameSettings { Rows = 16, Columns = 30, MineCount = 99 };

        public static GameSettings GetSettings(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Beginner => new GameSettings { Rows = 9, Columns = 9, MineCount = 10 },
                Difficulty.Intermediate => new GameSettings { Rows = 16, Columns = 16, MineCount = 40 },
                Difficulty.Expert => new GameSettings { Rows = 16, Columns = 30, MineCount = 99 },
                _ => new GameSettings { Rows = 9, Columns = 9, MineCount = 10 }
            };
        }
    }
}
