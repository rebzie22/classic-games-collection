using System.Drawing;

namespace Tetris
{
    public enum TetrominoType
    {
        I, O, T, S, Z, J, L
    }

    public class Tetromino
    {
        public TetrominoType Type { get; }
        public Color Color { get; }
        public Point Position { get; set; }
        public int Rotation { get; set; }
        public bool[,] Shape { get; private set; }

        private static readonly Dictionary<TetrominoType, bool[,]> Shapes = new()
        {
            [TetrominoType.I] = new bool[,] { { true, true, true, true } },
            [TetrominoType.O] = new bool[,] { { true, true }, { true, true } },
            [TetrominoType.T] = new bool[,] { { false, true, false }, { true, true, true } },
            [TetrominoType.S] = new bool[,] { { false, true, true }, { true, true, false } },
            [TetrominoType.Z] = new bool[,] { { true, true, false }, { false, true, true } },
            [TetrominoType.J] = new bool[,] { { true, false, false }, { true, true, true } },
            [TetrominoType.L] = new bool[,] { { false, false, true }, { true, true, true } }
        };

        private static readonly Dictionary<TetrominoType, Color> Colors = new()
        {
            [TetrominoType.I] = Color.Cyan,
            [TetrominoType.O] = Color.Yellow,
            [TetrominoType.T] = Color.Purple,
            [TetrominoType.S] = Color.Green,
            [TetrominoType.Z] = Color.Red,
            [TetrominoType.J] = Color.Blue,
            [TetrominoType.L] = Color.Orange
        };

        public Tetromino(TetrominoType type)
        {
            Type = type;
            Color = Colors[type];
            Position = new Point(4, 0); // Start at top center
            Rotation = 0;
            Shape = (bool[,])Shapes[type].Clone();
        }

        public void RotateClockwise()
        {
            var rows = Shape.GetLength(0);
            var cols = Shape.GetLength(1);
            var rotated = new bool[cols, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    rotated[j, rows - 1 - i] = Shape[i, j];
                }
            }

            Shape = rotated;
            Rotation = (Rotation + 1) % 4;
        }

        public void RotateCounterClockwise()
        {
            var rows = Shape.GetLength(0);
            var cols = Shape.GetLength(1);
            var rotated = new bool[cols, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    rotated[cols - 1 - j, i] = Shape[i, j];
                }
            }

            Shape = rotated;
            Rotation = (Rotation + 3) % 4; // -1 mod 4 = 3
        }

        public Tetromino Clone()
        {
            var clone = new Tetromino(Type)
            {
                Position = Position,
                Rotation = Rotation,
                Shape = (bool[,])Shape.Clone()
            };
            return clone;
        }

        public IEnumerable<Point> GetBlockPositions()
        {
            var blocks = new List<Point>();
            for (int row = 0; row < Shape.GetLength(0); row++)
            {
                for (int col = 0; col < Shape.GetLength(1); col++)
                {
                    if (Shape[row, col])
                    {
                        blocks.Add(new Point(Position.X + col, Position.Y + row));
                    }
                }
            }
            return blocks;
        }
    }
}
