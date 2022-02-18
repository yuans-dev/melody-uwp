using Media_Downloader_App.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            if(Settings.SpotifyClient.Authd)
            {
                AuthorizationControls.Visibility = Visibility.Collapsed;
            }

            switch (Settings.Theme){
                case ElementTheme.Light:
                    LightTheme.IsChecked = true;
                    break;
                case ElementTheme.Dark:
                    DarkTheme.IsChecked = true;
                    break;
                case ElementTheme.Default:
                    DefaultTheme.IsChecked = true;
                    break;
            }
            Settings.ThemeChanged += Settings_ThemeChanged;
            Settings.OutputChanged += Settings_OutputChanged;
        }
        public string Output { get; set; } = Settings.OutputFolder;
        private void Settings_ThemeChanged(object sender, EventArgs e)
        {
            RequestedTheme = Settings.Theme;
        }
        private void Settings_OutputChanged(object sender, EventArgs e)
        {
            OutputFolderTextBlock.Text = Settings.OutputFolder;
        }

        private void LightTheme_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Theme = ElementTheme.Light;
        }

        private void DarkTheme_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Theme = ElementTheme.Dark;
        }

        private void DefaultTheme_Checked(object sender, RoutedEventArgs e)
        {
            Settings.Theme = ElementTheme.Default;
        }

        private async void AuthorizeSpotify_Click(object sender, RoutedEventArgs e)
        {
            AuthSpotifyDialog dialog = new AuthSpotifyDialog();
            dialog.RequestedTheme = Settings.Theme;
            dialog.AuthorizationAttempted += Dialog_AuthorizationAttempted;
            await dialog.ShowAsync();
        }
        private async void DownloadsFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", folder);

                Settings.OutputFolder = folder.Path;
                Settings.Save();
            }
            else
            {

            }
        }
        private async void MediaFolderButton_Click(object sender, RoutedEventArgs e)
        {
            MediaFolderDialog dialog =
                new MediaFolderDialog 
                { 
                    RequestedTheme = Settings.Theme 
                };

            await dialog.ShowAsync();
        }
        private void Dialog_AuthorizationAttempted(object sender, AuthorizedEventArgs e)
        {
            if (e.SpotifyClient.Authd)
            {
                Settings.SpotifyClient = e.SpotifyClient;
                Settings.Save();
            }
        }
    }
}
