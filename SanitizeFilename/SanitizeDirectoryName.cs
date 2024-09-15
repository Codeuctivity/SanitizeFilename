namespace Codeuctivity
{
    internal class SanitizeDirectoryName
    {
        /// <summary>
        /// These are the not supported chars in windows directory names.
        /// </summary>
        public static readonly char[] invalidCharsInWindowsFileNames = SanitizeFilename.InvalidCharsInWindowsFileNames;

        public static string Sanitize(string directoryName, string replacement)
        {
            return SanitizeFilename.Sanitize(directoryName, replacement);
        }

        public static string Sanitize(string directoryName, char replacement = '_')
        {
            return SanitizeFilename.Sanitize(directoryName, replacement);
        }
    }
}