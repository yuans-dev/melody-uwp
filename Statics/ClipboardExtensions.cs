using Windows.ApplicationModel.DataTransfer;

namespace Media_Downloader_App.Statics
{
    public static class ClipboardExtensions
    {
        public static void CopyToClipboard(string Text)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(Text);
            Clipboard.SetContent(dataPackage);
        }
    }
}
