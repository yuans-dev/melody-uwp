﻿using System;
using System.Drawing;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MP3DL.Media
{
    public interface IMedia : IEquatable<IMedia>
    {
        string Name { get; }
        string Title { get; set; }
        string[] Authors { get; }
        string PrintedAuthors { get; set; }
        string FirstAuthor { get; }
        string Album { get; set; }
        Image Art { get; }
        BitmapImage Bitmap { get; }
        uint Number { get; }
        string Year { get; }
        double Duration { get; }
        string ID { get; }
        bool IsVideo { get; }
        bool IsPreviewAvailable { get; }
        void SetTagsAsync(StorageFile file);
    }
}
