namespace Codeuctivity
{
    /// <summary>
    /// Sanitizes a filename by replacing invalid chars with a replacement char. Follows rules defined in https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file
    /// </summary>
    public static class SanitizeFilenameExtensions
    {
        /// <summary>
        /// Sanitizes a filename by replacing invalid chars with a replacement char. Follows rules defined in https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="replacement"></param>
        /// <param name="filenameExtensionHandling"></param>
        /// <returns></returns>
        public static string SanitizeFilename(this string filename, char replacement = '_', FilenameExtensionHandling filenameExtensionHandling = FilenameExtensionHandling.PreserveFilenameExtension)
        {
            var sanitizedFilename = Codeuctivity.SanitizeFilename.Sanitize(filename, replacement, filenameExtensionHandling);

            return sanitizedFilename;
        }

        /// <summary>
        /// Sanitizes a filename by replacing invalid chars with a replacement string. Follows rules defined in https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="replacement"></param>
        /// <param name="filenameExtensionHandling"></param>
        /// <returns></returns>
        public static string SanitizeFilename(this string filename, string replacement, FilenameExtensionHandling filenameExtensionHandling = FilenameExtensionHandling.PreserveFilenameExtension)
        {
            var sanitizedFilename = Codeuctivity.SanitizeFilename.Sanitize(filename, replacement, filenameExtensionHandling);

            return sanitizedFilename;
        }
    }
}