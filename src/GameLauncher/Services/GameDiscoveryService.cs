using GameCore.Interfaces;
using Snake;
using Tetris;
using Minesweeper;
using Solitaire;

namespace GameLauncher.Services
{
    public class GameDiscoveryService
    {
        private List<IGame> _availableGames = new List<IGame>();
        
        public IReadOnlyList<IGame> AvailableGames => _availableGames.AsReadOnly();

        public async Task<bool> DiscoverGamesAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    _availableGames.Clear();
                    
                    // Add all games back
                    _availableGames.Add(new SnakeGame());
                    _availableGames.Add(new TetrisGame());
                    _availableGames.Add(new MinesweeperGameAdapter());
                    _availableGames.Add(new SolitaireGame());
                });
                
                return true;
            }
            catch (Exception ex)
            {
                // Log error but continue with available games
                System.Diagnostics.Debug.WriteLine($"Error discovering games: {ex.Message}");
                return false;
            }
        }

        public IGame? CreateGameInstance(string gameId)
        {
            try
            {
                return gameId.ToLower() switch
                {
                    "snake" => new SnakeGame(),
                    "tetris" => new TetrisGame(),
                    "minesweeper" => new MinesweeperGameAdapter(),
                    "solitaire" => new SolitaireGame(),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating game instance for {gameId}: {ex.Message}");
                return null;
            }
        }

        public IEnumerable<IGame> DiscoverGames()
        {
            return _availableGames;
        }
    }
}
