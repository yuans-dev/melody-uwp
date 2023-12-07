using Melody.Core;

namespace Melody.Classes
{
    public struct BrowsingParameters
    {
        public BrowsingParameters(Spotify SpotifyClient, YouTube YouTubeClient, int Results, int Offset)
        {
            this.SpotifyClient = SpotifyClient;
            this.YouTubeClient = YouTubeClient;
            this.Results = Results;
            this.Offset = Offset;
        }
        public Spotify SpotifyClient { get; private set; }
        public YouTube YouTubeClient { get; private set; }
        public int Results { get; private set; }
        public int Offset { get; private set; }
    }
}
