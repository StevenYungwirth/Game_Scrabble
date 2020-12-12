//-----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="ColeSeanStevenCompany">
//     Company copyright tag.
// </copyright>
//-----------------------------------------------------------------------

namespace Game_Scrabble
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Encapsulation not yet taught.")]
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Close the form.
        /// </summary>
        /// <param name="sender">The object that initiated the event.</param>
        /// <param name="e">The event arguments for the event.</param>
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            // closes the window
            this.Close();
        }

        /// <summary>
        /// Displays information about the application.
        /// </summary>
        /// <param name="sender">The object that initiated the event.</param>
        /// <param name="e">The event arguments for the event.</param>
        private void BtnCredits_Click(object sender, RoutedEventArgs e)
        {
            // Display information about the application
            MessageBox.Show(" Game_Scrabble \n \n Created by: Cole Frisch, Sean Beyer and Steven Yungwirth \n \n 2020");
        }

        /// <summary>
        /// Start game window.
        /// </summary>
        /// <param name="sender">The object that initiated the event.</param>
        /// <param name="e">The event arguments for the event.</param>
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PlayerCount = (int)playerCounter.Value;
            Window boardWindow = new BoardWindow();
            this.Hide();
            boardWindow.ShowDialog();
            this.Show();
        }
    }
}
