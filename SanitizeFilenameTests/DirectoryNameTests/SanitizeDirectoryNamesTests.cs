using Codeuctivity;
using SanitizeFilenameTests;

namespace DirectoryNameTests
{
    [Parallelizable(ParallelScope.Fixtures)]
    public class SanitizeDirectoryNamesTests : SanitizeFilenamesTestsBase
    {
        public SanitizeDirectoryNamesTests()
        {
            DirectoryWriteAsserter = new DirectoryWriteAsserter();
        }

        public DirectoryWriteAsserter DirectoryWriteAsserter { get; }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (Directory.Exists(DirectoryWriteAsserter.TempPath))
                Directory.Delete(DirectoryWriteAsserter.TempPath, true);
        }

        private static readonly string[] InvalidWindowsDirectoryNames = ["invalid<directoryName", "invalid>directoryName", "invalid\"directoryName", "invalid/directoryName", "invalid\\directoryName", "invalid|directoryName", "invalid?directoryName", "invalid*directoryName",];
        private static readonly string[] ReservedWindowsDirectoryNames = ["con", "CON", "PRN", "AUX", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM\u00B9", "COM\u00B2", "COM\u00B3", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "LPT\u00B9", "LPT\u00B2", "LPT\u00B3", "endingWithSpace ", "endingWithPeriod."];

        private static readonly string[] ReservedWindowsDirectoryNamePrefixUsed = ["con.txt", "CON.txt", "PRN.txt", "AUX.txt", "NUL.txt", "COM0.txt", "COM1.txt", "COM2.txt", "COM3.txt", "COM4.txt", "COM5.txt", "COM6.txt", "COM7.txt", "COM8.txt", "COM9.txt", "COM\u00B9.txt", "COM\u00B2.txt", "COM\u00B3.txt", "LPT0.txt", "LPT1.txt", "LPT2.txt", "LPT3.txt", "LPT4.txt", "LPT5.txt", "LPT6.txt", "LPT7.txt", "LPT8.txt", "LPT9.txt", "LPT\u00B9.txt", "LPT\u00B2.txt", "LPT\u00B3.txt"];

        private static readonly string[] ValidDirectoryNames = ["validDirectoryName"];

        [Test]
        [TestCaseSource(nameof(ValidDirectoryNames))]
        public void ShouldNotTouchASaneDirectoryName(string validDirecotryName)
        {
            var sanitizedFilename = validDirecotryName.SanitizeFilename();

            Assert.That(sanitizedFilename, Is.EqualTo(validDirecotryName));
        }

        [Test]
        [TestCase("CO*", 'N', "N")]
        public void ShouldSanitizeEdgeCase(string invalidDirectoryName, char replacement, string expectedOutcome)
        {
            var sanitizedFilename = invalidDirectoryName.SanitizeFilename(replacement);

            Assert.That(sanitizedFilename, Is.EqualTo(expectedOutcome));
        }

        [Test]
        [TestCase("CO*", '*')]
        public void ShouldThrow(string invalidDirectoryName, char replacement)
        {
            var ex = Assert.Throws<ArgumentException>(() => invalidDirectoryName.SanitizeFilename(replacement));

            if (IsRunningOnNet4x())
            {
                Assert.Pass("Test is not thought to be run with .net framwework / unicode 8");
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
        public void ShouldFallbackToHardCodedDefault(string invalidDirectoryName, char replacement)
        {
            var sanitizedFilename = invalidDirectoryName.SanitizeFilename(replacement);

            Assert.That(sanitizedFilename, Is.EqualTo(SanitizeFilename.FallbackFileName));
        }

        [Test]
        [TestCase("COM1", ".")]
        [TestCase("COM1", " ")]
        [TestCase("COM1", "")]
        public void ShouldFallbackToHardCodedDefault(string invalidDirectoryName, string replacement)
        {
            var sanitizedFilename = invalidDirectoryName.SanitizeFilename(replacement);

            Assert.That(sanitizedFilename, Is.EqualTo(SanitizeFilename.FallbackFileName));
        }

        [Test]
        public void ShouldSanitizeInvalidWindowsFileNamesWithControlCharacters()
        {
            // add Characters whose integer representations are in the range from 1 through 31, except for alternate data streams where these characters are allowed. For more information about file streams, see File Streams

            for (var i = 0; i < 32; i++)
            {
                var aReservedChar = (char)i;

                var invalidDirectoryName = "invalid" + new string(aReservedChar, 1) + "directoryName";

                var sanitizedFilename = invalidDirectoryName.SanitizeFilename();

                Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidDirectoryName));
                Assert.That(DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(sanitizedFilename), Is.True);
            }
        }

        // This test will run some minutes, depending on your system performance (6 min on my machine)
        [Test]
        public void ShouldIterateOverEveryUnicodeCodePoint()
        {
            var sanitizedDirectoryNames = new List<(string, int)>();

            // Iterate every UTF16 value
            Parallel.For(0, 0x10FFFF + 1, i =>
            {
                // Skip surrogate pairs
                if (i >= 0xD800 && i <= 0xDFFF)
                    return;

                var unicodeString = char.ConvertFromUtf32(i);
                var mightBeValid = "valid" + unicodeString + "directory" + i;

                var sanitizedDirectoryName = mightBeValid.SanitizeFilename();

                lock (sanitizedDirectoryNames)
                {
                    sanitizedDirectoryNames.Add((sanitizedDirectoryName, i));
                }
            });

            DirectoryWriteAsserter.AssertCollection(sanitizedDirectoryNames);
        }

        [Test]
        [TestCaseSource(nameof(InvalidWindowsDirectoryNames))]
        public void ShouldSanitizeInvalidWindowsFileNames(string invalidDirectoryName)
        {
            var sanitizedFilename = invalidDirectoryName.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidDirectoryName));
            Assert.That(DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(sanitizedFilename), Is.True);
        }

        [Test]
        [TestCaseSource(nameof(ReservedWindowsDirectoryNames))]
        public void ShouldSanitizeReservedWindowsFileNames(string invalidDirectoryName)
        {
            var sanitizedFilename = invalidDirectoryName.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidDirectoryName));
            Assert.That(DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(sanitizedFilename), Is.True);
        }

        [Test]
        [TestCaseSource(nameof(ReservedWindowsDirectoryNamePrefixUsed))]
        public void ShouldSanitizeReservedWindowsFileNamePrefix(string invalidDirectoryName)
        {
            var sanitizedFilename = invalidDirectoryName.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidDirectoryName));
            Assert.That(DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(sanitizedFilename), Is.True);
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
                Assert.Pass("Test is not thought to be run with .net framwework / unicode 8");
            }

            var invalidDirectoryName = new string('a', countOfFillingAChars) + testSuffix;
            var sanitizedDirectoryName = invalidDirectoryName.SanitizeFilename();
            Assert.That(sanitizedDirectoryName, Does.EndWith(expectedEnd));
            Assert.That(DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(sanitizedDirectoryName), Is.True);
            Assert.That(sanitizedDirectoryName, Has.Length.EqualTo(expectedSanitizedLength));
            Assert.That(System.Text.Encoding.UTF8.GetByteCount(sanitizedDirectoryName), Is.LessThan(256));
        }
    }
}