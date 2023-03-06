﻿using Melody.Classes;
using Melody.Dialogs;
using Melody.Statics;
using System;
using Windows.UI.Xaml;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Melody
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : BasePage
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            switch (Settings.Theme)
            {
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
        public override string Header => "Settings";
        public override string MinimalHeader => "SETTINGS";
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
        private void Dialog_AuthorizationAttempted(object sender, AuthorizedEventArgs e)
        {
            if (e.SpotifyClient.Authd)
            {
                Settings.SpotifyClient = e.SpotifyClient;
                Settings.Save();

                InfoHelper.ShowInAppNotification("Authorized!");
            }
            else
            {
                InfoHelper.ShowInAppNotification("Authorization failed! Please try again");
            }
        }
    }
}
