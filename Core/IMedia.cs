using Melody.Classes;
using System;
using System.Drawing;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace Melody.Core
{
    public interface IMedia : IEquatable<IMedia>, IBaseMedia
    {
        string Album { get; set; }
        uint Number { get; }
        string Year { get; }
        double Duration { get; }
        MediaLink Link { get; }
        bool IsVideo { get; }
        bool IsPreviewAvailable { get; }
    }
}
