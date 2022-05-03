using Media_Downloader_App.Core;
using System;
using System.Text.Json;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Media_Downloader_App
{
    internal class Settings
    {
        private static readonly ApplicationDataContainer Local = ApplicationData.Current.LocalSettings;
        public static event EventHandler ThemeChanged;
        public static event EventHandler OutputChanged;
        private static ElementTheme _Theme { get; set; } = ElementTheme.Default;
        public static ElementTheme Theme
        {
            get { return _Theme; }
            set
            {
                _Theme = value;
                OnThemeChanged();
            }
        }
        public static Spotify SpotifyClient = new Spotify();
        public static YouTube YouTubeClient = new YouTube();
        public static string _OutputFolder { get; set; } = String.Empty;
        public static string OutputFolder
        {
            get { return _OutputFolder; }
            set
            {
                _OutputFolder = value;
                OnOutputFolderChanged();
            }
        }
        public static void Save()
        {
            if (string.IsNullOrWhiteSpace(OutputFolder))
            {
                OutputFolder = "Downloads";
            }
            string JsonString = JsonSerializer.Serialize
                (new SerializableSettings
                {
                    Theme = Theme,
                    ID = SpotifyClient.Details.ID,
                    Secret = SpotifyClient.Details.Secret,
                    Authorized = SpotifyClient.Authd,
                    OutputFolder = OutputFolder
                });

            System.Diagnostics.Debug.WriteLine($"[SETTINGS] Saved {JsonString}");
            Local.Values["Settings"] = JsonString;
        }
        public async static void Load()
        {
            try
            {
                SerializableSettings settings = JsonSerializer.Deserialize<SerializableSettings>(
                (string)Local.Values["Settings"]);

                Theme = settings.Theme;
                SpotifyClient.Details = new ClientDetails { ID = settings.ID, Secret = settings.Secret };
                OutputFolder = settings.OutputFolder;
                try
                {
                    await SpotifyClient.Auth();
                }
                catch
                {

                }
            }
            catch
            {

            }
        }
        protected static void OnThemeChanged()
        {
            ThemeChanged?.Invoke(typeof(Settings), EventArgs.Empty);
        }
        protected static void OnOutputFolderChanged()
        {
            OutputChanged?.Invoke(typeof(Settings), EventArgs.Empty);
        }
    }
    public class SerializableSettings
    {
        public ElementTheme Theme { get; set; }
        public string ID { get; set; }
        public string Secret { get; set; }
        public string OutputFolder { get; set; }
        public bool Authorized { get; set; }
    }
    public struct ClientDetails
    {
        public string ID { get; set; }
        public string Secret { get; set; }
    }
}
