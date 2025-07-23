using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace Solitaire
{
    /// <summary>
    /// Main game form for Solitaire
    /// </summary>
    public partial class SolitaireGameForm : Form
    {
        private readonly string _difficulty;
        private readonly SolitaireStatistics _statistics;
        private readonly System.Windows.Forms.Timer _gameTimer;
        private bool _isPaused = false;
        
        // Game state
        private int _score = 0;
        private int _moves = 0;
        private DateTime _startTime;
        
        // Card game logic
        private List<Card> _deck = new();
        private List<Card> _stock = new();
        private List<Card> _waste = new();
        private List<List<Card>> _tableau = new(); // 7 columns
        private List<List<Card>> _foundation = new(); // 4 suits
        
        private Card? _draggedCard = null;
        private List<Card> _draggedCards = new(); // For dragging sequences
        private Point _dragOffset;
        private bool _isDragging = false;
        
        // Double-click detection
        private Card? _lastClickedCard = null;
        private DateTime _lastClickTime = DateTime.MinValue;
        private const int DoubleClickTimeMs = 500;
        
        // UI Controls
        private Label _scoreLabel = null!;
        private Label _movesLabel = null!;
        private Label _timeLabel = null!;
        private Button _newGameButton = null!;
        private Button _pauseButton = null!;
        private Panel _gameArea = null!;
        
        // Card dimensions - will be calculated based on window size
        private int CardWidth => Math.Max(60, _gameArea.Width / 15);
        private int CardHeight => Math.Max(80, (int)(CardWidth * 1.33));
        private new int Margin => Math.Max(10, _gameArea.Width / 80);
        private int CardSpacing => Math.Max(15, CardHeight / 5);

        // Prevent duplicate win handling
        private int _hasHandledWin = 0; // 0 = not handled, 1 = handled
        
        public SolitaireGameForm(string difficulty, SolitaireStatistics statistics)
        {
            _difficulty = difficulty;
            _statistics = statistics;
            _startTime = DateTime.Now;
            
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 1000; // Update every second
            _gameTimer.Tick += OnTimerTick;
            
            InitializeComponent();
            InitializeGame();
            _gameTimer.Start();
        }
        
        private void InitializeComponent()
        {
            Text = "Solitaire - " + _difficulty;
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(0, 100, 0); // Dark green felt

            // Enable double buffering to reduce flicker
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            // Score panel
            var scorePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(20, 80, 20),
                Padding = new Padding(10)
            };

            _scoreLabel = new Label
            {
                Text = "Score: 0",
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, 20),
                AutoSize = true
            };

            _movesLabel = new Label
            {
                Text = "Moves: 0",
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(150, 20),
                AutoSize = true
            };

            _timeLabel = new Label
            {
                Text = "Time: 00:00",
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(280, 20),
                AutoSize = true
            };

            _newGameButton = new Button
            {
                Text = "New Game",
                Location = new Point(420, 15),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _newGameButton.Click += OnNewGameClick;

            _pauseButton = new Button
            {
                Text = "Pause",
                Location = new Point(510, 15),
                Size = new Size(60, 30),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _pauseButton.Click += OnPauseClick;

#if DEBUG
            var simulateWinButton = new Button
            {
                Text = "Simulate Win",
                Location = new Point(580, 15),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.Yellow,
                FlatStyle = FlatStyle.Flat
            };
            simulateWinButton.Click += (s, e) => SimulateWin();
#endif

            var controls = new List<Control> { _scoreLabel, _movesLabel, _timeLabel, _newGameButton, _pauseButton };
#if DEBUG
            controls.Add(simulateWinButton);
#endif
            scorePanel.Controls.AddRange(controls.ToArray());

            // Game area
            _gameArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(0, 100, 0)
            };

            // Enable double buffering for the game area
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, _gameArea, new object[] { true });

            _gameArea.Paint += OnGameAreaPaint;
            _gameArea.MouseDown += OnGameAreaMouseDown;
            _gameArea.MouseMove += OnGameAreaMouseMove;
            _gameArea.MouseUp += OnGameAreaMouseUp;
            _gameArea.Resize += OnGameAreaResize;

            Controls.Add(_gameArea);
            Controls.Add(scorePanel);
        }

