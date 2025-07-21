using Xunit;
using FluentAssertions;
using Solitaire;

namespace ClassicGamesCollection.Tests.Games.Solitaire
{
    public class CardTests
    {
        [Fact]
        public void Card_Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var card = new Card(Card.Suit.Hearts, Card.Rank.Ace);
            
            // Assert
            card.CardSuit.Should().Be(Card.Suit.Hearts);
            card.CardRank.Should().Be(Card.Rank.Ace);
            card.IsFaceUp.Should().BeFalse();
            card.Color.Should().Be(Card.CardColor.Red);
        }
        
        [Theory]
        [InlineData(Card.Suit.Hearts, Card.CardColor.Red)]
        [InlineData(Card.Suit.Diamonds, Card.CardColor.Red)]
        [InlineData(Card.Suit.Clubs, Card.CardColor.Black)]
        [InlineData(Card.Suit.Spades, Card.CardColor.Black)]
        public void Card_Color_ShouldReturnCorrectColor(Card.Suit suit, Card.CardColor expectedColor)
        {
            // Arrange
            var card = new Card(suit, Card.Rank.Ace);
            
            // Act & Assert
            card.Color.Should().Be(expectedColor);
        }
        
        [Fact]
        public void CanPlaceOnFoundation_WithNullTopCard_ShouldOnlyAllowAce()
        {
            // Arrange
            var ace = new Card(Card.Suit.Hearts, Card.Rank.Ace);
            var king = new Card(Card.Suit.Hearts, Card.Rank.King);
            
            // Act & Assert
            ace.CanPlaceOnFoundation(null).Should().BeTrue();
            king.CanPlaceOnFoundation(null).Should().BeFalse();
        }
        
        [Fact]
        public void CanPlaceOnFoundation_WithTopCard_ShouldRequireSameSuitAndNextRank()
        {
            // Arrange
            var aceHearts = new Card(Card.Suit.Hearts, Card.Rank.Ace);
            var twoHearts = new Card(Card.Suit.Hearts, Card.Rank.Two);
            var twoSpades = new Card(Card.Suit.Spades, Card.Rank.Two);
            var threeHearts = new Card(Card.Suit.Hearts, Card.Rank.Three);
            
            // Act & Assert
            twoHearts.CanPlaceOnFoundation(aceHearts).Should().BeTrue();
            twoSpades.CanPlaceOnFoundation(aceHearts).Should().BeFalse();
            threeHearts.CanPlaceOnFoundation(aceHearts).Should().BeFalse();
        }
        
        [Fact]
        public void CanPlaceOnTableau_WithNullTopCard_ShouldOnlyAllowKing()
        {
            // Arrange
            var king = new Card(Card.Suit.Hearts, Card.Rank.King);
            var queen = new Card(Card.Suit.Spades, Card.Rank.Queen);
            
            // Act & Assert
            king.CanPlaceOnTableau(null).Should().BeTrue();
            queen.CanPlaceOnTableau(null).Should().BeFalse();
        }
        
        [Fact]
        public void CanPlaceOnTableau_WithTopCard_ShouldRequireAlternatingColorAndDescendingRank()
        {
            // Arrange
            var kingHearts = new Card(Card.Suit.Hearts, Card.Rank.King);
            var queenSpades = new Card(Card.Suit.Spades, Card.Rank.Queen);
            var queenHearts = new Card(Card.Suit.Hearts, Card.Rank.Queen);
            var jackSpades = new Card(Card.Suit.Spades, Card.Rank.Jack);
            
            // Act & Assert
            queenSpades.CanPlaceOnTableau(kingHearts).Should().BeTrue();
            queenHearts.CanPlaceOnTableau(kingHearts).Should().BeFalse();
            jackSpades.CanPlaceOnTableau(kingHearts).Should().BeFalse();
        }
        
        [Fact]
        public void GetBounds_ShouldReturnCorrectRectangle()
        {
            // Arrange
            var card = new Card(Card.Suit.Hearts, Card.Rank.Ace);
            card.Position = new System.Drawing.Point(10, 20);
            
            // Act
            var bounds = card.GetBounds(100, 140);
            
            // Assert
            bounds.X.Should().Be(10);
            bounds.Y.Should().Be(20);
            bounds.Width.Should().Be(100);
            bounds.Height.Should().Be(140);
        }
        
        [Theory]
        [InlineData(Card.Rank.Ace, 1)]
        [InlineData(Card.Rank.Two, 2)]
        [InlineData(Card.Rank.Jack, 11)]
        [InlineData(Card.Rank.Queen, 12)]
        [InlineData(Card.Rank.King, 13)]
        public void CardRank_IntValue_ShouldBeCorrect(Card.Rank rank, int expectedValue)
        {
            // Arrange
            var card = new Card(Card.Suit.Hearts, rank);
            
            // Act & Assert
            ((int)card.CardRank).Should().Be(expectedValue);
        }
    }
}
