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
        /// Chars that are invalid in MacOs file names
        /// </summary>
        public static readonly char[] InvalidCharsInMacOsFileNames = [
            (char)889,
            (char)896,
            (char)897,
            (char)898,
            (char)899,
            (char)907,
            (char)909,
            (char)930,
            (char)1328,
            (char)1367,
            (char)1368,
            (char)1419,
            (char)1420,
            (char)1424,
            (char)1480,
            (char)1481,
            (char)1482,
            (char)1483,
            (char)1484,
            (char)1485,
            (char)1486,
            (char)1487,
            (char)1515,
            (char)1516,
            (char)1517,
            (char)1518,
            (char)1525,
            (char)1526,
            (char)1527,
            (char)1528,
            (char)1529,
            (char)1530,
            (char)1531,
            (char)1532,
            (char)1533,
            (char)1534,
            (char)1535,
            (char)1806,
            (char)1867,
            (char)1868,
            (char)1970,
            (char)1971,
            (char)1972,
            (char)1973,
            (char)1974,
            (char)1975,
            (char)1976,
            (char)1977,
            (char)1978,
            (char)1979,
            (char)1980,
            (char)1981,
            (char)1982,
            (char)1983,
            (char)2043,
            (char)2044,
            (char)2094,
            (char)2095,
            (char)2111,
            (char)2140,
            (char)2141,
            (char)2143,
            (char)2155,
            (char)2156,
            (char)2157,
            (char)2158,
            (char)2159,
            (char)2191,
            (char)2194,
            (char)2195,
            (char)2196,
            (char)2197,
            (char)2198,
            (char)2199,
            (char)2436,
            (char)2445,
            (char)2446,
            (char)2449,
            (char)2450,
            (char)2473,
            (char)2481,
            (char)2483,
            (char)2484,
            (char)2485,
            (char)2490,
            (char)2491,
            (char)2501,
            (char)2502,
            (char)2505,
            (char)2506,
            (char)2511,
            (char)2512,
            (char)2513,
            (char)2514,
            (char)2515,
            (char)2516,
            (char)2517,
            (char)2518,
            (char)2520,
            (char)2521,
            (char)2522,
            (char)2523,
            (char)2526,
            (char)2532,
            (char)2533,
            (char)2559,
            (char)2560,
            (char)2564,
            (char)2571,
            (char)2572,
            (char)2573,
            (char)2574,
            (char)2577,
            (char)2578,
            (char)2601,
            (char)2609,
            (char)2612,
            (char)2615,
            (char)2618,
            (char)2619,
            (char)2621,
            (char)2627,
            (char)2628,
            (char)2629,
            (char)2630,
            (char)2633,
            (char)2634,
            (char)2638,
            (char)2639,
            (char)2640,
            (char)2642,
            (char)2643,
            (char)2644,
            (char)2645,
            (char)2646,
            (char)2647,
            (char)2648,
            (char)2653,
            (char)2655,
            (char)2656,
            (char)2657,
            (char)2658,
            (char)2659,
            (char)2660,
            (char)2661,
            (char)2679,
            (char)2680,
            (char)2681,
            (char)2682,
            (char)2683,
            (char)2684,
            (char)2685,
            (char)2686,
            (char)2687,
            (char)2688,
            (char)2692,
            (char)2702,
            (char)2706,
            (char)2729,
            (char)2737,
            (char)2740,
            (char)2746,
            (char)2747,
            (char)2758,
            (char)2762,
            (char)2766,
            (char)2767,
            (char)2769,
            (char)2770
        ];

        /// <summary>
        /// Chars that are invalid in Windows and MacOs file names
        /// </summary>
        public static readonly char[] InvalidCharsInWindowsAndMacOsFileNames = InvalidCharsInWindowsFileNames.Concat(InvalidCharsInMacOsFileNames).ToArray();


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
            filename = RemoveUnpairedSurrogates(filename);

            return filename;
        }

        // replace invalid unicode characters with a replacement character (they were failing on github runner using ubuntu)
        private static string ReplaceInvalidUnicodeChars(string input, string replacement)
        {
            var validChars = new StringBuilder();

            foreach (char c in input)
            {
                if (c <= 0x10FFFF)
                {
                    validChars.Append(c);
                }
                else
                {
                    validChars.Append(replacement);
                }
            }

            return validChars.ToString();
        }

        /// <summary>
        /// Removes unpaired surrogates from a string (Ubuntu does not like that)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveUnpairedSurrogates(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsHighSurrogate(input[i]) && (i < input.Length - 1 && char.IsLowSurrogate(input[i + 1])))
                {
                    // High surrogate is followed by a low surrogate
                    result.Append(input[i]);
                    result.Append(input[i + 1]);
                    // Skip the next character
                    i++;
                }
                else if (char.IsLowSurrogate(input[i]) && (i > 0 && char.IsHighSurrogate(input[i - 1])))
                {
                    // Low surrogate is preceded by a high surrogate
                    result.Append(input[i]);
                }
                else if (!char.IsSurrogate(input[i]))
                {
                    // Not a surrogate
                    result.Append(input[i]);
                }
            }

            return result.ToString();
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

