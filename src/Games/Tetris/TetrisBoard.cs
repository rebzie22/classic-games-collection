using System.Drawing;

namespace Tetris
{
    public class TetrisBoard
    {
        public const int Width = 10;
        public const int Height = 20;
        public const int BufferHeight = 4; // Extra rows at top for spawning

        private readonly Color[,] _grid;

        public TetrisBoard()
        {
            _grid = new Color[Height + BufferHeight, Width];
            Clear();
        }

        public void Clear()
        {
            for (int row = 0; row < Height + BufferHeight; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    _grid[row, col] = Color.Empty;
                }
            }
        }

        public bool IsValidPosition(Tetromino tetromino)
        {
            foreach (var block in tetromino.GetBlockPositions())
            {
                // Check bounds
                if (block.X < 0 || block.X >= Width || block.Y >= Height + BufferHeight)
                    return false;

                // Check if position is occupied (only check if within board)
                if (block.Y >= 0 && _grid[block.Y, block.X] != Color.Empty)
                    return false;
            }
            return true;
        }

        public void PlaceTetromino(Tetromino tetromino)
        {
            foreach (var block in tetromino.GetBlockPositions())
            {
                if (block.Y >= 0 && block.Y < Height + BufferHeight && 
                    block.X >= 0 && block.X < Width)
                {
                    _grid[block.Y, block.X] = tetromino.Color;
                }
            }
        }

        public int ClearLines()
        {
            var linesCleared = 0;
            
            // Check each row from bottom to top
            for (int row = Height + BufferHeight - 1; row >= 0; row--)
            {
                if (IsLineFull(row))
                {
                    RemoveLine(row);
                    linesCleared++;
                    row++; // Check the same row again since lines moved down
                }
            }
            
            return linesCleared;
        }

        private bool IsLineFull(int row)
        {
            for (int col = 0; col < Width; col++)
            {
                if (_grid[row, col] == Color.Empty)
                    return false;
            }
            return true;
        }

        private void RemoveLine(int lineToRemove)
        {
            // Move all lines above down by one
            for (int row = lineToRemove; row > 0; row--)
            {
                for (int col = 0; col < Width; col++)
                {
                    _grid[row, col] = _grid[row - 1, col];
                }
            }

            // Clear the top line
            for (int col = 0; col < Width; col++)
            {
                _grid[0, col] = Color.Empty;
            }
        }

        public bool IsGameOver()
        {
            // Check if any blocks are in the buffer zone (top 4 rows)
            for (int row = 0; row < BufferHeight; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    if (_grid[row, col] != Color.Empty)
                        return true;
                }
            }
            return false;
        }

        public Color GetBlock(int row, int col)
        {
            if (row >= 0 && row < Height + BufferHeight && col >= 0 && col < Width)
                return _grid[row, col];
            return Color.Empty;
        }

        public Color[,] GetVisibleGrid()
        {
            var visibleGrid = new Color[Height, Width];
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    visibleGrid[row, col] = _grid[row + BufferHeight, col];
                }
            }
            return visibleGrid;
        }
    }
}
