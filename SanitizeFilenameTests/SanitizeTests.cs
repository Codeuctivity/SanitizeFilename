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

        public static string[] InvalidWindowsFileNames = { "invalid<filename", "invalid>filename", "invalid\"filename", "invalid/filename", "invalid\\filename", "invalid|filename", "invalid?filename", "invalid*filename", };
        public static string[] ReservedWindowsFileNames = { "con", "CON", "PRN", "AUX", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM¹", "COM²", "COM³", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "LPT¹", "LPT²", "LPT³" };

        public static string[] ReservedWindowsFileNamePrefixUsed = { "con.txt", "CON.txt", "PRN.txt", "AUX.txt", "NUL.txt", "COM0.txt", "COM1.txt", "COM2.txt", "COM3.txt", "COM4.txt", "COM5.txt", "COM6.txt", "COM7.txt", "COM8.txt", "COM9.txt", "COM¹.txt", "COM².txt", "COM³.txt", "LPT0.txt", "LPT1.txt", "LPT2.txt", "LPT3.txt", "LPT4.txt", "LPT5.txt", "LPT6.txt", "LPT7.txt", "LPT8.txt", "LPT9.txt", "LPT¹.txt", "LPT².txt", "LPT³.txt" };

        [Test]
        public void ShouldSanitizeInvalidWindowsFileNamesWithControlCharacters()
        {
            // add Characters whose integer representations are in the range from 1 through 31, except for alternate data streams where these characters are allowed. For more information about file streams, see File Streams

            for (int i = 0; i < 32; i++)
            {
                char aReservedChar = (char)i;

                var invalidFilename = "invalid" + new string(aReservedChar, 1) + "filename";

                var sanitizedFilename = invalidFilename.Sanitize();

                Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
                Assert.That(TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
            }
        }

        [Test]
        [TestCaseSource(nameof(InvalidWindowsFileNames))]
        public void ShouldSanitizeInvalidWindowsFileNames(string invalidFilename)
        {
            var sanitizedFilename = invalidFilename.Sanitize();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
            Assert.That(TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
        }

        [Test]
        [TestCaseSource(nameof(ReservedWindowsFileNames))]
        public void ShouldSanitizeReservedWindowsFileNames(string invalidFilename)
        {
            var sanitizedFilename = invalidFilename.Sanitize();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
            Assert.That(TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
        }

        [Test]
        [TestCaseSource(nameof(ReservedWindowsFileNamePrefixUsed))]
        public void ShouldSanitizeReservedWindowsFileNamePrefix(string invalidFilename)
        {
            var sanitizedFilename = invalidFilename.Sanitize();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
            Assert.That(TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
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