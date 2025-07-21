using System.Drawing;

namespace Solitaire
{
    /// <summary>
    /// Represents a playing card in the Solitaire game
    /// </summary>
    public class Card
    {
        public enum Suit
        {
            Hearts,
            Diamonds,
            Clubs,
            Spades
        }
        
        public enum Rank
        {
            Ace = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6,
            Seven = 7,
            Eight = 8,
            Nine = 9,
            Ten = 10,
            Jack = 11,
            Queen = 12,
            King = 13
        }
        
        public Suit CardSuit { get; }
        public Rank CardRank { get; }
        public bool IsFaceUp { get; set; }
        public Point Position { get; set; }
        
        /// <summary>
        /// Gets the bounding rectangle of the card with dynamic sizing
        /// </summary>
        public Rectangle GetBounds(int width, int height) => new Rectangle(Position.X, Position.Y, width, height);
        
        /// <summary>
        /// Gets the bounding rectangle of the card with default sizing (for compatibility)
        /// </summary>
        public Rectangle Bounds => new Rectangle(Position.X, Position.Y, 60, 80);
        
        public Card(Suit suit, Rank rank)
        {
            CardSuit = suit;
            CardRank = rank;
            IsFaceUp = false;
            Position = Point.Empty;
        }
        
        /// <summary>
        /// Gets the color of the card (red for Hearts/Diamonds, black for Clubs/Spades)
        /// </summary>
        public Color Color => CardSuit == Suit.Hearts || CardSuit == Suit.Diamonds 
            ? System.Drawing.Color.Red 
            : System.Drawing.Color.Black;
        
        /// <summary>
        /// Gets the display string for the card rank
        /// </summary>
        public string RankDisplay => CardRank switch
        {
            Rank.Ace => "A",
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            _ => ((int)CardRank).ToString()
        };
        
        /// <summary>
        /// Gets the Unicode symbol for the card suit
        /// </summary>
        public string SuitSymbol => CardSuit switch
        {
            Suit.Hearts => "♥",
            Suit.Diamonds => "♦", 
            Suit.Clubs => "♣",
            Suit.Spades => "♠",
            _ => "?"
        };
        
        /// <summary>
        /// Checks if this card can be placed on another card in tableau (alternating colors, descending rank)
        /// </summary>
        public bool CanPlaceOnTableau(Card? other)
        {
            if (other == null) return CardRank == Rank.King;
            return other.Color != Color && (int)other.CardRank == (int)CardRank + 1;
        }
        
        /// <summary>
        /// Checks if this card can be placed on foundation pile (same suit, ascending rank)
        /// </summary>
        public bool CanPlaceOnFoundation(Card? other)
        {
            if (other == null) return CardRank == Rank.Ace;
            return CardSuit == other.CardSuit && (int)CardRank == (int)other.CardRank + 1;
        }
        
        /// <summary>
        /// Draws the card on the graphics surface with dynamic sizing
        /// </summary>
        public void Draw(Graphics g, int width, int height, bool isSelected = false)
        {
            var rect = GetBounds(width, height);
            
            if (IsFaceUp)
            {
                // Draw card background
                g.FillRectangle(Brushes.White, rect);
                g.DrawRectangle(isSelected ? Pens.Yellow : Pens.Black, rect);
                
                // Calculate font sizes based on card size
                var fontSize = Math.Max(8, width / 6);
                var centerFontSize = Math.Max(12, width / 3);
                
                // Draw rank and suit
                using var font = new Font("Arial", fontSize, FontStyle.Bold);
                using var brush = new SolidBrush(Color);
                
                // Top-left rank and suit
                g.DrawString(RankDisplay, font, brush, rect.X + 2, rect.Y + 2);
                g.DrawString(SuitSymbol, font, brush, rect.X + 2, rect.Y + fontSize + 4);
                
                // Bottom-right rank and suit (upside down)
                var transform = g.Transform;
                g.TranslateTransform(rect.Right, rect.Bottom);
                g.RotateTransform(180);
                g.DrawString(RankDisplay, font, brush, 2, 2);
                g.DrawString(SuitSymbol, font, brush, 2, fontSize + 4);
                g.Transform = transform;
                
                // Draw large suit symbol in center
                using var centerFont = new Font("Arial", centerFontSize, FontStyle.Bold);
                var symbolSize = g.MeasureString(SuitSymbol, centerFont);
                var centerX = rect.X + (rect.Width - symbolSize.Width) / 2;
                var centerY = rect.Y + (rect.Height - symbolSize.Height) / 2;
                g.DrawString(SuitSymbol, centerFont, brush, centerX, centerY);
            }
            else
            {
                // Draw card back
                g.FillRectangle(Brushes.DarkBlue, rect);
                g.DrawRectangle(isSelected ? Pens.Yellow : Pens.Black, rect);
                
                // Draw pattern on card back
                using var pen = new Pen(System.Drawing.Color.LightBlue, 2);
                for (int i = 0; i < rect.Width; i += Math.Max(4, rect.Width / 10))
                {
                    g.DrawLine(pen, rect.X + i, rect.Y, rect.X + i, rect.Bottom);
                }
                for (int i = 0; i < rect.Height; i += Math.Max(4, rect.Height / 10))
                {
                    g.DrawLine(pen, rect.X, rect.Y + i, rect.Right, rect.Y + i);
                }
            }
        }
        
        /// <summary>
        /// Draws the card on the graphics surface with default sizing (for compatibility)
        /// </summary>
        public void Draw(Graphics g, bool isSelected = false)
        {
            Draw(g, 60, 80, isSelected);
        }
    }
}