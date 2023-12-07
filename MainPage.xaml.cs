using Melody.ViewModels;
using Melody.Core;
using System;
using System.Collections.ObjectModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using System.Diagnostics;
using Melody.Statics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Melody
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            MainNavView.SelectedItem = BrowseItem;
            ContentFrame.Navigate(typeof(BrowsePage));

            Current = this;

            Settings.ThemeChanged += Settings_ThemeChanged;
        }
        public static MainPage Current;
        private void Settings_ThemeChanged(object sender, EventArgs e)
        {
            ApplicationViewTitleBar formattableTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            MainNavView.RequestedTheme = Settings.Theme;
            RequestedTheme = Settings.Theme;
            formattableTitleBar.ButtonForegroundColor = DefaultThemeBrush.Color;
        }
        public void Navigate(Type PageType, object args)
        {
            MainPage.Current.InAppNotif.Dismiss();
            ContentFrame.Navigate(PageType, args);
        }
        public void Navigate(Type PageType, object args, NavigationTransitionInfo a)
        {
            MainPage.Current.InAppNotif.Dismiss();
            ContentFrame.Navigate(PageType, args, a);
        }

        private void MainNavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItemContainer as NavigationViewItem;
            NavigationViewItem currentitem;

            //Get corresponding item
            switch (ContentFrame.SourcePageType.Name)
            {
                case "BrowsePage":
                    currentitem = BrowseItem;
                    break;
                case "DownloadsPage":
                    currentitem = DownloadsItem;
                    break;
                case "TopTrendingPage":
                    currentitem = TopTrendingItem;
                    break;
                case "SettingsPage":
                    currentitem = MainNavView.SettingsItem as NavigationViewItem;
                    break;
                default:
                    currentitem = null;
                    break;
            }
            if (args.IsSettingsInvoked)
            {
                item = MainNavView.SettingsItem as NavigationViewItem;
            }

            if (currentitem == item) {
                Debug.WriteLine($"{currentitem.Tag} - {item.Tag}: Current item is the same as item invoked");
                return; }

            switch (item.Tag)
            {
                case "BrowsePage":
                    ContentFrame.Navigate(typeof(BrowsePage));
                    break;
                case "DownloadsPage":
                    ContentFrame.Navigate(typeof(DownloadsPage));
                    break;
                case "TopTrendingPage":
                    ContentFrame.Navigate(typeof(TopTrendingPage));
                    break;
                default:
                    ContentFrame.Navigate(typeof(SettingsPage));
                    break;
            }
        }

        private void MainNavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            Debug.WriteLine("Back requested");
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
                switch (ContentFrame.SourcePageType.Name)
                {
                    case "BrowsePage":
                        MainNavView.SelectedItem = BrowseItem;
                        break;
                    case "DownloadsPage":
                        MainNavView.SelectedItem = DownloadsItem;
                        break;
                    case "TopTrendingPage":
                        MainNavView.SelectedItem = TopTrendingItem;
                        break;
                    case "SettingsPage":
                        MainNavView.SelectedItem = MainNavView.SettingsItem;
                        break;
                }
            }
        }

        private void ClosePlayerButton_Click(object sender, RoutedEventArgs e)
        {
            Player.Player.Hide();
        }

        private void PlayerDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var media = button.DataContext as IMedia;

            InfoHelper.ShowInAppNotification($"Successfully added \"{media.Name}\" to Downloads");
            DownloadManager.AddToDownloads(media);
        }
    }
}
