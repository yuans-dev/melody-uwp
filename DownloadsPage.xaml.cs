using Media_Downloader_App.Statics;
using Media_Downloader_App.ViewModels;
using MP3DL.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Media_Downloader_App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DownloadsPage : Page
    {
        public DownloadsPage()
        {
            this.InitializeComponent();

            Current = this;

            Downloads = MainPage.Current.Downloads;

            NavigationCacheMode = NavigationCacheMode.Required;

            DownloadsListView.ItemsSource = Downloads;

            Settings.ThemeChanged += Settings_ThemeChanged;
        }
        public static DownloadsPage Current;
        public ObservableCollection<IDownloadItem> Downloads { get; private set; }
        private void Settings_ThemeChanged(object sender, EventArgs e)
        {
            RequestedTheme = Settings.Theme;
        }

        private void DownloadsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DownloadsListView.SelectedItem = null;
        }

        private void ShowCollapseListButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var listview = VisualTreeHelper.GetChild(DependencyObjectHelper.RecursiveGetParent(button, 4), 1) as ListView;
            if(listview.Visibility == Visibility.Collapsed)
            {
                listview.Visibility = Visibility.Visible;
            }
            else
            {
                listview.Visibility = Visibility.Collapsed;
            }
        }

        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if((sender as Button).DataContext is DownloadItemViewModel item)
            {
                var fonticon = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild((sender as Button), 0), 0) as FontIcon;

                if (fonticon.Glyph == Glyphs.CancelGlyph)
                {
                    item.CancelDownload();
                }
                else if (fonticon.Glyph == Glyphs.RetryGlyph)
                {
                    await item.StartDownload();
                }
                else
                {

                }
            }else if((sender as Button).DataContext is DownloadCollectionItemViewModel collection)
            {
                var fonticon = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild((sender as Button), 0), 0) as FontIcon;

                if (fonticon.Glyph == Glyphs.CancelGlyph)
                {
                    collection.CancelDownload();
                }
                else if (fonticon.Glyph == Glyphs.RetryGlyph)
                {
                    collection.StartDownload(collection.IsBackground);
                }
                else
                {

                }
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listview = sender as ListView;
            listview.SelectedItem = null;
        }
        private async void DownloadsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            await Launcher.LaunchFolderPathAsync(Settings.OutputFolder);
        }
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as IDownloadItem;
            item.CancelDownload();
            Downloads.Remove(item);
            InfoHelper.ShowInAppNotification($"Removed \"{item.Author} - {item.Title}\" from Downloads");
        }
        private async void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as DownloadItemViewModel;
            if(item.OutputFile != null || File.Exists(item.OutputFile.Path))
            {
                await item.OutputFile.DeleteAsync();
                Downloads.Remove(item);
                InfoHelper.ShowInAppNotification($"Deleted \"{item.Author} - {item.Title}\"");
            }
            else
            {
                InfoHelper.ShowInAppNotification($"Download has not finished!");
            }
        }
    }
}
