using Melody.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Melody.ViewModels
{
    public class HistoryItemViewModel
    {
        public HistoryItemViewModel()
        {

        }
        public HistoryItemViewModel(string Title, string Description, DateTime Date)
        {
            this.Title = Title;
            this.Description = Description;
            this.Date = Date;
        }
        public HistoryItemViewModel(string Title, string Description, DateTime Date, IBaseMedia DataContext)
        {
            this.Title = Title;
            this.Description = Description;
            this.Date = Date;
            this.DataContext = new MediaItem(DataContext);
        }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public MediaItem DataContext { get; set; }
    }
}
