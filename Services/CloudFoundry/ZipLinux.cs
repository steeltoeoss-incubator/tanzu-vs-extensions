using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace TanzuForVS.Services.CloudFoundry
{
    public class ZipArchiver
    {
     
        private const string Packaging = "zip";

        private const string FileExtension = ".zip";

        /* ----------------------------------------------------------------- *
         * Fix UNIX permissions in Zip archive extraction                    *
         *                                             Owner                 *
         *                                                 Group             *
         *                                                     Other         *
         *                                             r w r   r             *
         * ----------------------------------------------------------------- */
        private const int UnixFilePermissions = 0b_0000_0001_1010_0100_0000_0000_0000_0000;
        private const int UnixDirectoryPermissions = 0b_0000_0001_1110_1101_0000_0000_0000_0000;


        private readonly CompressionLevel _compression;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipArchiver"/> class.
        /// </summary>
        /// <param name="compression">Compression level default <see cref="CompressionLevel.Fastest"/>.</param>
        public ZipArchiver(CompressionLevel compression = CompressionLevel.Fastest)
        {
            _compression = compression;
        }


        /// <inheritdoc/>
        public byte[] ToBytes(IEnumerable<FileEntry> fileEntries)
        {
            var buffer = new MemoryStream();

            var archive = new ZipArchive(buffer, ZipArchiveMode.Create, true);
            foreach (var fileEntry in fileEntries)
            {
                var zipEntry = archive.CreateEntry(fileEntry.Path, _compression);
                if (fileEntry.Text == null)
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        zipEntry.ExternalAttributes = UnixDirectoryPermissions;
                    }

                    continue;
                }

                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    zipEntry.ExternalAttributes = UnixFilePermissions;
                }

                // put contents of file into zipEntry
                var textStream = new MemoryStream(Encoding.UTF8.GetBytes(fileEntry.Text)); // dump contents of file into new stream as bytes
                var zipStream = zipEntry.Open();
                textStream.CopyTo(zipStream); // dump stream bytes into zipEntry
            }

            archive.Dispose();
            buffer.Seek(0, SeekOrigin.Begin);
            return buffer.ToArray();
        }

        /// <inheritdoc/>
        public string GetPackaging()
        {
            return Packaging;
        }

        /// <inheritdoc/>
        public string GetFileExtension()
        {
            return FileExtension;
        }
    }
}