#if DEBUG
        private void SimulateWin()
        {
            // Fill all foundation piles to 13 cards
            for (int i = 0; i < 4; i++)
            {
                while (_foundation[i].Count < 13)
                {
                    // Add dummy cards if needed
                    _foundation[i].Add(new Card(Card.Suit.Spades, Card.Rank.Ace));
                }
            }
            _moves = 20;
            CheckWinCondition();
        }
#endif
        
        private void InitializeGame()
        {
            System.Threading.Interlocked.Exchange(ref _hasHandledWin, 0);
            _score = GetDifficultyStartingScore();
            _moves = 0;
            // Initialize game collections
            InitializeCollections();
            // Create and shuffle deck
            CreateDeck();
            ShuffleDeck();
            // Deal cards
            DealCards();
            UpdateUI();
        }
        
        private void InitializeCollections()
        {
            _tableau.Clear();
            _foundation.Clear();
            
            // Initialize 7 tableau columns
            for (int i = 0; i < 7; i++)
            {
                _tableau.Add(new List<Card>());
            }
            
            // Initialize 4 foundation piles (one for each suit)
            for (int i = 0; i < 4; i++)
            {
                _foundation.Add(new List<Card>());
            }
        }
        
        private void CreateDeck()
        {
            _deck.Clear();
            
            // Create a standard 52-card deck
            foreach (Card.Suit suit in Enum.GetValues<Card.Suit>())
            {
                foreach (Card.Rank rank in Enum.GetValues<Card.Rank>())
                {
                    _deck.Add(new Card(suit, rank));
                }
            }
        }
        
        private void ShuffleDeck()
        {
            var random = new Random();
            for (int i = _deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
            }
        }
        
        private void DealCards()
        {
            int cardIndex = 0;
            var gameAreaTop = 10;
            var tableauStartY = gameAreaTop + CardHeight + Margin * 2;
            
            // Deal cards to tableau (7 columns, increasing number of cards)
            for (int col = 0; col < 7; col++)
            {
                for (int row = 0; row <= col; row++)
                {
                    var card = _deck[cardIndex++];
                    card.IsFaceUp = (row == col); // Only top card is face up
                    
                    // Position the card
                    var x = Margin + col * (CardWidth + Margin);
                    var y = tableauStartY + row * CardSpacing;
                    card.Position = new Point(x, y);
                    
                    _tableau[col].Add(card);
                }
            }
            
            // Remaining cards go to stock pile
            _stock.Clear();
            _waste.Clear();
            
            for (int i = cardIndex; i < _deck.Count; i++)
            {
                var card = _deck[i];
                card.IsFaceUp = false;
                card.Position = new Point(Margin, gameAreaTop);
                _stock.Add(card);
            }
        }
        
        private int GetDifficultyStartingScore()
        {
            return _difficulty switch
            {
                "Beginner" => 0,        // Standard scoring (was "Easy")
                "Intermediate" => 0,    // Standard scoring (was "Normal") 
                "Expert" => -52,        // Start negative - must move all cards to positive (was "Hard")
                // Legacy support for old difficulty names
                "Easy" => 0,
                "Normal" => 0,
                "Hard" => -52,
                _ => 0
            };
        }
        
        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (!_isPaused)
            {
                UpdateTimeDisplay();
            }
        }
        
        private void UpdateTimeDisplay()
        {
            var elapsed = DateTime.Now - _startTime;
            _timeLabel.Text = $"Time: {elapsed:mm\\:ss}";
        }
        
        private void UpdateUI()
        {
            _scoreLabel.Text = $"Score: {_score}";
            _movesLabel.Text = $"Moves: {_moves}";
            // Only update statistics score at the end of the game, not during play
        }
        
        private void OnNewGameClick(object? sender, EventArgs e)
        {
            _hasHandledWin = 0;
            InitializeGame();
            _startTime = DateTime.Now;
            _gameArea.Invalidate();
        }
        
        private void OnPauseClick(object? sender, EventArgs e)
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        
        public void PauseGame()
        {
            _isPaused = true;
            _pauseButton.Text = "Resume";
            _gameTimer.Stop();
        }
        
        public void ResumeGame()
        {
            _isPaused = false;
            _pauseButton.Text = "Pause";
            _gameTimer.Start();
        }
        
        private void OnGameAreaResize(object? sender, EventArgs e)
        {
            UpdateCardPositions();
            _gameArea.Invalidate();
        }
        
        private void UpdateCardPositions()
        {
            var gameAreaTop = 10;
            var tableauStartY = gameAreaTop + CardHeight + Margin * 2;
            
            // Update tableau card positions
            for (int col = 0; col < _tableau.Count; col++)
            {
                var x = Margin + col * (CardWidth + Margin);
                
                for (int cardIndex = 0; cardIndex < _tableau[col].Count; cardIndex++)
                {
                    var card = _tableau[col][cardIndex];
                    var y = tableauStartY + cardIndex * CardSpacing;
                    card.Position = new Point(x, y);
                }
            }
            
            // Update stock pile position
            foreach (var card in _stock)
            {
                card.Position = new Point(Margin, gameAreaTop);
            }
            
            // Update waste pile position
            var wasteX = Margin * 2 + CardWidth;
            foreach (var card in _waste)
            {
                card.Position = new Point(wasteX, gameAreaTop);
            }
        }
        
        private void OnGameAreaPaint(object? sender, PaintEventArgs e)
        {
            if (_isPaused)
            {
                // Draw pause overlay
                using var pauseBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
                e.Graphics.FillRectangle(pauseBrush, _gameArea.ClientRectangle);
                
                var pauseText = "PAUSED - Click Resume to continue";
                using var font = new Font("Arial", 24, FontStyle.Bold);
                var textSize = e.Graphics.MeasureString(pauseText, font);
                var x = (_gameArea.Width - textSize.Width) / 2;
                var y = (_gameArea.Height - textSize.Height) / 2;
                
                e.Graphics.DrawString(pauseText, font, Brushes.White, x, y);
                return;
            }
            
            DrawSolitaireGame(e.Graphics);
        }
        
        private void DrawSolitaireGame(Graphics g)
        {
            // Use high quality rendering
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            // Calculate the actual available game area (excluding the score panel)
            var gameAreaTop = 10; // Small margin from top of game area
            
            // Draw foundation piles (top right)
            for (int i = 0; i < 4; i++)
            {
                var x = _gameArea.Width - (4 - i) * (CardWidth + Margin) - Margin;
                var y = gameAreaTop;
                
                if (_foundation[i].Count > 0)
                {
                    var topCard = _foundation[i].Last();
                    topCard.Position = new Point(x, y);
                    topCard.Draw(g, CardWidth, CardHeight);
                }
                else
                {
                    DrawEmptySlot(g, x, y, "Foundation");
                }
            }
            
            // Draw stock pile
            if (_stock.Count > 0)
            {
                var stockCard = _stock.Last();
                stockCard.Position = new Point(Margin, gameAreaTop);
                stockCard.Draw(g, CardWidth, CardHeight);
                
                // Draw count indicator
                using var font = new Font("Arial", Math.Max(8, CardWidth / 8));
                g.DrawString(_stock.Count.ToString(), font, Brushes.White, 
                    Margin + 2, gameAreaTop + CardHeight + 2);
            }
            else
            {
                DrawEmptySlot(g, Margin, gameAreaTop, "Stock");
            }
            
            // Draw waste pile
            var wasteX = Margin * 2 + CardWidth;
            if (_waste.Count > 0)
            {
                var wasteCard = _waste.Last();
                wasteCard.Position = new Point(wasteX, gameAreaTop);
                wasteCard.Draw(g, CardWidth, CardHeight);
            }
            else
            {
                DrawEmptySlot(g, wasteX, gameAreaTop, "Waste");
            }
            
            // Draw tableau columns - position them below the top cards with proper spacing
            var tableauStartY = gameAreaTop + CardHeight + Margin * 2;
            
            for (int col = 0; col < 7; col++)
            {
                var x = Margin + col * (CardWidth + Margin);
                
                if (_tableau[col].Count == 0)
                {
                    DrawEmptySlot(g, x, tableauStartY, $"Col {col + 1}");
                }
                else
                {
                    for (int cardIndex = 0; cardIndex < _tableau[col].Count; cardIndex++)
                    {
                        var card = _tableau[col][cardIndex];
                        var y = tableauStartY + cardIndex * CardSpacing;
                        card.Position = new Point(x, y);
                        
                        // Don't draw the card if it's being dragged
                        if (!_draggedCards.Contains(card))
                        {
                            bool isSelected = false;
                            card.Draw(g, CardWidth, CardHeight, isSelected);
                        }
                    }
                }
            }
            
            // Draw dragged cards on top (highest z-order)
            if (_isDragging && _draggedCards.Count > 0)
            {
                // Use current mouse position for smooth dragging
                var mousePos = PointToClient(MousePosition);
                var dragPos = new Point(mousePos.X - _dragOffset.X, mousePos.Y - _dragOffset.Y);
                
                // Draw with a slight shadow effect for better visual feedback
                for (int i = 0; i < _draggedCards.Count; i++)
                {
                    var card = _draggedCards[i];
                    var cardPos = new Point(dragPos.X, dragPos.Y + (i * CardSpacing));
                    
                    // Draw shadow first
                    var shadowRect = new Rectangle(cardPos.X + 2, cardPos.Y + 2, CardWidth, CardHeight);
                    using var shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
                    g.FillRectangle(shadowBrush, shadowRect);
                    
                    // Draw the card
                    card.Position = cardPos;
                    card.Draw(g, CardWidth, CardHeight, true);
                }
            }
        }
        
        private void DrawEmptySlot(Graphics g, int x, int y, string label)
        {
            var rect = new Rectangle(x, y, CardWidth, CardHeight);
            g.DrawRectangle(Pens.Gray, rect);
            g.DrawRectangle(Pens.DarkGray, x + 1, y + 1, CardWidth - 2, CardHeight - 2);
            
            using var font = new Font("Arial", 8);
            var textSize = g.MeasureString(label, font);
            var textX = x + (CardWidth - textSize.Width) / 2;
            var textY = y + (CardHeight - textSize.Height) / 2;
            g.DrawString(label, font, Brushes.Gray, textX, textY);
        }
        
        private void OnGameAreaMouseDown(object? sender, MouseEventArgs e)
        {
            if (_isPaused || e.Button != MouseButtons.Left) return;
            
            var clickPoint = e.Location;
            var gameAreaTop = 10;
            
            // Check stock pile click
            var stockRect = new Rectangle(Margin, gameAreaTop, CardWidth, CardHeight);
            if (stockRect.Contains(clickPoint))
            {
                HandleStockClick();
                return;
            }
            
            // Check waste pile click - start drag if card available
            var wasteRect = new Rectangle(Margin * 2 + CardWidth, gameAreaTop, CardWidth, CardHeight);
            if (wasteRect.Contains(clickPoint) && _waste.Count > 0)
            {
                var topCard = _waste.Last();
                
                // Check for double-click
                if (IsDoubleClick(topCard))
                {
                    TryAutoMoveToFoundation(topCard);
                    return;
                }
                
                StartDrag(topCard, e.Location);
                return;
            }
            
            // Check tableau columns for drag start
            var tableauStartY = gameAreaTop + CardHeight + Margin * 2;
            
            for (int col = 0; col < 7; col++)
            {
                var x = Margin + col * (CardWidth + Margin);
                
                for (int cardIndex = _tableau[col].Count - 1; cardIndex >= 0; cardIndex--)
                {
                    var card = _tableau[col][cardIndex];
                    var cardBounds = card.GetBounds(CardWidth, CardHeight);
                    if (cardBounds.Contains(clickPoint))
                    {
                        if (card.IsFaceUp)
                        {
                            // Check for double-click
                            if (IsDoubleClick(card))
                            {
                                TryAutoMoveToFoundation(card);
                            }
                            else
                            {
                                StartDrag(card, e.Location);
                            }
                        }
                        else if (cardIndex == _tableau[col].Count - 1)
                        {
                            // Flip face-down card
                            card.IsFaceUp = true;
                            _moves++;
                            _statistics.RecordMove();
                            AddScore(5); // Flipping card gives 5 points
                            _gameArea.Invalidate();
                        }
                        return;
                    }
                }
            }
        }
        
        private void OnGameAreaMouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging && _draggedCard != null)
            {
                // Calculate the region that needs to be invalidated
                var oldBounds = Rectangle.Empty;
                var newBounds = Rectangle.Empty;
                
                // Calculate bounds for all dragged cards (old positions)
                for (int i = 0; i < _draggedCards.Count; i++)
                {
                    var card = _draggedCards[i];
                    var cardBounds = new Rectangle(card.Position.X - 2, card.Position.Y - 2, 
                        CardWidth + 4, CardHeight + 4);
                    oldBounds = oldBounds.IsEmpty ? cardBounds : Rectangle.Union(oldBounds, cardBounds);
                }
                
                // Calculate new position
                var newPos = new Point(e.X - _dragOffset.X, e.Y - _dragOffset.Y);
                
                // Calculate bounds for all dragged cards (new positions)
                for (int i = 0; i < _draggedCards.Count; i++)
                {
                    var cardPos = new Point(newPos.X, newPos.Y + (i * CardSpacing));
                    var cardBounds = new Rectangle(cardPos.X - 2, cardPos.Y - 2, 
                        CardWidth + 4, CardHeight + 4);
                    newBounds = newBounds.IsEmpty ? cardBounds : Rectangle.Union(newBounds, cardBounds);
                }
                
                // Update the primary dragged card position (others will be positioned in draw)
                _draggedCard.Position = newPos;
                
                // Invalidate the union of old and new bounds to minimize flicker
                var invalidateRegion = Rectangle.Union(oldBounds, newBounds);
                _gameArea.Invalidate(invalidateRegion);
            }
        }
        
        private void OnGameAreaMouseUp(object? sender, MouseEventArgs e)
        {
            if (!_isDragging || _draggedCard == null) return;
            
            var dropPoint = e.Location;
            bool validDrop = false;
            var gameAreaTop = 10;
            var tableauStartY = gameAreaTop + CardHeight + Margin * 2;
            
            // Check foundation piles for drop (only single cards allowed)
            if (_draggedCards.Count == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    var x = _gameArea.Width - (4 - i) * (CardWidth + Margin) - Margin;
                    var foundationRect = new Rectangle(x, gameAreaTop, CardWidth, CardHeight);
                    if (foundationRect.Contains(dropPoint))
                    {
                        var foundation = _foundation[i];
                        var topCard = foundation.Count > 0 ? foundation.Last() : null;
                        
                        if (_draggedCard.CanPlaceOnFoundation(topCard))
                        {
                            PerformMove(_draggedCard, foundation);
                            AddScore(10); // Foundation move gives 10 points
                            validDrop = true;
                        }
                        break;
                    }
                }
            }
            
            // Check tableau columns for drop
            if (!validDrop)
            {
                for (int col = 0; col < 7; col++)
                {
                    var x = Margin + col * (CardWidth + Margin);
                    var columnRect = new Rectangle(x, tableauStartY, CardWidth, CardHeight + (_tableau[col].Count * CardSpacing));
                    
                    if (columnRect.Contains(dropPoint))
                    {
                        var tableau = _tableau[col];
                        var topCard = tableau.Count > 0 ? tableau.Last() : null;
                        
                        if (_draggedCard.CanPlaceOnTableau(topCard))
                        {
                            PerformMove(_draggedCard, tableau);
                            AddScore(5); // Tableau move gives 5 points
                            validDrop = true;
                        }
                        break;
                    }
                }
            }
            
            // If not a valid drop, try auto-move to foundation (double-click behavior)
            if (!validDrop)
            {
                TryAutoMoveToFoundation(_draggedCard);
            }
            
            EndDrag();
            // Only check win condition once per user action
            CheckWinCondition();
        }
        
        private void HandleStockClick()
        {
            if (_stock.Count > 0)
            {
                // Move card from stock to waste
                var card = _stock.Last();
                _stock.RemoveAt(_stock.Count - 1);
                card.IsFaceUp = true;
                _waste.Add(card);
            }
            else if (_waste.Count > 0)
            {
                // Reset: move all waste cards back to stock
                while (_waste.Count > 0)
                {
                    var card = _waste.Last();
                    _waste.RemoveAt(_waste.Count - 1);
                    card.IsFaceUp = false;
                    _stock.Insert(0, card);
                }
                _moves++;
                _statistics.RecordMove();
            }
            _gameArea.Invalidate();
            UpdateUI();
            CheckWinCondition();
        }
        
        private void StartDrag(Card card, Point mouseLocation)
        {
            _draggedCard = card;
            _draggedCards.Clear();
            _draggedCards.Add(card);
            
            // If card is in tableau, check if we can drag a sequence
            for (int col = 0; col < 7; col++)
            {
                var tableau = _tableau[col];
                var cardIndex = tableau.IndexOf(card);
                
                if (cardIndex >= 0)
                {
                    // Add all cards from this position to the end (if they form a valid sequence)
                    _draggedCards.Clear();
                    for (int i = cardIndex; i < tableau.Count; i++)
                    {
                        var currentCard = tableau[i];
                        if (i == cardIndex || IsValidSequence(_draggedCards.Last(), currentCard))
                        {
                            _draggedCards.Add(currentCard);
                        }
                        else
                        {
                            break; // Stop if sequence is broken
                        }
                    }
                    break;
                }
            }
            
            _dragOffset = new Point(mouseLocation.X - card.Position.X, mouseLocation.Y - card.Position.Y);
            _isDragging = true;
            _gameArea.Invalidate();
        }
        
        private bool IsValidSequence(Card topCard, Card bottomCard)
        {
            // Valid sequence: descending rank, alternating colors
            return topCard.Color != bottomCard.Color && 
                   (int)topCard.CardRank == (int)bottomCard.CardRank + 1;
        }
        
        private bool IsDoubleClick(Card card)
        {
            var now = DateTime.Now;
            var isDoubleClick = _lastClickedCard == card && 
                               (now - _lastClickTime).TotalMilliseconds < DoubleClickTimeMs;
            
            _lastClickedCard = card;
            _lastClickTime = now;
            
            return isDoubleClick;
        }
        
        private void EndDrag()
        {
            if (_isDragging)
            {
                // Calculate the region that was occupied by dragged cards for invalidation
                if (_draggedCard != null && _draggedCards.Count > 0)
                {
                    var invalidateRegion = Rectangle.Empty;
                    var mousePos = PointToClient(MousePosition);
                    var dragPos = new Point(mousePos.X - _dragOffset.X, mousePos.Y - _dragOffset.Y);
                    
                    for (int i = 0; i < _draggedCards.Count; i++)
                    {
                        var cardPos = new Point(dragPos.X, dragPos.Y + (i * CardSpacing));
                        var cardBounds = new Rectangle(cardPos.X - 4, cardPos.Y - 4, 
                            CardWidth + 8, CardHeight + 8); // Include shadow
                        invalidateRegion = invalidateRegion.IsEmpty ? cardBounds : Rectangle.Union(invalidateRegion, cardBounds);
                    }
                    
                    _gameArea.Invalidate(invalidateRegion);
                }
                
                _draggedCard = null;
                _draggedCards.Clear();
                _isDragging = false;
                
                // Full invalidate to ensure clean state
                _gameArea.Invalidate();
            }
        }
        
        private void PerformMove(Card card, List<Card> destination)
        {
            // If we're moving multiple cards, move them all
            var cardsToMove = _draggedCards.Count > 1 ? _draggedCards.ToList() : new List<Card> { card };
            foreach (var cardToMove in cardsToMove)
            {
                // Remove card from its current location
                if (_waste.Contains(cardToMove))
                {
                    _waste.Remove(cardToMove);
                }
                else
                {
                    foreach (var tableau in _tableau)
                    {
                        if (tableau.Contains(cardToMove))
                        {
                            tableau.Remove(cardToMove);
                            break;
                        }
                    }
                }
                // Only add to foundation if eligible
                if (destination == null)
                    continue;
                if (_foundation.Contains(destination))
                {
                    var topCard = destination.Count > 0 ? destination.Last() : null;
                    if (cardToMove.CanPlaceOnFoundation(topCard))
                        destination.Add(cardToMove);
                }
                else
                {
                    destination.Add(cardToMove);
                }
            }
            _moves++;
            _statistics.RecordMove();
            UpdateUI();
            // Do NOT call CheckWinCondition here!
        }
        
        private void TryAutoMoveToFoundation(Card card)
        {
            for (int i = 0; i < 4; i++)
            {
                var foundation = _foundation[i];
                var topCard = foundation.Count > 0 ? foundation.Last() : null;
                if (card.CanPlaceOnFoundation(topCard))
                {
                    PerformMove(card, foundation);
                    AddScore(10);
                    CheckWinCondition();
                    return;
                }
            }
        }
        
        private void AddScore(int points)
        {
            _score += points;
            // Don't trigger score change events until game ends
            UpdateUI();
        }
        
        private void CheckWinCondition()
        {
            // Ensure foundation is properly initialized
            if (_foundation == null || _foundation.Count != 4)
                return;

            // Check if all foundations have 13 cards (complete suits)
            var totalFoundationCards = _foundation.Sum(f => f?.Count ?? 0);

            // DEBUG: Add console output to see what's happening
            System.Diagnostics.Debug.WriteLine($"Foundation cards: {totalFoundationCards}, Moves: {_moves}");

            // Only trigger win if we legitimately have all 52 cards in foundations
            // and the game has actually progressed (more than just a few moves)
            if (totalFoundationCards == 52 && _moves > 10)
            {
                // Atomically set _hasHandledWin to 1, only proceed if it was 0
                if (System.Threading.Interlocked.Exchange(ref _hasHandledWin, 1) == 0)
                {
                    HandleWinAsync();
                }
            }
        }

        // Async win handler to avoid UI freeze
        private async void HandleWinAsync()
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] HandleWinAsync called at {DateTime.Now:HH:mm:ss.fff} | _hasHandledWin={_hasHandledWin}");
            _gameTimer.Stop();
            var gameTime = DateTime.Now - _startTime;
            // Bonus points for time
            var timeBonus = Math.Max(0, 300 - (int)gameTime.TotalSeconds);
            AddScore(timeBonus);

            MessageBox.Show($"Congratulations! You won!\nScore: {_score}\nTime: {gameTime:mm\\:ss}\nMoves: {_moves}",
                "Game Won!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Prompt for player name and save high score
            try
            {
                var playerNameDialogType = Type.GetType("GameLauncher.Forms.PlayerNameDialog, GameLauncher");
                if (playerNameDialogType != null)
                {
                    using (var dialog = (Form)Activator.CreateInstance(playerNameDialogType)!)
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            var playerNameProp = playerNameDialogType.GetProperty("PlayerName");
                            var playerName = playerNameProp?.GetValue(dialog)?.ToString() ?? "Player";
                            // Find ScoreService via MainForm (assumes singleton or static access)
                            var mainFormType = Type.GetType("GameLauncher.Forms.MainForm, GameLauncher");
                            var openForms = Application.OpenForms;
                            Form? mainForm = null;
                            foreach (Form f in openForms)
                            {
                                if (f.GetType().FullName == "GameLauncher.Forms.MainForm")
                                {
                                    mainForm = f;
                                    break;
                                }
                            }
                            if (mainForm != null)
                            {
                                var scoreServiceField = mainFormType!.GetField("_scoreService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                var scoreService = scoreServiceField?.GetValue(mainForm);
                                if (scoreService != null)
                                {
                                    // Check if this is a new high score (async)
                                    var getTopScoresAsync = scoreService.GetType().GetMethod("GetTopScoresAsync");
                                    if (getTopScoresAsync != null)
                                    {
                                        dynamic topScoresTask = getTopScoresAsync.Invoke(scoreService, new object[] { "solitaire", 1 });
                                        await topScoresTask;
                                        var topScores = topScoresTask.Result as System.Collections.IEnumerable;
                                        double? bestScore = null;
                                        double? bestTime = null;
                                        if (topScores != null)
                                        {
                                            foreach (var entry in topScores)
                                            {
                                            var entryScore = (double)entry.GetType().GetProperty("Score")!.GetValue(entry)!;
                                            bestScore = entryScore;
                                            break; // Only need the top one
                                            }
                                        }
                                        bool isNewHigh = false;
                                        if (bestScore == null || _score > bestScore)
                                            isNewHigh = true;
                                        // Time property removed from ScoreEntry; only compare score

                                        if (isNewHigh)
                                        {
                                            // Create ScoreEntry and save via ScoreService
                                            var scoreEntryType = Type.GetType("GameCore.Models.ScoreEntry, GameCore");
                                            var scoreEntry = Activator.CreateInstance(scoreEntryType!);
                                            scoreEntryType!.GetProperty("GameId")!.SetValue(scoreEntry, "solitaire");
                                            scoreEntryType.GetProperty("PlayerName")!.SetValue(scoreEntry, playerName);
                                            scoreEntryType.GetProperty("Score")!.SetValue(scoreEntry, _score);
                                            scoreEntryType.GetProperty("AchievedAt")!.SetValue(scoreEntry, DateTime.UtcNow);
                                            var addScoreAsync = scoreService.GetType().GetMethod("AddScoreAsync");
                                            System.Diagnostics.Debug.WriteLine($"[DEBUG] About to call AddScoreAsync for player={playerName}, score={_score}, time={gameTime.TotalSeconds}");
                                            if (addScoreAsync != null)
                                            {
                                                dynamic addScoreTask = addScoreAsync.Invoke(scoreService, new object[] { scoreEntry });
                                                await addScoreTask;
                                                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddScoreAsync completed for player={playerName}, score={_score}, time={gameTime.TotalSeconds}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save high score: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Only call EndGame once - this handles statistics, but NOT high score saving (handled above)
            _statistics.EndGame(true, _score); // No longer triggers high score save

            InitializeGame();
            _gameTimer.Start();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gameTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
