using Media_Downloader_App.Core;
using Windows.UI.Xaml.Controls;

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
