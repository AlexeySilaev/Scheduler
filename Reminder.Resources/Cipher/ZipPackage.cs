using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;

namespace Reminder.Resources.Cipher
{
    public class ZipPackage
    {
        private static long BUFFER_SIZE = 4 * 1024;

        public string ZipFilePath { get; }
        public string PackageFolderPath { get; }

        public ZipPackage(string zipFilePath, string packageFolderPath)
        {
            ZipFilePath = zipFilePath;
            PackageFolderPath = packageFolderPath;
        }

        public void Unzip(bool skipExisting)
        {
            using (var zip = Package.Open(File.OpenRead(ZipFilePath)))
            {
                foreach (PackagePart part in zip.GetParts())
                {
                    string target = Path.GetFullPath(Path.Combine(PackageFolderPath, part.Uri.OriginalString.TrimStart('/')));
                    if (File.Exists(target))
                    {
                        if (skipExisting)
                            continue;
                        File.Delete(target);
                    }

                    using (Stream source = part.GetStream(FileMode.Open, FileAccess.Read))
                    {
                        EnsureFolderExists(Path.GetDirectoryName(target));
                        using (FileStream targetFile = File.OpenWrite(target))
                        {
                            source.CopyTo(targetFile);
                        }
                    }
                }
            }
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                string parentFolderPath = Path.GetDirectoryName(folderPath);
                EnsureFolderExists(parentFolderPath); // для возможного понижения версии .NET
                Directory.CreateDirectory(folderPath);
            }
        }

        public void Zip(string[] filters)
        {

            string[] filePaths = filters.SelectMany(filter => Directory.GetFiles(PackageFolderPath, filter, SearchOption.AllDirectories)).ToArray();

            foreach (string file in filePaths)
            {
                using (Package zip = Package.Open(ZipFilePath, FileMode.Create))
                {
                    string destFilename = file.Substring(PackageFolderPath.Length);
                    Uri uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
                    if (zip.PartExists(uri))
                    {
                        zip.DeletePart(uri);
                    }
                    PackagePart part = zip.CreatePart(uri, string.Empty, CompressionOption.Normal);
                    using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite))
                    {
                        using (Stream dest = part.GetStream())
                        {
                            CopyStream(fileStream, dest);
                            dest.Close();
                        }
                        fileStream.Close();
                    }
                }
            }
        }

        private static void CopyStream(FileStream inputStream, Stream outputStream)
        {
            long bufferSize = inputStream.Length < BUFFER_SIZE ? inputStream.Length : BUFFER_SIZE;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            long bytesWritten = 0;
            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                outputStream.Write(buffer, 0, bytesRead);
                bytesWritten += bufferSize;
            }
        }
    }
}
