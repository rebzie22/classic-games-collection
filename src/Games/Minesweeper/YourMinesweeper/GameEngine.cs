using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.YourMinesweeper
{
    public class GameEngine
    {
        private Cell[,] _grid = null!;
        private GameSettings _settings;
        private Random _random;
        private bool _firstClick;

        public int Rows => _settings.Rows;
        public int Columns => _settings.Columns;
        public int MineCount => _settings.MineCount;
        public GameState State { get; private set; }
        public int FlaggedMines { get; private set; }
        public int RevealedCells { get; private set; }

        public event EventHandler<GameStateChangedEventArgs>? GameStateChanged;

        public GameEngine(GameSettings settings)
        {
            _settings = settings;
            _random = new Random();
            InitializeGame();
        }

        public void InitializeGame()
        {
            _grid = new Cell[_settings.Rows, _settings.Columns];
            State = GameState.NotStarted;
            FlaggedMines = 0;
            RevealedCells = 0;
            _firstClick = true;

            // Initialize all cells
            for (int row = 0; row < _settings.Rows; row++)
            {
                for (int col = 0; col < _settings.Columns; col++)
                {
                    _grid[row, col] = new Cell(row, col);
                }
            }
        }

        public Cell? GetCell(int row, int column)
        {
            if (row >= 0 && row < _settings.Rows && column >= 0 && column < _settings.Columns)
                return _grid[row, column];
            return null;
        }

        public bool LeftClick(int row, int column)
        {
            var cell = GetCell(row, column);
            if (cell == null || cell.IsRevealed || cell.IsFlagged)
                return false;

            if (_firstClick)
            {
                PlaceMines(row, column);
                CalculateAdjacentMines();
                State = GameState.Playing;
                _firstClick = false;
                OnGameStateChanged();
            }

            if (cell.IsMine)
            {
                State = GameState.Lost;
                RevealAllMines();
                OnGameStateChanged();
                return false;
            }

            RevealCell(row, column);
            CheckWinCondition();
            return true;
        }

        public void RightClick(int row, int column)
        {
            var cell = GetCell(row, column);
            if (cell == null || cell.IsRevealed)
                return;

            if (cell.IsFlagged)
            {
                cell.IsFlagged = false;
                FlaggedMines--;
            }
            else
            {
                cell.IsFlagged = true;
                FlaggedMines++;
            }
        }

        private void PlaceMines(int excludeRow, int excludeColumn)
        {
            var cellList = new List<(int row, int col)>();
            
            // Create list of all cells except the first clicked cell and its neighbors
            for (int row = 0; row < _settings.Rows; row++)
            {
                for (int col = 0; col < _settings.Columns; col++)
                {
                    if (Math.Abs(row - excludeRow) <= 1 && Math.Abs(col - excludeColumn) <= 1)
                        continue; // Skip first click and its neighbors
                    
                    cellList.Add((row, col));
                }
            }

            // Randomly place mines
            for (int i = 0; i < _settings.MineCount && cellList.Count > 0; i++)
            {
                int index = _random.Next(cellList.Count);
                var (row, col) = cellList[index];
                _grid[row, col].IsMine = true;
                cellList.RemoveAt(index);
            }
        }

        private void CalculateAdjacentMines()
        {
            for (int row = 0; row < _settings.Rows; row++)
            {
                for (int col = 0; col < _settings.Columns; col++)
                {
                    if (!_grid[row, col].IsMine)
                    {
                        _grid[row, col].AdjacentMines = CountAdjacentMines(row, col);
                    }
                }
            }
        }

        private int CountAdjacentMines(int row, int column)
        {
            int count = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    
                    var neighbor = GetCell(row + i, column + j);
                    if (neighbor?.IsMine == true)
                        count++;
                }
            }
            return count;
        }

        private void RevealCell(int row, int column)
        {
            var cell = GetCell(row, column);
            if (cell == null || cell.IsRevealed || cell.IsFlagged)
                return;

            cell.IsRevealed = true;
            RevealedCells++;

            // If cell has no adjacent mines, reveal all neighbors
            if (cell.AdjacentMines == 0)
            {
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0) continue;
                        RevealCell(row + i, column + j);
                    }
                }
            }
        }

        private void RevealAllMines()
        {
            for (int row = 0; row < _settings.Rows; row++)
            {
                for (int col = 0; col < _settings.Columns; col++)
                {
                    var cell = _grid[row, col];
                    if (cell.IsMine)
                    {
                        cell.IsRevealed = true;
                    }
                }
            }
        }

        private void CheckWinCondition()
        {
            int totalCells = _settings.Rows * _settings.Columns;
            int cellsToReveal = totalCells - _settings.MineCount;

            if (RevealedCells >= cellsToReveal)
            {
                State = GameState.Won;
                OnGameStateChanged();
            }
        }

        protected virtual void OnGameStateChanged()
        {
            GameStateChanged?.Invoke(this, new GameStateChangedEventArgs(State));
        }
    }

    public class GameStateChangedEventArgs : EventArgs
    {
        public GameState NewState { get; }

        public GameStateChangedEventArgs(GameState newState)
        {
            NewState = newState;
        }
    }
}
