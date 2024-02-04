using Codeuctivity;

namespace SanitizeFilenameTests
{
    public class Tests
    {
        private string _tempPath;

        [SetUp]
        public void Setup()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), "test");
            if (!Directory.Exists(_tempPath))
                Directory.CreateDirectory(_tempPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempPath))
                Directory.Delete(_tempPath, true);
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

            Assert.That(ex.Message, Is.EqualTo("Replacement char '*' is invalid for Windows file names (Parameter 'replacement')"));
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
        public void ShouldSanitizeInvalidWindowsFileNamesWithControlCharacters()
        {
            // add Characters whose integer representations are in the range from 1 through 31, except for alternate data streams where these characters are allowed. For more information about file streams, see File Streams

            for (int i = 0; i < 32; i++)
            {
                char aReservedChar = (char)i;

                var invalidFilename = "invalid" + new string(aReservedChar, 1) + "filename";

                var sanitizedFilename = invalidFilename.SanitizeFilename();

                Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
                Assert.That(TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
            }
        }

        [Test]
        [TestCaseSource(nameof(InvalidWindowsFileNames))]
        public void ShouldSanitizeInvalidWindowsFileNames(string invalidFilename)
        {
            var sanitizedFilename = invalidFilename.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
            Assert.That(TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
        }

        [Test]
        [TestCaseSource(nameof(ReservedWindowsFileNames))]
        public void ShouldSanitizeReservedWindowsFileNames(string invalidFilename)
        {
            var sanitizedFilename = invalidFilename.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
            Assert.That(TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
        }

        [Test]
        [TestCaseSource(nameof(ReservedWindowsFileNamePrefixUsed))]
        public void ShouldSanitizeReservedWindowsFileNamePrefix(string invalidFilename)
        {
            var sanitizedFilename = invalidFilename.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
            Assert.That(TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
        }

        [Test]
        [TestCase("a", 300, "a", 255)]
        // Not compatible to ext4 but works on NTFS
        //[TestCase("👩🏽‍🚒", 248, "👩🏽‍🚒", 255)]
        // Fitting to ext4 using UTF-8 filenames
        [TestCase("👩🏽‍🚒", 240, "👩🏽‍🚒", 247)]
        [TestCase("👩🏽‍🚒", 241, "a", 241)]
        public void ShouldTruncateLongFileNamesPreserveUnicodeTextElements(string testSuffix, int countOfFillingAChars, string expectedEnd, int expectedSanitizedLength)
        {
            var invalidFilename = new string('a', countOfFillingAChars) + testSuffix;
            var sanitizedFilename = invalidFilename.SanitizeFilename();
            Assert.That(sanitizedFilename, Does.EndWith(expectedEnd));
            Assert.That(TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
            Assert.That(sanitizedFilename, Has.Length.EqualTo(expectedSanitizedLength));
            Assert.That(System.Text.Encoding.UTF8.GetByteCount(sanitizedFilename), Is.LessThan(256));
        }

        private bool TryWriteFileToTempDirectory(string sanitizedFilename)
        {
            try
            {
                var path = Path.Combine(_tempPath, sanitizedFilename);
                File.WriteAllText(path, "testFileContent");
                if (!File.Exists(path))
                    return false;

                // check if file is in directory, File.WriteAllText will implicitly sanitize some filenames, e.g. "invalid:filename" -> "invalid"
                var listOfFileNames = Directory.GetFiles(_tempPath);

                if (!listOfFileNames.Contains(path))
                    return false;

                File.Delete(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}