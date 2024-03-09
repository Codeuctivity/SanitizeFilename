using System.Globalization;
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
        /// These are the not supported chars in windows file names.
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
        /// Chars that trigger an System.IO.IOException: 'Illegal byte sequence' on MacOs, these are all in the UnicodeCategory.OtherNotAssigned category (with two exceptions), see https://docs.microsoft.com/en-us/dotnet/api/system.globalization.unicodecategory
        /// </summary>
        public static readonly char[] InvalidCharsInMacOsFileNames = [
            //NonSpacingMark
            (char)3790,
            //SpacingCombiningMark
            (char)3315,
        ];

        /// <summary>
        /// Chars that are invalid in Windows and MacOs file names
        /// </summary>
        public static readonly char[] InvalidCharsInWindowsAndMacOsFileNames = [.. InvalidCharsInWindowsFileNames, .. InvalidCharsInMacOsFileNames];

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

        /// <summary>
        /// Sanitizes a filename by replacing invalid chars with a replacement char. Follows rules defined in https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string Sanitize(string filename, string replacement)
        {
            if (char.TryParse(replacement, out var replacementChar))
                ReplacementSanityCheck(replacementChar);

            string saneFilename = InternalSanitize(filename, replacement);
            return UnicodeSafeStringTruncate(saneFilename);
        }

        private static void ReplacementSanityCheck(char replacement)
        {
            if (InvalidCharsInUnixFileNames.Contains(replacement))
                throw new ArgumentException($"Replacement '{replacement}' is invalid for Unix", nameof(replacement));

            if (InvalidCharsInWindowsFileNames.Contains(replacement))
                throw new ArgumentException($"Replacement '{replacement}' is invalid for Windows", nameof(replacement));

            if (InvalidCharsInMacOsFileNames.Contains(replacement))
                throw new ArgumentException($"Replacement '{replacement}' is invalid for MacOs", nameof(replacement));
        }

        private static string InternalSanitize(string filename, char replacement)
        {
            return InternalSanitize(filename, replacement.ToString());
        }

        private static string InternalSanitize(string filename, string replacement)
        {
            var invalidCharsFileNamesSanitized = InternalSanitizeChars(filename, replacement, InvalidCharsInWindowsAndMacOsFileNames);
            var reservedFileNamesSanitized = InternalSanitizeReservedFileNames(invalidCharsFileNamesSanitized, $"{replacement}");
            var reservedFileNamePrefixSanitized = InternalSanitizeReservedFileNamePrefix(reservedFileNamesSanitized, $"{replacement}");
            var trailingCharSanitized = RemoveTrailingPeriodOrSpace(reservedFileNamePrefixSanitized, $"{replacement}");

            if (string.IsNullOrEmpty(trailingCharSanitized) || trailingCharSanitized == " " || trailingCharSanitized == ".")
                return FallbackFileName;

            if (trailingCharSanitized == filename)
                return trailingCharSanitized;

            return InternalSanitize(trailingCharSanitized, replacement);
        }

        private static string InternalSanitizeChars(string filename, string replacement, char[]? invalidChars = null)
        {
            var usedInvalidChars = invalidChars ?? Path.GetInvalidFileNameChars();

            foreach (var invalidChar in usedInvalidChars)
                filename = filename.Replace(invalidChar.ToString(), replacement, StringComparison.Ordinal);

            filename = ReplaceInvalidUnicodeChars(filename, replacement);
            filename = RemoveUnpairedSurrogates(filename, replacement);

            return filename;
        }

        // replace invalid unicode characters with a replacement character (they were failing on github runner using ubuntu)
        private static string ReplaceInvalidUnicodeChars(string input, string replacement)
        {
            var validChars = new StringBuilder();

            foreach (char c in input)
            {
                if (IsUnassignedUnicodeCodePoint(c))
                {
                    validChars.Append(replacement);
                }
                else
                {
                    validChars.Append(c);
                }
            }

            return validChars.ToString();
        }

      

        /// <summary>
        /// Removes unpaired surrogates from a string (some systems dont support unpaired surrogates for filenames)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string RemoveUnpairedSurrogates(string input, string replacement)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder();
            var enumerator = StringInfo.GetTextElementEnumerator(input);

            while (enumerator.MoveNext())
            {
                var element = enumerator.GetTextElement();

                // No surrogate
                if (element != null && !IsSurrogate(element))
                {
                    if (IsUnassignedUnicodeCodePoint(element))
                    {
                        result.Append(replacement);
                    }
                    else
                    {
                        result.Append(element);
                    }
                }
                // Paired surrogate
                else if (element != null && element.Length > 1 && char.IsSurrogatePair(element[0], element[1]))
                {
                    // Check for Undefined Character
                    if (IsUnassignedUnicodeCodePoint(element) || IsSurrogate(element))
                    {
                        result.Append(replacement);
                    }
                    else
                    {
                        result.Append(element);
                    }
                }
                // Unpaired surrogate
                else
                {
                    result.Append(replacement);
                }
            }

            return result.ToString();
        }

        private static bool IsUnassignedUnicodeCodePoint(char c)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            // Undefined Character
            return category == UnicodeCategory.OtherNotAssigned;
        }

        private static bool IsUnassignedUnicodeCodePoint(string c)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c, c.Length - 1);
            // Undefined Character
            return category == UnicodeCategory.OtherNotAssigned;
        }

        private static bool IsSurrogate(string c)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c, c.Length - 1);
            // Undefined Character
            return category == UnicodeCategory.Surrogate;
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

        private static string UnicodeSafeStringTruncate(string longFileName)
        {
            // Most filenames are shorter than 255 bytes, so we can avoid the expensive string enumeration in most cases
            if (FileNameLengthIsExt4Compatible(longFileName))
            {
                return longFileName;
            }

            var builder = new StringBuilder();
            var builderForward = new StringBuilder();

            var textElementEnumerator = StringInfo.GetTextElementEnumerator(longFileName);

            while (textElementEnumerator.MoveNext())
            {
                builderForward.Append(textElementEnumerator.Current);

                // Rule working for EXT4 and most other file systems
                if (!FileNameLengthIsExt4Compatible(builderForward.ToString()))
                {
                    return builder.ToString();
                }

                builder.Append(textElementEnumerator.Current);
            }

            return longFileName;
        }

        private static bool FileNameLengthIsExt4Compatible(string longFileName)
        {
            // Rule that would be working for NTFS
            //longFileName.Length <= 255

            return Encoding.UTF8.GetByteCount(longFileName) <= 255;
        }
    }
}