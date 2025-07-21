using GameCore.Base;

namespace Snake
{
    public class SnakeStatistics : BaseGameStatistics
    {
        public int TotalFoodEaten { get; set; }
        public int LongestSnake { get; set; }
        public TimeSpan FastestTime { get; set; } = TimeSpan.MaxValue;

        public SnakeStatistics(string gameId) : base(gameId)
        {
        }

        public void UpdateFoodEaten(int foodCount)
        {
            TotalFoodEaten += foodCount;
        }

        public void UpdateLongestSnake(int length)
        {
            if (length > LongestSnake)
            {
                LongestSnake = length;
            }
        }

        public void UpdateFastestTime(TimeSpan gameTime)
        {
            if (gameTime < FastestTime)
            {
                FastestTime = gameTime;
            }
        }
    }
}
