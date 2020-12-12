//-----------------------------------------------------------------------
// <copyright file="TileBag.cs" company="ColeSeanStevenCompany">
//     Company copyright tag.
// </copyright>
//-----------------------------------------------------------------------

namespace Game_Scrabble
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interaction logic form TileBag
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Encapsulation not yet taught.")]
    public class TileBag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TileBag"/> class.
        /// </summary>
        public TileBag()
        {
            this.Tiles = new List<Tile>();

            // Populate our tile bag for each letter
            int[] tileCount = new int[27] { 9, 2, 2, 4, 12, 2, 3, 2, 9, 1, 1, 4, 2, 6, 8, 2, 1, 6, 4, 6, 4, 2, 2, 1, 2, 1, 0 };
            string[] tileLetter = new string[27]
            {
                "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Wild"
            };

            // Loop through each letter
            for (int i = 0; i < 27; i++)
            {
                // Loop for how many tiles are supposed to be of that letter
                for (int j = 0; j < tileCount[i]; j++)
                {
                    Tile newTile = new Tile
                    {
                        Letter = tileLetter[i]
                    };

                    this.Tiles.Add(newTile);
                }
            }

            this.RandomTile = new Random();
        }

        /// <summary>
        /// Gets the tiles
        /// </summary>
        private List<Tile> Tiles { get; }

        /// <summary>
        /// Gets the random tile
        /// </summary>
        private Random RandomTile { get; }

        /// <summary>
        /// Draws a random tile from bag
        /// </summary>
        /// <returns> Returns Tile</returns>
        public Tile DrawRandomTileFromBag()
        {
            if (this.Tiles.Count > 0)
            {
                int tileNumber = this.RandomTile.Next(0, this.Tiles.Count - 1);

                Tile drawnTile = this.Tiles[tileNumber];

                this.Tiles.RemoveAt(tileNumber);

                return drawnTile;
            }
            else
            {
                return null;
            }
        }
    }
}
