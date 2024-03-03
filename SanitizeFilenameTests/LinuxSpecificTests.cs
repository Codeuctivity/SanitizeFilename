using System.Runtime.InteropServices;

namespace SanitizeFilenameTests
{
    internal class LinuxSpecificTests
    {
        public LinuxSpecificTests()
        {
            FileWriteAsserter = new FileWriteAsserter();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (Directory.Exists(FileWriteAsserter.TempPath))
                Directory.Delete(FileWriteAsserter.TempPath, true);
        }

        [Test]
        public void ShouldBehaviorOsDependentOnWritingFilenameWithMoreThan255Bytes()
        {
            var expected = !RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var fileNameTooLongForLinux = new string('a', 248) + "👩🏽‍🚒";
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(fileNameTooLongForLinux);
            Assert.That(actual, Is.EqualTo(expected), "Filenames that exceed utf-8 255 byte length are expected to be valid on Windows and OsX (beyond 255 chars) and to be invalid on Linux. This expectation failed.");
        }

        public static IEnumerable<int> HighSurrogateRange
        {
            get
            {
                for (int i = 0xD800; i <= 0xDBFF; i++)
                {
                    yield return i;
                }
            }
        }

        [Test, TestCaseSource(nameof(HighSurrogateRange))]
        public void TestHighSurrogates(int i)
        {
            string fileNameWithUnpairedSurrogate = "filename" + (char)i;
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(fileNameWithUnpairedSurrogate);
            Assert.That(actual, Is.True, $"Expected the high surrogate {i:X4} to be valid to use in filenames.");
        }

        public static IEnumerable<int> LowSurrogateRange
        {
            get
            {
                for (int i = 0xDC00; i <= 0xDFFF; i++)
                {
                    yield return i;
                }
            }
        }

        public FileWriteAsserter FileWriteAsserter { get; }

        [Test, TestCaseSource(nameof(LowSurrogateRange))]
        public void TestLowSurrogates(int i)
        {
            string fileNameWithUnpairedSurrogate = "filename" + (char)i;
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(fileNameWithUnpairedSurrogate);
            Assert.That(actual, Is.True, $"Expected the low surrogate {i:X4} to be valid to use in filenames.");
        }
    }
}