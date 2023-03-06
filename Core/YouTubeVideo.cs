﻿using Melody.Abstractions;
using Melody.Classes;
using Melody.Statics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TagLib;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Melody.Core
{
    public class YouTubeVideo : IMedia
    {
        public YouTubeVideo(Video Video, bool IsVideo)
        {
            SpotifyTagged = false;
            this.Video = Video;
            Title = Video.Title;
            Authors = new string[1] { Video.Author.Title };
            Number = 1;
            Year = Video.UploadDate.Year.ToString();
            Duration = Video.Duration.Value.TotalMilliseconds;
            if (Video.Duration != null)
            {
                DurationAsTimeSpan = (TimeSpan)Video.Duration;
            }
            else
            {
                DurationAsTimeSpan = TimeSpan.Zero;
            }
            ID = new MediaID(MediaType.YouTubeVideo, Video.Id);
            this.IsVideo = IsVideo;
            Link = new MediaLink(Video.Url);
            Bitmap = new BitmapImage(new Uri(Video.Thumbnails[0].Url, UriKind.Absolute));
            JpgBitmap = new BitmapImage(new Uri(Utils.IsolateJPG(Video.Thumbnails[0].Url), UriKind.Absolute));
        }
        public YouTubeVideo(Video Video, bool IsVideo, string ThumnbailUrl)
        {
            SpotifyTagged = false;
            this.Video = Video;
            Title = Video.Title;
            Authors = new string[1] { Video.Author.Title };
            Number = 1;
            Year = Video.UploadDate.Year.ToString();
            Duration = Video.Duration.Value.TotalMilliseconds;
            if (Video.Duration != null)
            {
                DurationAsTimeSpan = (TimeSpan)Video.Duration;
            }
            else
            {
                DurationAsTimeSpan = TimeSpan.Zero;
            }
            ID = new MediaID(MediaType.YouTubeVideo, Video.Id);
            this.IsVideo = IsVideo;
            Link = new MediaLink(Video.Url);
            Bitmap = new BitmapImage(new Uri(ThumnbailUrl, UriKind.Absolute));
            JpgBitmap = new BitmapImage(new Uri(Utils.IsolateJPG(Video.Thumbnails[0].Url), UriKind.Absolute));
        }
        public YouTubeVideo(YouTubeVideo Video)
        {
            SpotifyTagged = false;
            this.Video = Video.Video;
            Title = Video.Title;
            Authors = Video.Authors;
            Number = Video.Number;
            Year = Video.Year;
            Duration = Video.Duration;
            DurationAsTimeSpan = Video.DurationAsTimeSpan;
            ID = Video.ID;
            IsVideo = Video.IsVideo;
            Link = Video.Link;
            Bitmap = Video.Bitmap;
            JpgBitmap = Video.JpgBitmap;
        }
        public string Name
        {
            get
            {
                return $"{Authors.First()} - {Title}";
            }
        }
        private Video Video { get; set; }
        public bool SpotifyTagged { get; set; }
        public string Title { get; set; }

        public string[] Authors { get; set; }
        public string Album { get; set; }
        public BitmapImage Bitmap { get; set; }
        public BitmapImage JpgBitmap { get; set; }

        public uint Number { get; private set; }

        public double Duration { get; private set; }
        public TimeSpan DurationAsTimeSpan { get; private set; }
        public MediaID ID { get; private set; }
        public MediaLink Link { get; private set; }
        public string Year { get; private set; }
        public bool IsPreviewAvailable { get; private set; } = true;
        public bool IsVideo { get; set; }

        public bool Equals(IMedia other)
        {
            if (other == null)
                return false;

            if (this.Name == other.Name)
                return true;
            else
                return false;
        }
        public override string ToString()
        {
            return "Video";
        }
        public IVideoStreamInfo RequestedVideoQuality { get; set; }
    }
}
