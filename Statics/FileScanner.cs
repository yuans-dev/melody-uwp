using MP3DL.Media;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;

namespace MP3DL
{
    public class FileScanner
    {
        public static List<StorageFile> Scan(List<StorageFolder> directories,string FileExtension)
        {
            var tmp = new ConcurrentBag<StorageFile>();

            Parallel.ForEach(directories, async directory =>
            {
                foreach (var file in await ProcessDirectoryAsync(directory))
                {
                    if (file.Path.EndsWith(FileExtension))
                    {
                        try
                        {
                            if (!tmp.Contains(file))
                            {
                                tmp.Add(file);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            });

            return tmp.ToList();
        }
        private async static Task<List<StorageFile>> ProcessDirectoryAsync(StorageFolder directory)
        {
            var subdirectories = await directory.GetFoldersAsync();
            var files = (await directory.GetFilesAsync()).ToList();

            foreach (var subdirectory in subdirectories)
            {
                files.AddRange(await ProcessDirectoryAsync(subdirectory));
            }
            return files;
        }
    }
}
