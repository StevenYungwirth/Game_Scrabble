//-----------------------------------------------------------------------
// <copyright file="Player.cs" company="ColeSeanStevenCompany">
//     Company copyright tag.
// </copyright>
//-----------------------------------------------------------------------

namespace Game_Scrabble
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Contains interaction logic for Player
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Encapsulation not yet taught.")]
    public class Player
    {
        /// <summary>
        /// The player's number
        /// </summary>
        public int Number;

        /// <summary>
        /// Tracks the player's score.
        /// </summary>
        public int Score;

        /// <summary>
        /// The player's maximum hand size.
        /// </summary>
        private const int HANDSIZE = 7;

        /// <summary>
        /// Initializes a new instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="playerNumber">The player's number</param>
        public Player(int playerNumber)
        {
            this.TileBar = new List<Tile>();
            this.Number = playerNumber;
        }

        /// <summary>
        /// Gets or sets the player's tiles.
        /// </summary>
        public List<Tile> TileBar { get; set; }

        /// <summary>
        /// Adds the wordValue to the player's score.
        /// </summary>
        /// <param name="word">The word to add to the score</param>
        /// <param name="letterBonus">The factor to multiply the letter's score by</param>
        /// <param name="wordBonus">The factor to multiply the word's score by</param>
        public void AddToScore(string word, List<int> letterBonus, int wordBonus)
        {
            int score = 0;

            for (int letter = 0; letter < word.Length; letter++)
            {
                score += this.LetterToPointValue(word[letter]) * letterBonus[letter];
            }

            this.Score += score * wordBonus;
        }

        /// <summary>
        /// Refills the player's tray.
        /// </summary>
        /// <param name="tileBag"> The tile bag to fill </param>
        public void DrawTilesToMax(TileBag tileBag)
        {
            // Get count of tiles in tile bar
            int currentTileCount = this.TileBar.Count;

            for (int i = currentTileCount; i < 7; i++)
            {
                Tile drawnTile = new Tile();
                drawnTile = tileBag.DrawRandomTileFromBag();
                if (drawnTile == null)
                {
                    return;
                }

                this.AddTileToBar(drawnTile);
            }
        }

        /// <summary>
        /// adds a letter to the player's tray.
        /// </summary>
        /// <param name="tile"> The tile </param>
        public void AddTileToBar(Tile tile)
        {
            this.TileBar.Add(tile);

            if (tile != null)
            {
                // Get the board window
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(BoardWindow))
                    {
                        // Find the first open space
                        int tileLocation = -1;
                        int space = 1;
                        while (space <= HANDSIZE && tileLocation == -1)
                        {
                            Label label = (Label)window.FindName("player" + this.Number + "Letter" + space);
                            if (!label.IsVisible)
                            {
                                tileLocation = space;
                            }

                            space++;
                        }

                        if (tileLocation != -1)
                        {
                            // Set the tile's location
                            tile.Location = (Grid)window.FindName("player" + this.Number + "Tile" + tileLocation);
                            tile.Label = (Label)window.FindName("player" + this.Number + "Letter" + tileLocation);

                            // Add the letter
                            tile.Label.Content = tile.Letter;
                            tile.Label.Visibility = Visibility.Visible;

                            // Add the letter's point value
                            Label pointLabel = (Label)window.FindName("player" + this.Number + "Letter" + tileLocation + "Value");
                            pointLabel.Content = tile.PointValue;
                            pointLabel.Visibility = Visibility.Visible;

                            // Make sure the tile is visible
                            Grid tileGrid = (Grid)window.FindName("player" + this.Number + "Tile" + tileLocation);
                            tileGrid.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts the characters in wordValue into a point value.
        /// </summary>
        /// <returns>Returns an integer</returns>
        /// <param name="letter">The letter to convert into a point value</param>
        private int LetterToPointValue(char letter)
        {
            Tile letterTile = new Tile();
            letterTile.Letter = letter.ToString();
            return letterTile.PointValue;
        }
    }
}