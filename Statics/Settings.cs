using Melody.Classes;
using Melody.Core;
using Melody.Statics;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Melody
{
    internal class Settings
    {
        public Settings Current = new Settings();
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
        public static StorageFolder TemporaryFolder { get; set; } = ApplicationData.Current.TemporaryFolder;
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
                    OutputFolder = OutputFolder
                });

            Local.Values["Settings"] = JsonString;
        }
        public async static void Load()
        {
            try
            {
                var spotifycredentials = await StorageFile.GetFileFromApplicationUriAsync((new Uri(@"ms-appx:///s_cred.json")));
                SpotifyClient.Details = JsonSerializer.Deserialize<ClientDetails>(await FileIO.ReadTextAsync(spotifycredentials));
                try
                {
                    await SpotifyClient.Auth();
                }
                catch
                {

                }
                SerializableSettings settings = JsonSerializer.Deserialize<SerializableSettings>(
                (string)Local.Values["Settings"]);
                

                Theme = settings.Theme;
                OutputFolder = settings.OutputFolder;
                
                
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
        public string OutputFolder { get; set; }
    }
    public struct ClientDetails
    {
        public string ID { get; set; }
        public string Secret { get; set; }
    }
}
