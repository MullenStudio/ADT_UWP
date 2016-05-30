//-----------------------------------------------------------------------
// <copyright file="SignInPage.xaml.cs" company="Mullen Studio">
//     Copyright (c) Mullen Studio. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MullenStudio.ADT_UWP
{
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// The sign in page.
    /// </summary>
    public sealed partial class SignInPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignInPage"/> class.
        /// </summary>
        public SignInPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the sign in button is clicked.
        /// </summary>
        /// <param name="sender">The sign in button which is clicked.</param>
        /// <param name="e">Details about the event.</param>
        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values["UserName"] = UserNameTextBox.Text;
            // TODO: password is saved without encryption, should have at least simple encryption.
            settings.Values["Password"] = PasswordTextBox.Password;
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
