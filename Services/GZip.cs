using System;
using System.IO;
using System.IO.Compression;

namespace Compression.Services
{
    /// <summary>
    /// Utility class used for compressing data.
    /// using GZip.
    /// </summary>
    public class GZipService : CompressionService
    {
        #region Functions

        /// <summary>
        /// Compresses data.
        /// </summary>
        /// <param name="bytes">The byte array to be compressed.</param>
        /// <returns>A byte array of compressed data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes" /> is <c>null</c>.</exception>
        public override byte[] Compress(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException();
            byte[] values;
            using (var stream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(stream, CompressionMode.Compress, true))
                {
                    zipStream.Write(bytes, 0, bytes.Length);
                }
                values = stream.ToArray();
            }
            return values;
        }

        /// <summary>
        /// Decompresses data.
        /// </summary>
        /// <param name="bytes">The byte array to be decompressed.</param>
        /// <returns>A byte array of uncompressed data.</returns>
        public override byte[] Decompress(byte[] bytes)
        {
            byte[] values;
            using (var stream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress, true))
                {
                    var buffer = new byte[4096];
                    while (true)
                    {
                        var size = zipStream.Read(buffer, 0, buffer.Length);
                        if (size > 0)
                        {
                            stream.Write(buffer, 0, size);
                            continue;
                        }
                        break;
                    }
                }
                values = stream.ToArray();
            }
            return values;
        }

        #endregion
    }
}