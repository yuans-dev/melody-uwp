using Melody.Core;
using Melody.Statics;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Melody.Dialogs
{
    public sealed partial class EditTagsDialog : ContentDialog
    {
        public EditTagsDialog(IMedia Media)
        {
            this.InitializeComponent();

            this.Media = Media;

            TitleTextBox.Text = this.Media.Title;
            ArtistTextBox.Text = this.Media.Authors.ToString(", ");
        }
        private IMedia Media { get; set; }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Media.Title = TitleTextBox.Text;
            Media.Authors = ArtistTextBox.Text.ToArray(", ");
            Media.Album = AlbumTextBox.Text;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
