//-----------------------------------------------------------------------
// <copyright file="BoardWindow.xaml.cs" company="ColeSeanStevenCompany">
//     Company copyright tag.
// </copyright>
//-----------------------------------------------------------------------

namespace Game_Scrabble
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Xml;

    /// <summary>
    /// Interaction logic for BoardWindow
    /// </summary>
    public partial class BoardWindow : Window
    {
        /// <summary>
        /// Number of spaces on the board.
        /// </summary>
        private const int SPACECOUNT = 225;

        /// <summary>
        /// Creates a tileBag.
        /// </summary>
        private TileBag tileBag;

        /// <summary>
        /// The list of players.
        /// </summary>
        private List<Player> players;

        /// <summary>
        /// Creates a Player.
        /// </summary>
        private Player currentPlayer;

        /// <summary>
        /// The spaces.
        /// </summary>
        private Space[] spaces;

        /// <summary>
        /// The list of available words.
        /// </summary>
        private List<string> dictionary;

        /// <summary>
        /// The list of tiles that have been placed.
        /// </summary>
        private List<Space> placedTileSpaces;

        /// <summary>
        /// Keeps track of the mouse start point.
        /// </summary>
        private Point mouseStart;

        /// <summary>
        /// The mouse point offset
        /// </summary>
        private Vector mouseStartOffset;

        /// <summary>
        /// The selected tile
        /// </summary>
        private Tile selectedTile;

        /// <summary>
        /// The selected tile grid
        /// </summary>
        private Grid selectedTileGrid;

        /// <summary>
        /// Is it the first turn.
        /// </summary>
        private bool isFirstTurn;

        /// <summary>
        /// The number of skips
        /// </summary>
        private int skipCount;

        /// <summary>
        /// The space's letter bonus.
        /// </summary>
        private List<int> letterBonus = new List<int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BoardWindow"/> class.
        /// </summary>
        public BoardWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Calls initialize
        /// </summary>
        /// <param name="sender">The object that initiated the event.</param>
        /// <param name="e">The event arguments for the event.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
        }

        /// <summary>
        /// Initializes the game components
        /// </summary>
        private void Initialize()
        {
            // Initialize our variables
            this.tileBag = new TileBag();
            this.placedTileSpaces = new List<Space>();

            // Initialize our dictionary
            this.dictionary = new List<string>();
            this.LoadDictionary();

            // Create an instance of the Spaces class
            this.spaces = new Space[SPACECOUNT];
            for (int i = 0; i < SPACECOUNT; i++)
            {
                // Set the Spaces
                Space tempSpace = new Space();
                tempSpace.SpaceGrid = (Grid)this.FindName("spaceGrid" + i);
                tempSpace.XYPosition = tempSpace.SpaceGrid.TransformToAncestor(gameGrid).Transform(new Point(0, 0));
                this.spaces[i] = tempSpace;
            }

            // Initialize the players
            this.players = new List<Player>();

            for (int playerCount = 1; playerCount <= Properties.Settings.Default.PlayerCount; playerCount++)
            {
                Player player = new Player(playerCount);

                // Populate our player's drawn tiles
                player.DrawTilesToMax(this.tileBag);

                // Add the player to the player list
                this.players.Add(player);

                Label scoreLabel = (Label)this.windowGrid.FindName("player" + player.Number + "Score");
                scoreLabel.Visibility = Visibility.Visible;
            }

            // Set the current player
            this.currentPlayer = this.players[0];
            this.ToggleTiles();
            this.isFirstTurn = true;
        }

        /// <summary>
        /// Enable or disable the players' pieces
        /// </summary>
        private void ToggleTiles()
        {
            foreach (Player player in this.players)
            {
                Grid piecesGrid = (Grid)this.FindName("player" + player.Number + "Pieces");
                piecesGrid.IsEnabled = false;
            }

            Grid currentPiecesGrid = (Grid)this.FindName("player" + this.currentPlayer.Number + "Pieces");
            currentPiecesGrid.IsEnabled = true;
        }

        /// <summary>
        /// Loads the dictionary
        /// </summary>
        private void LoadDictionary()
        {
            string dictPath = "dictionary.txt";
            StreamReader wordFile = File.OpenText(dictPath);

            while (!wordFile.EndOfStream)
            {
                this.dictionary.Add(wordFile.ReadLine());
            }

            wordFile.Close();
        }

        /// <summary>
        /// Fires when the mouse clicks on a tile
        /// </summary>
        /// <param name="sender">The object that initiated the event.</param>
        /// <param name="e">The event arguments for the event.</param>
        private void ClickOnTile(object sender, MouseButtonEventArgs e)
        {
            if (this.selectedTileGrid == null)
            {
                // Check if the tile is in the player's pieces
                Grid playerPieces = (Grid)this.FindName("player" + this.currentPlayer.Number + "Pieces");

                this.selectedTileGrid = sender as Grid;

                if (this.selectedTileGrid.Parent.Equals(playerPieces))
                {
                    // Find the tile that was selected
                    int space = 0;
                    while (this.selectedTile == null && space < this.currentPlayer.TileBar.Count)
                    {
                        if (this.currentPlayer.TileBar[space].Location == this.selectedTileGrid)
                        {
                            this.selectedTile = this.currentPlayer.TileBar[space];
                        }

                        space++;
                    }

                    // Clone the grabbed tile
                    foreach (UIElement child in this.selectedTileGrid.Children)
                    {
                        UIElement copy = this.CloneObject(child);
                        pickedUpTile.Children.Add(copy);
                    }

                    pickedUpTile.Visibility = Visibility.Visible;

                    this.selectedTileGrid.Visibility = Visibility.Hidden;

                    // First click: Pick up the tile
                    this.mouseStart = e.GetPosition(this.windowGrid);
                    double initialXOffset = this.pickedUpTile.Width / 2;
                    double initialYOffset = this.pickedUpTile.Height / 2;
                    Point pickedUpStart = this.pickedUpTile.TransformToVisual(this).Transform(new Point(initialXOffset, initialYOffset));
                    this.mouseStartOffset = new Vector();
                    this.mouseStartOffset = Point.Subtract(this.mouseStart, pickedUpStart);
                    this.pickedUpTile.CaptureMouse();
                }
            }
            else
            {
                // Second click: Put down the tile
                // Find the space the tile is over
                Point relativePoint = this.pickedUpTile.TransformToVisual(gameGrid).Transform(new Point(0, 0));
                double xPos = relativePoint.X;
                double yPos = relativePoint.Y;
                int spaceNum = 0;
                Space destination = null;

                while (destination == null && spaceNum < SPACECOUNT)
                {
                    if (xPos > this.spaces[spaceNum].XYPosition.X - 10 && xPos < this.spaces[spaceNum].XYPosition.X + 10)
                    {
                        if (yPos > this.spaces[spaceNum].XYPosition.Y - 10 && yPos < this.spaces[spaceNum].XYPosition.Y + 10)
                        {
                            destination = this.spaces[spaceNum];
                        }
                    }

                    if (destination == null)
                    {
                        spaceNum++;
                    }
                }

                if (destination != null)
                {
                    if (destination.Tile == null)
                    {
                        this.pickedUpTile.Visibility = Visibility.Hidden;

                        // Place the tile
                        foreach (UIElement child in this.selectedTileGrid.Children)
                        {
                            UIElement copy = this.CloneObject(child);
                            destination.SpaceGrid.Children.Add(copy);
                        }

                        // Add the tile to the space
                        this.spaces[spaceNum].Tile = this.selectedTile;
                        this.spaces[spaceNum].Tile.Location = this.spaces[spaceNum].SpaceGrid;

                        // Add the space to our tracked list of spaces
                        this.placedTileSpaces.Add(this.spaces[spaceNum]);

                        // Remove the tile from the player's pieces
                        this.currentPlayer.TileBar.Remove(this.selectedTile);

                        // Reset back to normal
                        this.pickedUpTile.ReleaseMouseCapture();
                        this.selectedTileGrid = null;
                        this.selectedTile = null;

                        // Put the tile back to where it started
                        TranslateTransform translate = pickedUpTile.RenderTransform as TranslateTransform;
                        translate.X = 0;
                        translate.Y = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Moves the tile when the mouse moves
        /// </summary>
        /// <param name="sender">The object that initiated the event.</param>
        /// <param name="e">The event arguments for the event.</param>
        private void MoveTile(object sender, MouseEventArgs e)
        {
            Grid element = sender as Grid;
            TranslateTransform translate = element.RenderTransform as TranslateTransform;

            if (element.IsMouseCaptured)
            {
                Vector offset = Point.Subtract(e.GetPosition(windowGrid), this.mouseStart);

                translate.X = this.mouseStartOffset.X + offset.X;
                translate.Y = this.mouseStartOffset.Y + offset.Y;
            }
        }

        /// <summary>
        /// Clones the object of a source
        /// </summary>
        /// <param name="source">The object that needs to be cloned.</param>
        /// <typeparam name="T">The generic type parameter.</typeparam>
        /// <returns> Returns T</returns>
        private T CloneObject<T>(T source)
        {
            string xaml = XamlWriter.Save(source);
            StringReader sr = new StringReader(xaml);
            XmlReader xr = XmlReader.Create(sr);
            return (T)XamlReader.Load(xr);
        }

        /// <summary>
        /// Handles the submit word button
        /// </summary>
        /// <param name="sender">The object that initiated the event.</param>
        /// <param name="e">The event arguments for the event.</param>
        private void BtnSubmitWordClick(object sender, RoutedEventArgs e)
        {
            int horizontalCount = 0;
            int verticalCount = 0;

            if (this.placedTileSpaces.Count > 0)
            {
                // Check the orientation of the word
                double earliestXPoint = this.placedTileSpaces.OrderBy(o => o.XYPosition.X).First().XYPosition.X;
                double earliestYPoint = this.placedTileSpaces.OrderBy(o => o.XYPosition.Y).First().XYPosition.Y;

                foreach (Space space in this.placedTileSpaces)
                {
                    // Check the vertical
                    // Check if they are the same
                    if (earliestXPoint == space.XYPosition.X)
                    {
                        // We have a vertically orientated word
                        verticalCount++;
                    }

                    // Check the horizontal
                    // Check if they are the same
                    if (earliestYPoint == space.XYPosition.Y)
                    {
                        horizontalCount++;
                    }
                }

                bool isHorizontal = horizontalCount == this.placedTileSpaces.Count;
                bool isVertical = verticalCount == this.placedTileSpaces.Count;

                if (!(isVertical || isHorizontal))
                {
                    MessageBox.Show("Letters need to be placed in a line.");
                    this.ReturnTiles();
                }
                else
                {
                    List<Space> sortedSpaces = new List<Space>();

                    // Get the word that was spelled
                    if (isHorizontal)
                    {
                        // Sort list of tiles horizontally
                        sortedSpaces = this.placedTileSpaces.OrderBy(o => o.XYPosition.X).ToList();
                    }
                    else
                    {
                        // Sort list of tiles horizontally
                        sortedSpaces = this.placedTileSpaces.OrderBy(o => o.XYPosition.Y).ToList();
                    }

                    string word = string.Empty;
                    bool isCenterSpace = false;
                    int wordBonus = 1;
                    bool extraTilesFound = false;

                    if (this.isFirstTurn)
                    {
                        // Make sure one of the tiles is on the center space
                        foreach (Space space in sortedSpaces)
                        {
                            isCenterSpace = space.SpaceGrid.Name == "spaceGrid112";

                            if (isCenterSpace)
                            {
                                break;
                            }
                        }

                        if (!isCenterSpace)
                        {
                            MessageBox.Show("First word must include the center space.");
                            this.ReturnTiles();
                            return;
                        }

                        // There are no tiles on the board for the word to be adjacent to, so all the tiles should be next to each other
                        if (this.AreAllTilesAdjacent(sortedSpaces, isHorizontal))
                        {
                            foreach (Space space in sortedSpaces)
                            {
                                word += space.Tile.Letter;
                                this.letterBonus.Add(space.LetterBonus);
                                wordBonus *= space.WordBonus;
                            }
                        }
                        else
                        {
                            MessageBox.Show("All tiles need to be next to each other.");
                            this.ReturnTiles();
                            return;
                        }
                    }
                    else
                    {
                        // Get all tiles before the word
                        Tile extraTiles = this.GetPreviousAdjacent(sortedSpaces[0], isHorizontal);
                        while (extraTiles != null)
                        {
                            word = word.Insert(0, extraTiles.Letter);
                            this.letterBonus.Add(1);
                            int spaceNumber = int.Parse(extraTiles.Location.Name.Substring(extraTiles.Location.Name.IndexOf("spaceGrid") + 9));
                            extraTiles = this.GetPreviousAdjacent(this.spaces[spaceNumber], isHorizontal);
                            extraTilesFound = true;
                        }

                        // Get the tiles in the middle of the word
                        for (int space = 0; space < sortedSpaces.Count; space++)
                        {
                            // Add the letter to the word
                            word += sortedSpaces[space].Tile.Letter;
                            this.letterBonus.Add(sortedSpaces[space].LetterBonus);
                            wordBonus *= sortedSpaces[space].WordBonus;

                            if (space != sortedSpaces.Count - 1 &&
                                this.GetNextAdjacent(sortedSpaces[space], isHorizontal) != sortedSpaces[space + 1].Tile)
                            {
                                // The next tile was placed already, add it and any other already placed tiles to the word
                                extraTiles = this.GetNextAdjacent(sortedSpaces[space], isHorizontal);
                                while (extraTiles != null && extraTiles != sortedSpaces[space + 1].Tile)
                                {
                                    word += extraTiles.Letter;
                                    this.letterBonus.Add(1);
                                    int spaceNumber = int.Parse(extraTiles.Location.Name.Substring(extraTiles.Location.Name.IndexOf("spaceGrid") + 9));
                                    extraTiles = this.GetNextAdjacent(this.spaces[spaceNumber], isHorizontal);
                                }

                                extraTilesFound = true;
                            }
                        }

                        // Get the tiles after the word
                        extraTiles = this.GetNextAdjacent(sortedSpaces[sortedSpaces.Count - 1], isHorizontal);
                        while (extraTiles != null)
                        {
                            word += extraTiles.Letter;
                            this.letterBonus.Add(1);
                            int spaceNumber = int.Parse(extraTiles.Location.Name.Substring(extraTiles.Location.Name.IndexOf("spaceGrid") + 9));
                            extraTiles = this.GetNextAdjacent(this.spaces[spaceNumber], isHorizontal);
                            extraTilesFound = true;
                        }
                    }

                    // Use to determine if word is valid
                    bool isWordValid;
                    if (!this.isFirstTurn && word.Length == 1)
                    {
                        isWordValid = true;
                        this.letterBonus.Clear();
                    }
                    else if (this.isFirstTurn && word.Length == 1)
                    {
                        MessageBox.Show("Must play more than 1 tile on the first turn");
                        this.ReturnTiles();
                        return;
                    }
                    else
                    {
                        isWordValid = this.dictionary.Contains(word);
                    }

                    // check if the word was valid
                    if (isWordValid)
                    {
                        // Check for words in the middle
                        List<string> trackedWords = new List<string>();
                        foreach (Space space in sortedSpaces)
                        {
                            string nextWord = this.CheckNextOpposite(space, isHorizontal);
                            string previousWord = this.CheckPreviousOpposite(space, isHorizontal);

                            if (nextWord != string.Empty && previousWord != string.Empty)
                            {
                                nextWord = previousWord + space.Tile.Letter + nextWord;
                                this.letterBonus.Add(space.LetterBonus);
                                trackedWords.Add(nextWord);
                                extraTilesFound = true;
                            }
                            else
                            {
                                if (nextWord != string.Empty)
                                {
                                    nextWord = nextWord.Insert(0, space.Tile.Letter);
                                    this.letterBonus.Add(space.LetterBonus);
                                    trackedWords.Add(nextWord);
                                    extraTilesFound = true;
                                }

                                if (previousWord != string.Empty)
                                {
                                    previousWord += space.Tile.Letter;
                                    this.letterBonus.Add(space.LetterBonus);
                                    trackedWords.Add(previousWord);
                                    extraTilesFound = true;
                                }
                            }
                        }
                        
                        foreach (string trackedWord in trackedWords)
                        {
                            if (!this.dictionary.Contains(trackedWord))
                            {
                                // At least one word wasn't valid. Return all the placed tiles
                                MessageBox.Show(trackedWord + " was not in the dictionary.");
                                this.ReturnTiles();
                                return;
                            }
                        }

                        if (!this.isFirstTurn && !extraTilesFound)
                        {
                            // No extra tiles were found, so the word wasn't placed next to existing tiles
                            MessageBox.Show("New words must be played off of tiles already on the board.");
                            this.ReturnTiles();
                            return;
                        }

                        foreach (string trackedWord in trackedWords)
                        {
                            this.currentPlayer.AddToScore(trackedWord, this.letterBonus, wordBonus);
                            MessageBox.Show("The word spelled was: " + trackedWord.ToLower() + ". Your score is now " + this.currentPlayer.Score);
                        }

                        // Add score to the word that was spelled
                        if (word.Length != 1)
                        {
                            this.currentPlayer.AddToScore(word, this.letterBonus, wordBonus);
                            if (sortedSpaces.Count == 7)
                            {
                                this.currentPlayer.Score += 50;
                            }

                            MessageBox.Show("The word spelled was: " + word.ToLower() + ". Your score is now " + this.currentPlayer.Score);
                        }

                        // Draw tiles until full and clear our tracked tiles that were placed
                        Label scoreLabel =
                            (Label)this.windowGrid.FindName("player" + this.currentPlayer.Number + "Score");
                        scoreLabel.Content = "Player " + this.currentPlayer.Number + ": " +
                                             this.currentPlayer.Score;
                        this.currentPlayer.DrawTilesToMax(this.tileBag);
                        this.CheckForWin();
                        this.placedTileSpaces.Clear();
                        this.letterBonus.Clear();
                        this.currentPlayer = this.currentPlayer.Number == this.players.Count ? this.players[0] : this.players[this.currentPlayer.Number];
                        this.ToggleTiles();
                        this.isFirstTurn = false;
                        this.skipCount = 0;
                    }
                    else
                    {
                        MessageBox.Show("The word spelled was not in the dictionary.");

                        // Return the placed tiles and clear out our tracked tiles
                        this.ReturnTiles();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please place at least one tile down");
            }
        }

        /// <summary>
        /// Check if the game is over.
        /// </summary>
        private void CheckForWin()
        {
            if (this.currentPlayer.TileBar.Count == 0 || this.skipCount == 4)
            {
                // Display the winner
                List<Player> playerList = this.players.OrderByDescending(x => x.Score).ToList();
                string message = "Player " + playerList[0].Number + " wins! \n\n";
                foreach (Player player in playerList)
                {
                    message += "Player " + player.Number + ": " + player.Score + "\n";
                }

                MessageBox.Show(message);
                this.Close();
            }
        }

        /// <summary>
        /// Get the next word that is the opposite orientation of the placed word.
        /// </summary>
        /// <param name="space">The space to look for more letters from</param>
        /// <param name="isHorizontal">The orientation to look</param>
        /// <returns>Returns the vertical word if the placed word is horizontal and vice versa</returns>
        private string CheckNextOpposite(Space space, bool isHorizontal)
        {
            Tile nextTile = this.GetNextAdjacent(space, !isHorizontal);
            string word = string.Empty;

            if (nextTile != null)
            {
                while (nextTile != null)
                {
                    word += nextTile.Letter;
                    this.letterBonus.Add(1);
                    int spaceNumber = int.Parse(nextTile.Location.Name.Substring(nextTile.Location.Name.IndexOf("spaceGrid") + 9));
                    nextTile = this.GetNextAdjacent(this.spaces[spaceNumber], !isHorizontal);
                }
            }

            return word;
        }

        /// <summary>
        /// Get the previous word that is the opposite orientation of the placed word.
        /// </summary>
        /// <param name="space">The space to look for more letters from</param>
        /// <param name="isHorizontal">The orientation to look</param>
        /// <returns>Returns the vertical word if the placed word is horizontal and vice versa</returns>
        private string CheckPreviousOpposite(Space space, bool isHorizontal)
        {
            Tile previousTile = this.GetPreviousAdjacent(space, !isHorizontal);
            string word = string.Empty;

            if (previousTile != null)
            {
                while (previousTile != null)
                {
                    word = word.Insert(0, previousTile.Letter);
                    this.letterBonus.Add(1);
                    int spaceNumber = int.Parse(previousTile.Location.Name.Substring(previousTile.Location.Name.IndexOf("spaceGrid") + 9));
                    previousTile = this.GetPreviousAdjacent(this.spaces[spaceNumber], !isHorizontal);
                }
            }

            return word;
        }

        /// <summary>
        /// Returns if all of the placed tiles are next to each other.
        /// </summary>
        /// <param name="sortedSpaces">The space to look for the next tile from</param>
        /// <param name="isHorizontal">If the method is looking horizontally or vertically</param>
        /// <returns>Returns if all the placed tiles are next to each other</returns>
        private bool AreAllTilesAdjacent(List<Space> sortedSpaces, bool isHorizontal)
        {
            double position = 0;
            foreach (Space space in sortedSpaces)
            {
                if (isHorizontal)
                {
                    if (position == 0)
                    {
                        position = space.XYPosition.X;
                    }
                    else
                    {
                        if (position != space.XYPosition.X)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (position == 0)
                    {
                        position = space.XYPosition.Y;
                    }
                    else
                    {
                        if (position != space.XYPosition.Y)
                        {
                            return false;
                        }
                    }
                }

                position += 45;
            }

            return true;
        }

        /// <summary>
        /// Get the tile directly below or to the right of the given space.
        /// </summary>
        /// <param name="space">The space to look for the next tile from</param>
        /// <param name="isHorizontal">If the method is looking horizontally or vertically</param>
        /// <returns>Returns the tile after the given space</returns>
        private Tile GetNextAdjacent(Space space, bool isHorizontal)
        {
            int spaceNumber = int.Parse(space.SpaceGrid.Name.Substring(space.SpaceGrid.Name.IndexOf("spaceGrid") + 9));

            if (isHorizontal)
            {
                if ((spaceNumber - 14) % 15 == 0)
                {
                    // The space is in the last column; no tiles to the right of the space
                    return null;
                }
                else
                {
                    // Get the tile from the space to the right of this space
                    Space nextSpace = this.spaces[spaceNumber + 1];
                    return nextSpace.Tile;
                }
            }
            else
            {
                if (spaceNumber >= 210 && spaceNumber <= 224)
                {
                    // The space is in the last row; no tiles below the space
                    return null;
                }
                else
                {
                    // Get the tile from the space below this space
                    Space nextSpace = this.spaces[spaceNumber + 15];
                    return nextSpace.Tile;
                }
            }
        }

        /// <summary>
        /// Get the tile directly above or to the left of the given space.
        /// </summary>
        /// <param name="space">The space to look for the previous tile from</param>
        /// <param name="isHorizontal">If the method is looking horizontally or vertically</param>
        /// <returns>Returns the tile before the given space</returns>
        private Tile GetPreviousAdjacent(Space space, bool isHorizontal)
        {
            int spaceNumber = int.Parse(space.SpaceGrid.Name.Substring(space.SpaceGrid.Name.IndexOf("spaceGrid") + 9));

            if (isHorizontal)
            {
                if (spaceNumber % 15 == 0)
                {
                    // The space is in the first column; no tiles to the left of the space
                    return null;
                }
                else
                {
                    // Get the tile from the space to the left of this space
                    Space previousSpace = this.spaces[spaceNumber - 1];
                    return previousSpace.Tile;
                }
            }
            else
            {
                if (spaceNumber >= 0 && spaceNumber <= 14)
                {
                    // The space is in the first row; no tiles above the space
                    return null;
                }
                else
                {
                    // Get the tile from the space above this space
                    Space previousSpace = this.spaces[spaceNumber - 15];
                    return previousSpace.Tile;
                }
            }
        }

        /// <summary>
        /// Return the placed tiles and clear the list of placed tile spaces.
        /// </summary>
        private void ReturnTiles()
        {
            this.ReturnPlacedTiles();
            this.placedTileSpaces.Clear();
        }

        /// <summary>
        /// Returns the placed tiles
        /// </summary>
        private void ReturnPlacedTiles()
        {
            // Return the placed tiles
            foreach (Space space in this.placedTileSpaces)
            {
                // Add the tile back to the player tile bar
                this.currentPlayer.AddTileToBar(space.Tile);

                // Remove the tile at that location
                for (int i = space.SpaceGrid.Children.Count - 1; i >= 0; i--)
                {
                    // Don't remove the text block if it's a bonus space
                    UIElement child = space.SpaceGrid.Children[i];
                    if (child.GetType() != typeof(TextBlock))
                    {
                        space.SpaceGrid.Children.Remove(child);
                    }
                }

                space.Tile = null;
            }
        }

        /// <summary>
        /// Skip the current player's turn.
        /// </summary>
        /// <param name="sender">The object that initiated the event.</param>
        /// <param name="e">The event arguments for the event.</param>
        private void BtnSkipTurnClick(object sender, RoutedEventArgs e)
        {
            this.ReturnTiles();
            this.currentPlayer = this.currentPlayer.Number == this.players.Count ? this.players[0] : this.players[this.currentPlayer.Number];
            this.ToggleTiles();
            this.isFirstTurn = false;
            this.skipCount++;
            this.CheckForWin();
        }

        /// <summary>
        /// Quit the game.
        /// </summary>
        /// <param name="sender">The object that initiated the event.</param>
        /// <param name="e">The event arguments for the event.</param>
        private void BtnQuitGameClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Return any placed tiles to the player's hand.
        /// </summary>
        /// <param name="sender">The object that initiated the event.</param>
        /// <param name="e">The event arguments for the event.</param>
        private void BtnResetTilesClick(object sender, RoutedEventArgs e)
        {
            this.ReturnTiles();
        }
    }
}
