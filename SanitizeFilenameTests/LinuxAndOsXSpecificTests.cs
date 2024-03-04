using Codeuctivity;
using System.Runtime.InteropServices;

namespace SanitizeFilenameTests
{
    internal class LinuxAndOsXSpecificTests
    {
        public LinuxAndOsXSpecificTests()
        {
            FileWriteAsserter = new FileWriteAsserter();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (Directory.Exists(FileWriteAsserter.TempPath))
                Directory.Delete(FileWriteAsserter.TempPath, true);
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

        [Test, TestCaseSource(nameof(HighSurrogateRange))]
        public void HighSurrogatesShouldFailToBeUsedAsFileNameOnLinuxOrOsX(int i)
        {
            var expected = !(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX));
            string fileNameWithUnpairedSurrogate = "filename" + (char)i;
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(fileNameWithUnpairedSurrogate);
            Assert.That(actual, Is.EqualTo(expected), $"Expected the high surrogate {i:X4} to be valid to use in filenames.");
        }
        public FileWriteAsserter FileWriteAsserter { get; }

        [Test, TestCaseSource(nameof(HighSurrogateRange))]
        public void HighSurrogatesShouldBeSanitized(int i)
        {
            string fileNameWithUnpairedSurrogate = "filename" + (char)i;
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(fileNameWithUnpairedSurrogate.SanitizeFilename());
            Assert.That(actual, $"Expected the high surrogate {i:X4} to be sanitized.");
        }

        [Test, TestCaseSource(nameof(LowSurrogateRange))]
        public void LowSurrogatesShouldFailToBeUsedAsFileNameOnLinux(int i)
        {
            var expected = !(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX));
            string fileNameWithUnpairedSurrogate = (char)i + "filename";
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(fileNameWithUnpairedSurrogate);
            Assert.That(actual, Is.EqualTo(expected), $"Expected the low surrogate {i:X4} to be valid to use in filenames.");
        }

        [Test, TestCaseSource(nameof(LowSurrogateRange))]
        public void LowSurrogatesShouldBeSanitized(int i)
        {
            string fileNameWithUnpairedSurrogate = (char)i + "filename";
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(fileNameWithUnpairedSurrogate.SanitizeFilename());
            Assert.That(actual, $"Expected the high surrogate {i:X4} to be sanitized.");
        }
    }
}