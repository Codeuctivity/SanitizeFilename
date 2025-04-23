using Codeuctivity;
using DirectoryNameTests;
using System.Runtime.InteropServices;

namespace SanitizeFilenameTests
{
    internal class LinuxAndOsXSpecificTests
    {
        public LinuxAndOsXSpecificTests()
        {
            DirectoryWriteAsserter = new DirectoryWriteAsserter();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (Directory.Exists(DirectoryWriteAsserter.TempPath))
                Directory.Delete(DirectoryWriteAsserter.TempPath, true);
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

        public DirectoryWriteAsserter DirectoryWriteAsserter { get; }

        [Test, TestCaseSource(nameof(HighSurrogateRange))]
        public void HighSurrogatesShouldFailToBeUsedAsFileNameOnLinuxAndOsX(int i)
        {
            var expected = !(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX));
            string fileNameWithUnpairedSurrogate = "filename" + (char)i;
            var actual = DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(fileNameWithUnpairedSurrogate);
            Assert.That(actual, Is.EqualTo(expected), $"Expected the unpaired high surrogate {i:X4} to be valid to use in filenames.");
        }

        [Test, TestCaseSource(nameof(HighSurrogateRange))]
        public void HighSurrogatesShouldBeSanitized(int i)
        {
            string fileNameWithUnpairedSurrogate = "filename" + (char)i;
            string sanitizedFilenameWithoutUnpairedSurrogate = fileNameWithUnpairedSurrogate.SanitizeFilename();
            var actual = DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(sanitizedFilenameWithoutUnpairedSurrogate);
            Assert.That(actual, $"Expected the unpaired high surrogate {i:X4} to be sanitized and usable on any OS.");
            Assert.That(fileNameWithUnpairedSurrogate, Is.Not.EqualTo(sanitizedFilenameWithoutUnpairedSurrogate));
        }

        [Test, TestCaseSource(nameof(LowSurrogateRange))]
        public void LowSurrogatesShouldFailToBeUsedAsFileNameOnLinuxAndOsX(int i)
        {
            var expected = !(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX));
            string fileNameWithUnpairedSurrogate = (char)i + "filename";
            var actual = DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(fileNameWithUnpairedSurrogate);
            Assert.That(actual, Is.EqualTo(expected), $"Expected the low unpaired surrogate {i:X4} to be valid to use in filenames.");
        }

        [Test, TestCaseSource(nameof(LowSurrogateRange))]
        public void LowSurrogatesShouldBeSanitized(int i)
        {
            string fileNameWithUnpairedSurrogate = (char)i + "filename";
            string sanitizedFilenameWithoutUnpairedSurrogate = fileNameWithUnpairedSurrogate.SanitizeFilename();
            var actual = DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(sanitizedFilenameWithoutUnpairedSurrogate);
            Assert.That(actual, $"Expected the unpaired low surrogate {i:X4} to be sanitized and usable on any OS.");
            Assert.That(fileNameWithUnpairedSurrogate, Is.Not.EqualTo(sanitizedFilenameWithoutUnpairedSurrogate));
        }
    }
}