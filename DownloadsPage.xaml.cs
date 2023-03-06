using Melody.Classes;
using Melody.Statics;
using Melody.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Melody
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DownloadsPage : BasePage
    {
        public DownloadsPage()
        {
            this.InitializeComponent();

            Downloads = DownloadManager.Downloads;

            NavigationCacheMode = NavigationCacheMode.Required;

            DownloadsListView.ItemsSource = Downloads;

            Settings.ThemeChanged += Settings_ThemeChanged;
        }
        public override string Header => "Downloads";
        public override string MinimalHeader => "DOWNLOADS";
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
            var listview = VisualTreeHelper.GetChild(button.RecursiveGetParent(4), 1) as ListView;
            if (listview.Visibility == Visibility.Collapsed)
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
            if ((sender as Button).DataContext is DownloadItemViewModel item)
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
            }
            else if ((sender as Button).DataContext is DownloadCollectionItemViewModel collection)
            {
                var fonticon = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild((sender as Button), 0), 0) as FontIcon;

                if (fonticon.Glyph == Glyphs.CancelGlyph)
                {
                    collection.CancelDownload();
                }
                else if (fonticon.Glyph == Glyphs.RetryGlyph)
                {
                    collection.StartDownload();
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
            if (e.ClickedItem is DownloadItemViewModel downloaditem)
            {
                if(downloaditem.Status == "Completed.")
                {
                    await Launcher.LaunchFileAsync(downloaditem.OutputFile);
                }
            }
            else
            {
                await Launcher.LaunchFolderPathAsync(Settings.OutputFolder);
            }
        }
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as IDownloadItem;
            item.CancelDownload();
            Downloads.Remove(item);
            InfoHelper.ShowInAppNotification($"Removed \"{item.Authors.First()} - {item.Title}\" from Downloads");
        }
        private async void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as DownloadItemViewModel;
            if (await item.OutputFile.TryDeleteAsync())
            {
                Downloads.Remove(item);
                InfoHelper.ShowInAppNotification($"Deleted \"{item.Authors.First()} - {item.Title}\"");
            }
            else
            {
                InfoHelper.ShowInAppNotification($"Could not delete file!");
            }
        }
    }
}
