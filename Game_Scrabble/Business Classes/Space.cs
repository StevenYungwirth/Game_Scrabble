//-----------------------------------------------------------------------
// <copyright file="Space.cs" company="ColeSeanStevenCompany">
//     Company copyright tag.
// </copyright>
//-----------------------------------------------------------------------

namespace Game_Scrabble
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// The class used to represent a space.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Encapsulation not yet taught.")]
    public class Space
    {
        /// <summary>
        /// The space's grid.
        /// </summary>
        public Grid SpaceGrid;

        /// <summary>
        /// The tile.
        /// </summary>
        public Tile Tile;

        /// <summary>
        /// The space's x and y position
        /// </summary>
        public Point XYPosition;

        /// <summary>
        /// The triple word spaces
        /// </summary>
        private readonly int[] tripleWord = new int[] { 0, 7, 14, 105, 119, 210, 217, 224 };

        /// <summary>
        /// The double word spaces
        /// </summary>
        private readonly int[] doubleWord = new int[] { 16, 28, 32, 42, 48, 56, 64, 70, 112, 154, 160, 168, 176, 182, 192, 196, 208 };

        /// <summary>
        /// The triple letter spaces
        /// </summary>
        private readonly int[] tripleLetter = new int[] { 20, 24, 76, 80, 84, 88, 136, 140, 144, 148, 200, 204 };

        /// <summary>
        /// The double letter spaces
        /// </summary>
        private readonly int[] doubleLetter = new int[] { 3, 11, 36, 38, 45, 52, 59, 92, 96, 98, 102, 108, 116, 122, 126, 128, 132, 165, 172, 179, 186, 188, 213, 221 };
        
        /// <summary>
        /// Gets the factor to multiply a letter's point value by.
        /// </summary>
        public int LetterBonus
        {
            get
            {
                int spaceNumber = int.Parse(this.SpaceGrid.Name.Substring(this.SpaceGrid.Name.IndexOf("spaceGrid") + 9));
                foreach (int specialSpace in this.tripleLetter)
                {
                    if (specialSpace == spaceNumber)
                    {
                        return 3;
                    }
                }

                foreach (int specialSpace in this.doubleLetter)
                {
                    if (specialSpace == spaceNumber)
                    {
                        return 2;
                    }
                }

                return 1;
            }
        }

        /// <summary>
        /// Gets the factor to multiply a word's point value by.
        /// </summary>
        public int WordBonus
        {
            get
            {
                int spaceNumber = int.Parse(this.SpaceGrid.Name.Substring(this.SpaceGrid.Name.IndexOf("spaceGrid") + 9));
                foreach (int specialSpace in this.tripleWord)
                {
                    if (specialSpace == spaceNumber)
                    {
                        return 3;
                    }
                }

                foreach (int specialSpace in this.doubleWord)
                {
                    if (specialSpace == spaceNumber)
                    {
                        return 2;
                    }
                }

                return 1;
            }
        }
    }
}
