using System;
using System.IO;
using Windows.Storage;

namespace Melody.Abstractions
{
    public class StorageFileAbstraction : TagLib.File.IFileAbstraction
    {
        private readonly StorageFile file;

        public string Name => file.Name;

        public Stream ReadStream
        {
            get
            {
#pragma warning disable DF0023 // Disposing of this is handled by the TagLibSharp lib by calling the CloseStream method defined here
                return file.OpenStreamForReadAsync().GetAwaiter().GetResult();
#pragma warning restore DF0023 // Disposing of this is handled by the TagLibSharp lib by calling the CloseStream method defined here
            }
        }

        public Stream WriteStream
        {
            get
            {

#pragma warning disable DF0023 // Disposing of this is handled by the TagLibSharp lib by calling the CloseStream method defined here
                return file.OpenStreamForWriteAsync().GetAwaiter().GetResult();
#pragma warning restore DF0023 // Disposing of this is handled by the TagLibSharp lib by calling the CloseStream method defined here
            }
        }


        public StorageFileAbstraction(StorageFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            this.file = file;
        }


        public void CloseStream(Stream stream)
        {
            stream?.Dispose();
        }
    }
}
