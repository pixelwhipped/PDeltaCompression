namespace Compression.Services
{
    /// <summary>
    /// The IO.Compression Service Provider framework with predifined methods to simplify future implementations.
    /// </summary>
    public class CompressionService : ServiceProvider<CompressionService>
    {
        #region ICompressionProvider Members

        #endregion

        /// <summary>
        /// Compresses data.
        /// </summary>
        /// <param name="bytes">The byte array to be compressed.</param>
        /// <returns>A byte array of compressed data.</returns>
        public virtual byte[] Compress(byte[] bytes)
        {
            return bytes;
        }

        /// <summary>
        /// Decompresses data.
        /// </summary>
        /// <param name="bytes">The byte array to be decompressed.</param>
        /// <returns>A byte array of uncompressed data.</returns>
        public virtual byte[] Decompress(byte[] bytes)
        {
            return bytes;
        }
    }

    /// <summary>
    /// IO.Compression Service Factory creates instances of compression providers.
    /// </summary>
    public static class CompressionServiceFactory
    {
        /// <summary>
        /// GZip service.
        /// </summary>
        private static CompressionService _gZipService;

        /// <summary>
        /// Deflate Service.
        /// </summary>
        private static CompressionService _deflateService;

        /// <summary>
        /// Deflate Service.
        /// </summary>
        private static CompressionService _pZipService;
        /// <summary>
        /// Gets the default random service provider.
        /// </summary>
        /// <returns>The default random serice provider.</returns>
        public static CompressionService GetDefaultService()
        {
            return GetDeflateService();
        }


        /// <summary>
        /// Gets a Deflate compression service provider.
        /// </summary>
        /// <returns>The Deflate compression service.</returns>
        public static CompressionService GetDeflateService()
        {
            return _deflateService ?? (_deflateService = new DeflateService());
        }

        /// <summary>
        /// Gets a GZip compression service.
        /// </summary>
        /// <returns>The GZip compression service.</returns>
        public static CompressionService GetGZipService()
        {
            return _gZipService ?? (_gZipService = new GZipService());
        }

        /// <summary>
        /// Gets a PZip compression service.
        /// </summary>
        /// <returns>The PZip compression service.</returns>
        public static CompressionService GetPZipService()
        {
            return _pZipService ?? (_pZipService = new PZipService());
        }
    }
}