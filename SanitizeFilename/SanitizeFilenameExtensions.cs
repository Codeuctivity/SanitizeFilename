namespace Codeuctivity
{
    public static class SanitizeFilenameExtensions
    {
        public static string Sanitize(this string filename, char replacement = '_')
        {
            var invalidCharsSanitized = SanitizeFilename.Sanitize(filename, replacement);

            return invalidCharsSanitized;
        }
    }
}