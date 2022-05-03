using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace Media_Downloader_App.Classes
{
    public class BasePage : Page, INotifyPropertyChanged
    {
        public virtual string Header => "";
        public virtual string MinimalHeader => "";
        private bool _IsLoading { get; set; } = false;
        public virtual bool IsLoading
        {
            get { return _IsLoading; }
            set 
            { 
                _IsLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }
        private string _LoadingText { get; set; } = "";
        public virtual string LoadingText
        {
            get { return _LoadingText; }
            set
            {
                _LoadingText = value;
                OnPropertyChanged("LoadingText");
            }
        }
        private string _Label { get; set; } = "";
        public virtual string Label
        {
            get { return _Label; }
            set
            {
                _LoadingText = value;
                OnPropertyChanged("Label");
            }
        }
        private double _LoadingProgress { get; set; } = 0;
        public virtual double LoadingProgress
        {
            get { return _LoadingProgress; }
            set
            {
                _LoadingProgress = value;
                OnPropertyChanged("LoadingProgress");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
