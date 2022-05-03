using Media_Downloader_App.ViewModels;
using Media_Downloader_App.Core;
using System;
using System.Collections.ObjectModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Media_Downloader_App
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

            Downloads = new ObservableCollection<IDownloadItem>();

            Settings.ThemeChanged += Settings_ThemeChanged;
        }
        public static MainPage Current;
        public ObservableCollection<IDownloadItem> Downloads { get; private set; }

        private void Settings_ThemeChanged(object sender, EventArgs e)
        {
            ApplicationViewTitleBar formattableTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            MainNavView.RequestedTheme = Settings.Theme;
            RequestedTheme = Settings.Theme;
            formattableTitleBar.ButtonForegroundColor = DefaultThemeBrush.Color;
        }

        public async void AddToDownloads(IMedia Media)
        {
            var item = new DownloadItemViewModel(Media);
            Downloads.Add(item);
            await item.StartDownload();
        }
        public void AddToDownloads(SpotifyPlaylist Playlist, bool IsBackground)
        {
            var item = new DownloadCollectionItemViewModel(Playlist);
            Downloads.Add(item);
            item.StartDownload(IsBackground);
        }
        public void AddToDownloads(SpotifyAlbum Album, bool IsBackground)
        {
            var item = new DownloadCollectionItemViewModel(Album);
            Downloads.Add(item);
            item.StartDownload(IsBackground);
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
                default:
                    currentitem = MainNavView.SettingsItem as NavigationViewItem;
                    break;
            }

            if (currentitem == item) { return; }
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                switch (item.Tag)
                {
                    case "BrowsePage":
                        ContentFrame.Navigate(typeof(BrowsePage));
                        break;
                    case "DownloadsPage":
                        ContentFrame.Navigate(typeof(DownloadsPage));
                        break;
                }
            }
        }

        private void MainNavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
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
                    case "SettingsPage":
                        MainNavView.SelectedItem = MainNavView.SettingsItem;
                        break;
                }
            }
        }
    }
}
