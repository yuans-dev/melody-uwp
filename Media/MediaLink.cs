using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3DL.Media
{
    public struct MediaLink
    {
        public MediaLink(string App, string Web)
        {
            this.App = App;
            this.Web = Web;
        }
        public MediaLink(string Web)
        {
            App = Web;
            this.Web = Web;
        }
        public string App { get; set; }
        public string Web { get; set; }
    }
}
