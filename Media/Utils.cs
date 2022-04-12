namespace MP3DL.Media
{
    internal class Utils
    {
        public static string ClearChars(string input)
        {
            string tmp = input;
            tmp = tmp.Replace('/', '-');
            tmp = tmp.Replace('|', ' ');
            tmp = tmp.Replace('\"', ' ');
            tmp = tmp.Replace('[', ' ');
            tmp = tmp.Replace(']', ' ');
            tmp = tmp.Replace('{', ' ');
            tmp = tmp.Replace('}', ' ');
            tmp = tmp.Replace('\'', ' ');
            tmp = tmp.Replace(',', ' ');
            tmp = tmp.Replace('.', ' ');
            tmp = tmp.Replace(':', ' ');
            tmp = tmp.Replace('?', ' ');
            tmp = tmp.Replace('*', ' ');
            return tmp;
        }
        public static string IsolateJPG(string LINK)
        {
            try
            {
                int a = LINK.LastIndexOf(".jpg");
                return LINK.Substring(0, a + 4);
            }
            catch
            {
                return "";
            }
        }
        public static bool IsSpotifyLink(string Query)
        {
            if (Query.Contains("open.spotify.com"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static SpotifyLink GetSpotifyLink(string Link)
        {
            SpotifyLink temp = new SpotifyLink
            {
                Link = Link,
                ID = IsolateID(Link)
            };
            if (Link.Contains("/track/"))
            {
                temp.Type = 0;
            }
            else if (Link.Contains("/playlist/"))
            {
                temp.Type = 1;
            }
            else if (Link.Contains("/album/"))
            {
                temp.Type = 2;
            }
            else
            {
                temp.Type = 3;
            }
            return temp;
        }
        public static string IsolateID(string Link)
        {
            //https://open.spotify.com/playlist/37i9dQZF1EIUn3pmwSRorE?si=4f9ec77d3dc04759
            var start = Link.LastIndexOf('/') + 1;
            var end = Link.LastIndexOf('?');
            if(end < 0) { end = Link.Length; }
            return Link.Substring(start, end - start);
        }
    }
    public struct SpotifyLink
    {
        public int Type { get; set; }
        public string ID { get; set; }
        public string Link { get; set; }
    }
}
