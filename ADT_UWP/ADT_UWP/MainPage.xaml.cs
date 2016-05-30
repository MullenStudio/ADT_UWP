//-----------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="Mullen Studio">
//     Copyright (c) Mullen Studio. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MullenStudio.ADT_UWP
{
    using System;
    using System.Threading.Tasks;
    using MullenStudio.ADT_UWP.Models;
    using Windows.Storage;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// The main page.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = new AdtStatus();
        }

        /// <summary>
        /// Gets the ADT status.
        /// </summary>
        public AdtStatus AdtStatus
        {
            get { return DataContext as AdtStatus; }
        }

        /// <summary>
        /// Invoked when the page is loaded.
        /// </summary>
        /// <param name="sender">The page which is loaded.</param>
        /// <param name="e">Details about the event.</param>
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = ApplicationData.Current.LocalSettings;
            await this.AdtStatus.Initialize(settings.Values["UserName"] as string, settings.Values["Password"] as string);
            Bindings.Update();
        }

        /// <summary>
        /// Invoked when the sign out button is clicked.
        /// </summary>
        /// <param name="sender">The sign out button which is clicked.</param>
        /// <param name="e">Details about the event.</param>
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values.Remove("UserName");
            settings.Values.Remove("Password");
            await AdtApi.Current.SignOut();
            App.Current.Exit();
        }

        /// <summary>
        /// Invoked when the refresh button is clicked.
        /// </summary>
        /// <param name="sender">The refresh button which is clicked.</param>
        /// <param name="e">Details about the event.</param>
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await this.AdtStatus.Refresh();
            Bindings.Update();
        }

        /// <summary>
        /// Invoked when the set arm button is clicked.
        /// </summary>
        /// <param name="sender">The set arm button which is clicked.</param>
        /// <param name="e">Details about the event.</param>
        private async void ArmButton_Click(object sender, RoutedEventArgs e)
        {
            await AdtApi.Current.SetArm((sender as Button).DataContext as string);
            await Task.Delay(5000);
            await this.AdtStatus.Refresh();
            Bindings.Update();
        }

        /// <summary>
        /// Invoked when set mode button is clicked.
        /// </summary>
        /// <param name="sender">The set mode button which is clicked.</param>
        /// <param name="e">Details about the event.</param>
        private async void ModeButton_Click(object sender, RoutedEventArgs e)
        {
            await AdtApi.Current.SetMode((int)(sender as Button).DataContext);
            await Task.Delay(5000);
            await this.AdtStatus.Refresh();
            Bindings.Update();
        }
    }
}
