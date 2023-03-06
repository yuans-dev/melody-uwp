namespace Melody.Core
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
