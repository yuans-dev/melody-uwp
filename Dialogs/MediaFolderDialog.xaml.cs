using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class MediaFolderDialog : ContentDialog
    {
        public MediaFolderDialog()
        {
            this.InitializeComponent();

            FolderPaths = new BindingList<FolderViewModel>();
            FolderPaths.Add(new FolderViewModel { FolderPath = "dick" });
            FolderPaths.Add(new FolderViewModel { FolderPath = "adwd" });
            FolderPathsListView.ItemsSource = FolderPaths;
        }
        public event EventHandler<MediaFoldersChangedArgs> MediaFoldersChanged;
        private BindingList<FolderViewModel> FolderPaths { get; set; } 
        private List<string> ModelListToStringList(List<FolderViewModel> list)
        {
            List<string> temp = new List<string>();
            foreach (var item in list)
            {
                temp.Add(item.FolderPath);
            }
            return temp;
        }
        private void Close_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            OnMediaFoldersChanged();
        }
        protected virtual void OnMediaFoldersChanged()
        {
            MediaFoldersChanged?.Invoke(this, new MediaFoldersChangedArgs() { FolderPaths = ModelListToStringList(FolderPaths.ToList()) });
        }
        class FolderViewModel
        {
            public string FolderPath { get; set; } = string.Empty;
        }
    }
    public class MediaFoldersChangedArgs : EventArgs
    {
        public List<string> FolderPaths { get; set; }
    }
}
