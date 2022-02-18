using MP3DL.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Media_Downloader_App.Dialogs
{
    public partial class AuthSpotifyDialog : ContentDialog
    {
        public AuthSpotifyDialog()
        {
            this.InitializeComponent();
        }
        public event EventHandler<AuthorizedEventArgs> AuthorizationAttempted;
        private Spotify SpotifyClient { get; set; }
        private string ResultMessage { get; set; }
        private void Cancel_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void Authorize_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            SpotifyClient = new Spotify
            {
                Details = new ClientDetails { ID = ClientIDTextbox.Password, Secret = ClientSecretTextbox.Password}
            };

            AuthProgressBar.Visibility = Visibility.Visible;
            try
            {
                await SpotifyClient.Auth();
                ResultMessage = "Success";
            }
            catch(Exception ex)
            {
                ResultMessage = $"Error \"{ex.Message}\"";
            }
            OnAuthorized();
            AuthProgressBar.Visibility = Visibility.Collapsed;
        }
        protected virtual void OnAuthorized()
        {
            AuthorizationAttempted?.Invoke(this,
                new AuthorizedEventArgs()
                {
                    SpotifyClient = SpotifyClient,
                    ResultMessage = ResultMessage
                });
        }
    }
    public class AuthorizedEventArgs : EventArgs
    {
        public Spotify SpotifyClient { get; set; }
        public string ResultMessage { get; set; }
    }
}
