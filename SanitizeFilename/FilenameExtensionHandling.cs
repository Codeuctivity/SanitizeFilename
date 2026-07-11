namespace Codeuctivity
{
    /// <summary>
    /// Defines how file extensions should be handled when truncating long filenames.
    /// </summary>
    public enum FilenameExtensionHandling
    {
        /// <summary>
        /// Preserve the file extension, truncating only the base filename if necessary to fit within the 255-byte limit.
        /// If the extension itself is too long, it will also be truncated to ensure the total filename fits.
        /// </summary>
        PreserveFilenameExtension = 0,

        /// <summary>
        /// Do not preserve the file extension. Truncate the entire filename including the extension.
        /// </summary>
        PreserveFilenameWithoutExtension = 1,

        /// <summary>
        /// Throw an exception if the filename extension is too long to fit within the 255-byte limit.
        /// </summary>
        ThrowWhenFilenameExtensionIsTooLong = 2
    }
}
