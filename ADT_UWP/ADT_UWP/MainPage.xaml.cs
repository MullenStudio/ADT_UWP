//-----------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="Mullen Studio">
//     Copyright (c) Mullen Studio. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MullenStudio.ADT_UWP
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using MullenStudio.ADT_UWP.Models;
    using Windows.System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media;

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
            var passwordVault = (App.Current as App).Password;
            var p = passwordVault.Retrieve();
            await this.AdtStatus.Initialize(p.UserName, p.Password);
            Bindings.Update();
        }

        /// <summary>
        /// Invoked when the sign out button is clicked.
        /// </summary>
        /// <param name="sender">The sign out button which is clicked.</param>
        /// <param name="e">Details about the event.</param>
        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var passwordVault = (App.Current as App).Password;
            passwordVault.ClearAllPasswords();
            await AdtApi.Current.SignOut();
            this.Frame.Navigate(typeof(SignInPage));
        }

        /// <summary>
        /// Invoked when the web button is clicked.
        /// </summary>
        /// <param name="sender">The web button which is clicked.</param>
        /// <param name="e">Details about the event.</param>
        private async void WebButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://portal-2.adtpulse.com/"));
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
        /// Invoked when the set mode button is clicked.
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

        /// <summary>
        /// Invoked when the grid view size is changed. Make sure the grid view items can fill the width of the grid view and maintain minimal size.
        /// </summary>
        /// <param name="sender">The grid view whose size is changed.</param>
        /// <param name="e">Details about the event.</param>
        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var itemsPanel = (sender as GridView).ItemsPanelRoot as ItemsWrapGrid;
            double totalWidth = itemsPanel.ActualWidth;
            int columns = Math.Max(1, Math.Min(4, (int)(totalWidth / 320)));
            itemsPanel.ItemWidth = totalWidth / columns;
            itemsPanel.ItemHeight = 600;
        }

        /// <summary>
        /// Invoked when the list view's container content is changing. Make sure the list view items can fill the height of the list view.
        /// </summary>
        /// <param name="sender">The list view whose container content is changing.</param>
        /// <param name="args">Details about the event.</param>
        private void ListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var listView = sender as ListView;
            double totalHeight = listView.ActualHeight;
            int rows = listView.Items.Count;
            if (rows == 0)
            {
                return;
            }

            double height = totalHeight / rows;
            var itemsStackPanel = this.FindItemsStackPanel(sender as DependencyObject);
            if (itemsStackPanel != null)
            {
                foreach (var child in itemsStackPanel.Children)
                {
                    (child as ListViewItem).Height = height;
                }
            }
        }

        /// <summary>
        /// Helper function to find the ItemsStackPanel of the list view recursively in the visual tree.
        /// </summary>
        /// <param name="parent">The parent dependency object.</param>
        /// <returns>The ItemsStackPanel if found, or null if not found.</returns>
        private ItemsStackPanel FindItemsStackPanel(DependencyObject parent)
        {
            return Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(parent))
                .Select(i =>
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child is ItemsStackPanel)
                    {
                        return child as ItemsStackPanel;
                    }

                    return this.FindItemsStackPanel(child);
                })
                .FirstOrDefault(dependencyObject => dependencyObject != null);
        }
    }
}
