using System.Drawing;
using System.Reflection;

namespace GameLauncher.Helpers
{
    /// <summary>
    /// Helper class for loading embedded image resources
    /// </summary>
    public static class ImageHelper
    {
        private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
        
        /// <summary>
        /// Load an image from embedded resources
        /// </summary>
        /// <param name="imageName">Name of the image file (e.g., "minesweeper.png")</param>
        /// <returns>Image object or null if not found</returns>
        public static Image? LoadGameIcon(string imageName)
        {
            try
            {
                string resourceName = $"GameLauncher.Assets.Images.{imageName}";
                using var stream = _assembly.GetManifestResourceStream(resourceName);
                
                if (stream != null)
                {
                    return Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                // Log error in production, for now just return null
                System.Diagnostics.Debug.WriteLine($"Failed to load image {imageName}: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Create a default game icon if no image is available
        /// </summary>
        /// <param name="gameInitial">First letter of the game name</param>
        /// <returns>A simple bitmap with the game initial</returns>
        public static Image CreateDefaultIcon(char gameInitial, Color backgroundColor)
        {
            var bitmap = new Bitmap(64, 64);
            using var graphics = Graphics.FromImage(bitmap);
            
            // Fill background
            using var backgroundBrush = new SolidBrush(backgroundColor);
            graphics.FillRectangle(backgroundBrush, 0, 0, 64, 64);
            
            // Draw border
            using var borderPen = new Pen(Color.DarkGray, 2);
            graphics.DrawRectangle(borderPen, 1, 1, 62, 62);
            
            // Draw letter
            using var font = new Font("Arial", 24, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.White);
            
            var text = gameInitial.ToString().ToUpper();
            var textSize = graphics.MeasureString(text, font);
            var x = (64 - textSize.Width) / 2;
            var y = (64 - textSize.Height) / 2;
            
            graphics.DrawString(text, font, textBrush, x, y);
            
            return bitmap;
        }
    }
}
