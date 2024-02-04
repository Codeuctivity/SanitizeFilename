﻿using System.Globalization;
using System.Text;

namespace Codeuctivity
{
    /// <summary>
    /// Sanitizes a filename by replacing invalid chars with a replacement char. Follows rules defined in https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file
    /// </summary>
    public static class SanitizeFilename
    {
        /// <summary>
        /// These are the reserved names in Windows. See https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file. While .net can write alle of these filenames on modern environments (except PRN), there are many application that will refuse to read them.
        /// </summary>
        public static readonly string[] ReservedWindowsFileNames = ["CON", "PRN", "AUX", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM\u00B9", "COM\u00B2", "COM\u00B3", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "LPT\u00B9", "LPT\u00B2", "LPT\u00B3"];

        /// <summary>
        /// Values of reserved windows file names with extension. See https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file. While .net can write alle of these filenames on modern environments (except PRN), there are many application that will refuse to read them.
        /// </summary>
        public static readonly string[] ReservedWindowsFileNamesWithExtension = ["CON.", "PRN.", "AUX.", "NUL.", "COM0.", "COM1.", "COM2.", "COM3.", "COM4.", "COM5.", "COM6.", "COM7.", "COM8.", "COM9.", "COM\u00B9.", "COM\u00B2.", "COM\u00B3.", "LPT0.", "LPT1.", "LPT2.", "LPT3.", "LPT4.", "LPT5.", "LPT6.", "LPT7.", "LPT8.", "LPT9.", "LPT\u00B9.", "LPT\u00B2.", "LPT\u00B3."];

        /// <summary>
        ///
        /// </summary>
        public static readonly char[] InvalidCharsInWindowsFileNames = ['\\',
            '/',
            '\"',
            '<',
            '>',
            '|',
            ':',
            '*',
            '?',
            '\0',
            (char)1,
            (char)2,
            (char)3,
            (char)4,
            (char)5,
            (char)6,
            (char)7,
            (char)8,
            (char)9,
            (char)10,
            (char)11,
            (char)12,
            (char)13,
            (char)14,
            (char)15,
            (char)16,
            (char)17,
            (char)18,
            (char)19,
            (char)20,
            (char)21,
            (char)22,
            (char)23,
            (char)24,
            (char)25,
            (char)26,
            (char)27,
            (char)28,
            (char)29,
            (char)30,
            (char)31,
        ];

        /// <summary>
        /// These chars are invalid in Windows file names
        /// </summary>
        public static readonly string[] InvalidTrailingChars = [".", " "];

        /// <summary>
        /// These chars are invalid in Unix file names
        /// </summary>

        public static readonly char[] InvalidCharsInUnixFileNames = ['/', '\0'];
        /// <summary>
        /// FallbackFileName is used in cases where the sanitized file name would result in an empty string.
        /// </summary>
        public static readonly string FallbackFileName = "FileName";

        /// <summary>
        /// Sanitizes a filename by replacing invalid chars with a replacement char. Follows rules defined in https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string Sanitize(string filename, char replacement = '_')
        {
            ReplacementSanityCheck(replacement);

            string saneFilename = InternalSanitize(filename, replacement);
            return UnicodeSafeStringTruncate(saneFilename);
        }

        private static void ReplacementSanityCheck(char replacement)
        {
            if (InvalidCharsInUnixFileNames.Contains(replacement))
                throw new ArgumentException($"Replacement char '{replacement}' is invalid for Unix file names", nameof(replacement));

            if (InvalidCharsInWindowsFileNames.Contains(replacement))
                throw new ArgumentException($"Replacement char '{replacement}' is invalid for Windows file names", nameof(replacement));

        }

        private static string InternalSanitize(string filename, char replacement)
        {
            var invalidCharsFileNamesSanitized = InternalSanitizeChars(filename, replacement, InvalidCharsInWindowsFileNames);
            var reservedFileNamesSanitized = InternalSanitizeReservedFileNames(invalidCharsFileNamesSanitized, $"{replacement}");
            var reservedFileNamePrefixSanitized = InternalSanitizeReservedFileNamePrefix(reservedFileNamesSanitized, $"{replacement}");
            var trailingCharSanitized = RemoveTrailingPeriodOrSpace(reservedFileNamePrefixSanitized, $"{replacement}");

            if (string.IsNullOrEmpty(trailingCharSanitized) || trailingCharSanitized == " " || trailingCharSanitized == ".")
                return FallbackFileName;

            if (trailingCharSanitized == filename)
                return trailingCharSanitized;

            return InternalSanitize(trailingCharSanitized, replacement);
        }

        private static string InternalSanitizeChars(string filename, char replacement, char[]? invalidChars = null)
        {
            var usedInvalidChars = invalidChars ?? Path.GetInvalidFileNameChars();
            var sanitizedFilename = new string(filename.Select(c => usedInvalidChars.Contains(c) ? replacement : c).ToArray());
            return sanitizedFilename;
        }

        private static string InternalSanitizeReservedFileNames(string filename, string replacement)
        {
            foreach (var reservedFileName in ReservedWindowsFileNames)
                filename = filename.Replace(reservedFileName, replacement, true, CultureInfo.InvariantCulture);

            return filename;
        }

        private static string InternalSanitizeReservedFileNamePrefix(string filename, string replacement)
        {
            foreach (var reservedFileNamePrefix in ReservedWindowsFileNamesWithExtension)
                if (filename.StartsWith(reservedFileNamePrefix, true, CultureInfo.InvariantCulture))
                    filename = string.Concat(replacement, filename.AsSpan(0, reservedFileNamePrefix.Length));

            return filename;
        }

        private static string RemoveTrailingPeriodOrSpace(string filename, string replacement)
        {
            foreach (var InvalidTrailingChar in InvalidTrailingChars)
                if (filename.EndsWith(InvalidTrailingChar, true, CultureInfo.InvariantCulture))
                    return filename[..^1] + replacement;

            return filename;
        }

        static string UnicodeSafeStringTruncate(string longFileName)
        {

            // Rule working for NTFS and most other file systems
            if (Encoding.UTF8.GetByteCount(longFileName) <= 255)
            {
                return longFileName;
            }

            // Rule working for NTFS and most other file systems
            //if (longFileName.Length <= 255)
            //{
            //    return longFileName;
            //}


            var builder = new StringBuilder();
            var builderForward = new StringBuilder();

            var textElementEnumerator = StringInfo.GetTextElementEnumerator(longFileName);

            int textElementCount = 1;
            while (textElementEnumerator.MoveNext())
            {
                builderForward.Append(textElementEnumerator.Current);

                if (Encoding.UTF8.GetByteCount(builderForward.ToString()) > 255)
                {
                    return builder.ToString();
                }

                // Rule working for NTFS and most other file systems
                //if (builderForward.ToString().Length > 255)
                //{
                //    return builder.ToString();
                //}

                builder.Append(textElementEnumerator.Current);
                textElementCount++;
            }

            return longFileName;
        }
    }
}

