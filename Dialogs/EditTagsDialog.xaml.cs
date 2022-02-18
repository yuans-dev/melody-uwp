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
    public sealed partial class EditTagsDialog : ContentDialog
    {
        public EditTagsDialog(IMedia Media)
        {
            this.InitializeComponent();

            this.Media = Media;

            TitleTextBox.Text = this.Media.Title;
            ArtistTextBox.Text = this.Media.PrintedAuthors;
        }
        public IMedia Media { get; private set; }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Media.Title = TitleTextBox.Text;
            Media.PrintedAuthors = ArtistTextBox.Text;
            Media.Album = AlbumTextBox.Text;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
