using Codeuctivity;
using System.Runtime.InteropServices;

namespace SanitizeFilenameTests
{
    [Parallelizable(ParallelScope.Fixtures)]
    public class SanitizeFilenamesTests : SanitizeFilenamesTestsBase
    {
        public SanitizeFilenamesTests()
        {
            FileWriteAsserter = new FileWriteAsserter();
        }

        public FileWriteAsserter FileWriteAsserter { get; }

        [OneTimeTearDown]
        public void TearDown()
        {
            FileWriteAsserter.Dispose();
        }

        private static readonly string[] InvalidWindowsFileNames = ["invalid<filename", "invalid>filename", "invalid\"filename", "invalid/filename", "invalid\\filename", "invalid|filename", "invalid?filename", "invalid*filename",];
        private static readonly string[] ReservedWindowsFileNames = ["con", "CON", "PRN", "AUX", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM\u00B9", "COM\u00B2", "COM\u00B3", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "LPT\u00B9", "LPT\u00B2", "LPT\u00B3", "endingWithSpace ", "endingWithPeriod."];

        private static readonly string[] ReservedWindowsFileNamePrefixUsed = ["con.txt", "CON.txt", "PRN.txt", "AUX.txt", "NUL.txt", "COM0.txt", "COM1.txt", "COM2.txt", "COM3.txt", "COM4.txt", "COM5.txt", "COM6.txt", "COM7.txt", "COM8.txt", "COM9.txt", "COM\u00B9.txt", "COM\u00B2.txt", "COM\u00B3.txt", "LPT0.txt", "LPT1.txt", "LPT2.txt", "LPT3.txt", "LPT4.txt", "LPT5.txt", "LPT6.txt", "LPT7.txt", "LPT8.txt", "LPT9.txt", "LPT\u00B9.txt", "LPT\u00B2.txt", "LPT\u00B3.txt"];

        private static readonly string[] ValidFileNames = ["validFileName"];

        [Test]
        [TestCaseSource(nameof(ValidFileNames))]
        public void ShouldNotTouchASaneFilename(string validFilename)
        {
            var sanitizedFilename = validFilename.SanitizeFilename();

            Assert.That(sanitizedFilename, Is.EqualTo(validFilename));
        }

        [Test]
        [TestCase("CO*", 'N', "N")]
        public void ShouldSanitizeEdgeCase(string invalidFilename, char replacement, string expectedOutcome)
        {
            var sanitizedFilename = invalidFilename.SanitizeFilename(replacement);

            Assert.That(sanitizedFilename, Is.EqualTo(expectedOutcome));
        }

        [Test]
        [TestCase("CO*", '*')]
        public void ShouldThrow(string invalidFilename, char replacement)
        {
            var ex = Assert.Throws<ArgumentException>(() => invalidFilename.SanitizeFilename(replacement));

            if (IsRunningOnNet4x())
            {
                Assert.That(ex?.Message, Is.EqualTo("Replacement '*' is invalid for Windows\r\nParameter name: replacement"));
                return;
            }

            Assert.That(ex.Message, Is.EqualTo("Replacement '*' is invalid for Windows (Parameter 'replacement')"));
        }

        [Test]
        public void ShouldFallbackToHardCodedDefault()
        {
            var sanitizedFilename = "Invalid*Filename.txt".SanitizeFilename(string.Empty);

            Assert.That(sanitizedFilename, Is.EqualTo("InvalidFilename.txt"));
        }

        [Test]
        [TestCase("COM1", '.')]
        [TestCase("COM1", ' ')]
        public void ShouldFallbackToHardCodedDefault(string invalidFilename, char replacement)
        {
            var sanitizedFilename = invalidFilename.SanitizeFilename(replacement);

            Assert.That(sanitizedFilename, Is.EqualTo(SanitizeFilename.FallbackFileName));
        }

        [Test]
        [TestCase("COM1", ".")]
        [TestCase("COM1", " ")]
        [TestCase("COM1", "")]
        public void ShouldFallbackToHardCodedDefault(string invalidFilename, string replacement)
        {
            var sanitizedFilename = invalidFilename.SanitizeFilename(replacement);

            Assert.That(sanitizedFilename, Is.EqualTo(SanitizeFilename.FallbackFileName));
        }

        [Test]
        public void ShouldSanitizeInvalidWindowsFileNamesWithControlCharacters()
        {
            // add Characters whose integer representations are in the range from 1 through 31, except for alternate data streams where these characters are allowed. For more information about file streams, see File Streams

            for (int i = 0; i < 32; i++)
            {
                char aReservedChar = (char)i;

                var invalidFilename = "invalid" + new string(aReservedChar, 1) + "filename";

                var sanitizedFilename = invalidFilename.SanitizeFilename();

                Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
                Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
            }
        }

        // This test will run some minutes, depending on your system performance (6 min on my machine)
        [Test]
        public void ShouldIterateOverEveryUnicodeCodePoint()
        {
            var sanitizedFilenames = new List<(string, int)>();

            // Iterate every UTF16 value
            Parallel.For(0, 0x10FFFF + 1, i =>
            {
                // Skip surrogate pairs
                if (i >= 0xD800 && i <= 0xDFFF)
                    return;

                string unicodeString = char.ConvertFromUtf32(i);
                var mightBeValid = "valid" + unicodeString + "filename" + i;

                var sanitizedFilename = mightBeValid.SanitizeFilename();

                lock (sanitizedFilenames)
                {
                    sanitizedFilenames.Add((sanitizedFilename, i));
                }
            });

            FileWriteAsserter.AssertCollection(sanitizedFilenames);
        }

        [Test]
        [TestCaseSource(nameof(InvalidWindowsFileNames))]
        public void ShouldSanitizeInvalidWindowsFileNames(string invalidFilename)
        {
            var sanitizedFilename = invalidFilename.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
        }

        [Test]
        [TestCaseSource(nameof(ReservedWindowsFileNames))]
        public void ShouldSanitizeReservedWindowsFileNames(string invalidFilename)
        {
            var sanitizedFilename = invalidFilename.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
        }

        [Test]
        [TestCaseSource(nameof(ReservedWindowsFileNamePrefixUsed))]
        public void ShouldSanitizeReservedWindowsFileNamePrefix(string invalidFilename)
        {
            var sanitizedFilename = invalidFilename.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
        }

        [Test]
        [TestCase("👩🏽‍🚒", 0, "👩🏽‍🚒", 7)]
        [TestCase("a", 300, "a", 255)]
        // Fitting to ext4 using UTF-8 filenames
        // https://emojipedia.org/woman-firefighter-medium-skin-tone#technical
        [TestCase("👩🏽‍🚒", 240, "👩🏽‍🚒", 247)]
        [TestCase("👩🏽‍🚒", 241, "a", 241)]
        public void ShouldTruncateLongFileNamesPreserveUnicodeTextElements(string testSuffix, int countOfFillingAChars, string expectedEnd, int expectedSanitizedLength)
        {
            if (IsRunningOnNet4x())
            {
                Assert.Pass("Test is not thought to be run with .net framework / unicode 8");
            }

            var invalidFilename = new string('a', countOfFillingAChars) + testSuffix;
            var sanitizedFilename = invalidFilename.SanitizeFilename();
            Assert.That(sanitizedFilename, Does.EndWith(expectedEnd));
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
            Assert.That(sanitizedFilename, Has.Length.EqualTo(expectedSanitizedLength));
            Assert.That(System.Text.Encoding.UTF8.GetByteCount(sanitizedFilename), Is.LessThan(256));
        }

        // Unicode examples https://emojipedia.org/unicode-17.0
        [TestCase("💏🏻", 13, "__", "Unicode 13.1 example https://emojipedia.org/kiss-light-skin-tone but is an https://emojipedia.org/emoji-modifier-sequence combining unicode a codpage from v8 and v6 -> that is not touched by ")]
        [TestCase("", null, null, " Private Use Area (PUA) character that is supported on iOS and macOS https://emojipedia.org/apple-logo")]
        [TestCase("⛷️", 5, null, "Unicode 5.2  example https://emojipedia.org/skier")]
#pragma warning disable IDE0060 // unicodeVersionNote is used for documentation purposes only
        public void ShouldNotBeTouchedBySanitizer(string unicodeSpecificEmoticon, int? unicodeVersion, string? expectedNetFramework, string unicodeVersionNote)
#pragma warning restore IDE0060 // unicodeVersionNote is used for documentation purposes only
        {
            var expected = unicodeSpecificEmoticon;

            // https://learn.microsoft.com/en-us/dotnet/framework/whats-new/#character-categories .net framework 4.0 is stuck to Unicode 8
            if (unicodeVersion.HasValue && unicodeVersion > 8 && IsRunningOnNet4x())
            {
                expected = expectedNetFramework;
            }

            var sanitizedFilename = unicodeSpecificEmoticon.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.EqualTo(expected));
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
        }

