// Based on reference implementation from:
// https://github.com/windowsgamessamples/UnityPorting

using System;
using System.IO;
#if NETFX_CORE
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
#endif


namespace LegacySystem.IO
{
    public class File
    {

        public static void Delete(string path)
        {
#if NETFX_CORE
            path = FixPath(path);
            var thread = DeleteAsync(path);
            thread.Wait();
#else
            throw new NotImplementedException();
#endif
        }

        public static StreamWriter AppendText(string path)
        {
#if NETFX_CORE
            path = FixPath(path);
            var thread = AppendTextAsync(path);
            thread.Wait();

            if (thread.IsCompleted)
                return thread.Result;

            throw thread.Exception;
#else
            throw new NotImplementedException();
#endif
        }

        public static StreamReader OpenText(string path)
        {
#if NETFX_CORE
            path = FixPath(path);
            var thread = OpenTextAsync(path);
            thread.Wait();

            if (thread.IsCompleted)
                return thread.Result;

            throw thread.Exception;
#else
            throw new NotImplementedException();
#endif
        }


#if NETFX_CORE

        private static async Task<StreamReader> OpenTextAsync(string path)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            var stream = await file.OpenStreamForReadAsync();
            return new StreamReader(stream);
        }

        private static string FixPath(string path)
        {
            return path.Replace('/', '\\');
        }

        private static async Task<StreamWriter> AppendTextAsync(string path)
        {
            var str = await AppendAsync(path);
            return new StreamWriter(str);
        }

        private static async Task DeleteAsync(string path)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            if (file != null)
                await file.DeleteAsync();
        }

        private static async Task<Stream> AppendAsync(string path)
        {
            var dirName = Path.GetDirectoryName(path);
            var filename = Path.GetFileName(path);

            var dir = await StorageFolder.GetFolderFromPathAsync(dirName);
            //var file = await dir.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            var file = await dir.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);

            var stream = await file.OpenStreamForWriteAsync();
            stream.Seek(0, SeekOrigin.End);

            return stream;
        }

#endif // NETFX_CORE

    } // class File
} // namespace LegacySystem.IO
