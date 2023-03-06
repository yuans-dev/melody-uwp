using Melody.Core;

namespace Melody.Classes
{
    public class PagingOptions
    {
        public PagingOptions(Spotify SpotifyClient, YouTube YouTubeClient, string Query, int Results, int Offset)
        {
            this.SpotifyClient = SpotifyClient;
            this.YouTubeClient = YouTubeClient;
            this.Query = Query;
            this.Results = Results;
            this.Offset = Offset;
        }
        public Spotify SpotifyClient { get; private set; }
        public YouTube YouTubeClient { get; private set; }
        public string Query { get; private set; }
        public int Results { get; private set; }
        public int Offset { get; private set; }
    }
}