        // https://learn.microsoft.com/en-us/dotnet/api/system.globalization.charunicodeinfo?view=net-8.0#notes-to-callers
        // Unicode examples https://emojipedia.org/unicode-17.0
        [TestCase("😀", "Unicode 6.1 example https://emojipedia.org/grinning-face")]
        [TestCase("🚴", "Unicode 6 example https://emojipedia.org/person-biking")]
        [TestCase("🙂", "Unicode 8 example https://emojipedia.org/person-biking")]
        [TestCase("🤩", "Unicode 10 example https://emojipedia.org/star-struck#emoji")]
        [TestCase("🥰", "Unicode 11 example https://emojipedia.org/smiling-face-with-hearts")]
        [TestCase("🦿", "Unicode 12 example https://emojipedia.org/mechanical-leg")]
        [TestCase("🫀", "Unicode 13.1 example https://emojipedia.org/anatomical-heart")]
        [TestCase("🫠", "Unicode 14 example https://emojipedia.org/melting-face")]
        [TestCase("🫥", "Unicode 14 example https://emojipedia.org/dotted-line-face")]
        [TestCase("🪿", "Unicode 15 example https://emojipedia.org/goose")]
#pragma warning disable IDE0060 // unicodeVersionNote is used for documentation purposes only
        public void ShouldSanitizeUnicodeVersion9Plus(string unicodeSpecificEmoticon, string unicodeVersion)
        {
            var sanitizedFilename = unicodeSpecificEmoticon.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(unicodeSpecificEmoticon));
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(unicodeSpecificEmoticon), Is.True);
        }

        // This emoticons are supported by every OS/FS tested, except macOS, because unicode 16 and 17 specific code points are not supported by macOS today
        // Behavior on macos is expected to change over time
        [TestCase("🫩", "Unicode 16 example https://emojipedia.org/face-with-bags-under-eyes")]
        [TestCase("🫝", "Unicdoe 17 example https://emojipedia.org/apple-core")]
        public void Unicode17SpecificMacoOsBehavior(string unicodeSpecificEmoticon, string unicodeVersion)
        {
            var sanitizedFilename = unicodeSpecificEmoticon.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(unicodeSpecificEmoticon));
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(unicodeSpecificEmoticon), Is.Not.EqualTo(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)));
        }
    }
#pragma warning restore IDE0060 // unicodeVersionNote is used for documentation purposes only
}