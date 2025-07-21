using GameCore.Base;

namespace Tetris
{
    public class TetrisStatistics : BaseGameStatistics
    {
        public int LinesCleared { get; private set; }
        public int Level { get; private set; }
        public int Tetrominoes { get; private set; }
        public TimeSpan FastestLevel { get; private set; } = TimeSpan.MaxValue;

        public TetrisStatistics(string gameId) : base(gameId)
        {
        }

        public void UpdateLinesCleared(int lines)
        {
            LinesCleared += lines;
            AddScore(CalculateScore(lines, Level));
        }

        public void UpdateLevel(int level)
        {
            Level = level;
        }

        public void UpdateTetrominoes(int count)
        {
            Tetrominoes += count;
        }

        public void UpdateFastestLevel(TimeSpan time)
        {
            if (time < FastestLevel)
            {
                FastestLevel = time;
            }
        }

        private int CalculateScore(int lines, int level)
        {
            // Standard Tetris scoring
            return lines switch
            {
                1 => 40 * (level + 1),      // Single
                2 => 100 * (level + 1),     // Double
                3 => 300 * (level + 1),     // Triple
                4 => 1200 * (level + 1),    // Tetris
                _ => 0
            };
        }

        public override void Reset()
        {
            base.Reset();
            LinesCleared = 0;
            Level = 1;
            Tetrominoes = 0;
            FastestLevel = TimeSpan.MaxValue;
        }
    }
}
