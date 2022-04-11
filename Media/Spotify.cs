using Media_Downloader_App;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MP3DL.Media
{
    public class Spotify
    {
        public Spotify()
        {
            Details.ID = "";
            Details.Secret = "";
        }
        public ClientDetails Details;
        public bool Authd { get; private set; } = false;

        public SpotifyClient Client;
        public event EventHandler<CollectionProgressEventArgs> CollectionFetchingProgressChanged;
        public event EventHandler CollectionFetchingDone;
        public async Task Auth()
        {
            try
            {
                var config = SpotifyClientConfig
                    .CreateDefault()
                    .WithRetryHandler(new SimpleRetryHandler() { RetryAfter = TimeSpan.FromSeconds(2), RetryTimes = 2 });
                var request = new ClientCredentialsRequest(Details.ID, Details.Secret);
                var response = await new OAuthClient(config).RequestToken(request);

                Client = new SpotifyClient(config.WithToken(response.AccessToken));
                Debug.WriteLine("[SPOTIFY CLIENT] Auth Success!");
                Authd = true;
            }
            catch (ArgumentNullException)
            {
                Authd = false;
                throw new ArgumentNullException("Must set Client ID and Secret");
            }
            catch (Exception)
            {
                Authd = false;
                throw new ArgumentException("Invalid Client ID and Secret");
            }
        }
        public async Task<SpotifyTrack> GetTrack(string TRACK_ID)
        {
            try
            {
                var tempx = await Client.Tracks.Get(TRACK_ID);
                var tempy = new SpotifyTrack(tempx, await Client.Albums.Get(tempx.Album.Id));

                return tempy;
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid ID! Please enter a valid track ID");
            }
        }
        public async Task<SpotifyAlbum> GetAlbum(string ALBUM_ID)
        {
            try
            {
                var tempx = await Client.Albums.Get(ALBUM_ID);
                var tempy = new SpotifyAlbum(tempx);

                return tempy;
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid ID! Please enter a valid album ID");
            }
        }
        public async Task<SpotifyPlaylist> GetPlaylist(string PLAYLIST_ID)
        {
            try
            {
                var tempx = await Client.Playlists.Get(PLAYLIST_ID);
                var tempy = new SpotifyPlaylist(tempx);

                return tempy;
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid ID! Please enter a valid playlist ID");
            }
        }
        public async Task<FullPlaylist> GetFullPlaylist(string PLAYLIST_ID)
        {
            try
            {
                Debug.WriteLine(PLAYLIST_ID);
                return await Client.Playlists.Get(PLAYLIST_ID);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid ID! Please enter a valid playlist ID");
            }
        }
        public async Task<string> SearchTrack(string SearchQuery, int Index)
        {
            var search = await Client.Search.Item(new SearchRequest(SearchRequest.Types.Track, SearchQuery));
            var item = search.Tracks.Items[Index].Id;
            return item;
        }
        public async Task<string> SearchPlaylist(string SearchQuery, int Index)
        {
            var search = await Client.Search.Item(new SearchRequest(SearchRequest.Types.Playlist, SearchQuery));
            var item = search.Playlists.Items[Index].Id;
            return item;
        }
        public async Task<List<SpotifyTrack>> BrowseSpotifyTracks(string BrowseQuery, int Results, int Offset)
        {
            SearchResponse search = await Client.Search.Item(new SearchRequest(SearchRequest.Types.Track, BrowseQuery));
            List<SpotifyTrack> temp = new List<SpotifyTrack>();

            if (search.Tracks.Items.Count >= Results)
            {
                for (int i = 0; i < Results; i++)
                {
                    temp.Add(new SpotifyTrack(search.Tracks.Items[i], search.Tracks.Items[i].Album));
                }
            }
            else
            {
                for (int i = 0; i < search.Tracks.Items.Count; i++)
                {
                    temp.Add(new SpotifyTrack(search.Tracks.Items[i], search.Tracks.Items[i].Album));
                }
            }
            return temp;
        }
        public async Task<List<SpotifyPlaylist>> BrowseSpotifyPlaylist(string BrowseQuery, int Results, int Offset)
        {
            SearchResponse search = await Client.Search.Item(new SearchRequest(SearchRequest.Types.Playlist, BrowseQuery));
            List<SpotifyPlaylist> temp = new List<SpotifyPlaylist>();

            if(Results == 0)
            {
                for (int i = 0; i < search.Playlists.Items.Count; i++)
                {
                    try
                    {
                        temp.Add(new SpotifyPlaylist(search.Playlists.Items[i + Offset]));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < Results; i++)
                {
                    try
                    {
                        temp.Add(new SpotifyPlaylist(search.Playlists.Items[i + Offset]));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            return temp;
        }
        public async Task<List<SpotifyAlbum>> BrowseSpotifyAlbum(string BrowseQuery, int Results, int Offset)
        {
            SearchResponse search = await Client.Search.Item(new SearchRequest(SearchRequest.Types.Album, BrowseQuery));
            List<SpotifyAlbum> temp = new List<SpotifyAlbum>();

            if (Results == 0)
            {
                for (int i = 0; i < search.Albums.Items.Count; i++)
                {
                    try
                    {
                        temp.Add(new SpotifyAlbum(search.Albums.Items[i + Offset]));
                    }
                    catch { break; }
                }
            }
            else
            {
                for (int i = 0; i < Results; i++)
                {
                    try
                    {
                        temp.Add(new SpotifyAlbum(search.Albums.Items[i + Offset]));
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            return temp;
        }
        public async Task<List<SpotifyTrack>> GetPlaylistTracks(SpotifyPlaylist Playlist)
        {
            Debug.WriteLine("[SPOTIFY CLIENT] Getting Playlist Tracks");
            var fullplaylist = await Client.Playlists.Get(Playlist.ID);
            Debug.WriteLine($"[SPOTIFY CLIENT] Found {fullplaylist.Name}");
            var temp = new List<SpotifyTrack>();

            Debug.WriteLine($"[SPOTIFY CLIENT] {Playlist.MediaCount} Total IDs found in playlist");

            int x = (int)Math.Ceiling((decimal)Playlist.MediaCount / 100);
            int finished = 0;
            int total = (int)Playlist.MediaCount;
            Debug.WriteLine($"[SPOTIFY CLIENT] Playlist going through {x} loop(s)--");
            for (int i = 0; i < x; i++)
            {
                foreach (PlaylistTrack<IPlayableItem> item in fullplaylist.Tracks.Items)
                {
                    if (item.Track is FullTrack track)
                    {
                        try
                        {
                            temp.Add(new SpotifyTrack(track, track.Album));
                            Debug.WriteLine($"[SPOTIFY CLIENT] Added {track.Name} | Loop {i + 1}");
                        }
                        catch(System.ArgumentNullException ex)
                        {
                            Debug.WriteLine($"[SPOTIFY CLIENT] Null item error ({ex.Message}) | Source: {ex.Source} from {ex.TargetSite}");
                        }
                        catch (System.ArgumentOutOfRangeException e)
                        {
                            Debug.WriteLine($"[SPOTIFY CLIENT] Out of range error ({e.Message}) | Source: {e.Source} from {e.TargetSite}");
                        }
                    }
                    finished++;
                    OnPlaylistFetchingProgressChanged(finished, total);
                }
                await Offset(fullplaylist, i);
            }
            return temp;
        }
        public async Task<List<SpotifyTrack>> GetAlbumTracks(SpotifyAlbum Album)
        {
            Debug.WriteLine("[SPOTIFY CLIENT] Getting Album Tracks");
            var fullalbum = await Client.Albums.Get(Album.ID);
            Debug.WriteLine($"[SPOTIFY CLIENT] Found {fullalbum.Name}");
            var temp = new List<SpotifyTrack>();

            Debug.WriteLine($"[SPOTIFY CLIENT] {Album.MediaCount} Total IDs found in album");

            int x = (int)Math.Ceiling((decimal)Album.MediaCount / 100);
            int finished = 0;
            int total = (int)Album.MediaCount;
            Debug.WriteLine($"[SPOTIFY CLIENT] Album going through {x} loop(s)--");
            for (int i = 0; i < x; i++)
            {
                foreach (SimpleTrack item in fullalbum.Tracks.Items)
                {
                    try
                    {
                        temp.Add(new SpotifyTrack(item, fullalbum));
                        Debug.WriteLine($"[SPOTIFY CLIENT] Added {item.Name} | Loop {i+1}");
                    }
                    catch (System.ArgumentNullException ex)
                    {
                        Debug.WriteLine($"[SPOTIFY CLIENT] Null item error ({ex.Message}) | Source: {ex.Source} from {ex.TargetSite}");
                    }
                    catch (System.ArgumentOutOfRangeException e)
                    {
                        Debug.WriteLine($"[SPOTIFY CLIENT] Out of range error ({e.Message}) | Source: {e.Source} from {e.TargetSite}");
                    }
                    finished++;
                    OnPlaylistFetchingProgressChanged(finished, total);
                }
                await Offset(fullalbum, i);
            }
            return temp;
        }
        private async Task Offset(FullPlaylist Playlist, int Index)
        {
            int offset = (Index + 1) * 100;
            Playlist.Tracks = await Client.Playlists.GetItems
                (Playlist.Id,
                new PlaylistGetItemsRequest { Offset = offset });

        }
        private async Task Offset(SpotifyPlaylist Playlist, int Index)
        {
            int offset = (Index + 1) * 100;
            Playlist.Media = await Client.Playlists.GetItems
                (Playlist.ID,
                new PlaylistGetItemsRequest { Offset = offset });

        }
        private async Task Offset(SimplePlaylist Playlist, int Index)
        {
            int offset = (Index + 1) * 100;
            Playlist.Tracks = await Client.Playlists.GetItems
                (Playlist.Id,
                new PlaylistGetItemsRequest { Offset = offset });

        }
        private async Task Offset(FullAlbum Album, int Index)
        {
            int offset = (Index + 1) * 100;
            Album.Tracks = await Client.Albums.GetTracks
                (Album.Id,
                new AlbumTracksRequest { Offset = offset });

        }
        protected virtual void OnPlaylistFetchingProgressChanged(int Finished, int Total)
        {
            CollectionFetchingProgressChanged?.Invoke(this,
                new CollectionProgressEventArgs()
                {
                    Total = Total,
                    Finished = Finished,
                });
        }
        protected virtual void OnCollectionFetchingDone()
        {
            CollectionFetchingDone?.Invoke(this, EventArgs.Empty);
        }
    }
    public class CollectionProgressEventArgs : EventArgs
    {
        public int Finished { get; set; }
        public int Total { get; set; }
    }
}