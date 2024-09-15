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
        /// <returns></returns>
        public static string SanitizeFilename(this string filename, char replacement = '_')
        {
            var sanitizedFilename = Codeuctivity.SanitizeFilename.Sanitize(filename, replacement);

            return sanitizedFilename;
        }

        /// <summary>
        /// Sanitizes a filename by replacing invalid chars with a replacement string. Follows rules defined in https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string SanitizeFilename(this string filename, string replacement)
        {
            var sanitizedFilename = Codeuctivity.SanitizeFilename.Sanitize(filename, replacement);

            return sanitizedFilename;
        }

        public static string SanitizeDirectoryName(this string filename, char replacement = '_')
        {
            var sanitizedFilename = Codeuctivity.SanitizeDirectoryName.Sanitize(filename, replacement);

            return sanitizedFilename;
        }

        public static string SanitizeDirectoryName(this string filename, string replacement)
        {
            var sanitizedFilename = Codeuctivity.SanitizeDirectoryName.Sanitize(filename, replacement);

            return sanitizedFilename;
        }
    }
}