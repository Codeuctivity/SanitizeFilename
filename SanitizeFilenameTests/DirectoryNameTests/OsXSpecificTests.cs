using Codeuctivity;
using System.Runtime.InteropServices;

namespace DirectoryNameTests
{
    internal class OsXSpecificTests
    {
        public OsXSpecificTests()
        {
            DirectoryWriteAsserter = new DirectoryWriteAsserter();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (Directory.Exists(DirectoryWriteAsserter.TempPath))
                Directory.Delete(DirectoryWriteAsserter.TempPath, true);
        }

        public DirectoryWriteAsserter DirectoryWriteAsserter { get; }

        [Test]
        // https://codepoints.net/U+11F02?lang=en
        [TestCase(73474)]
        [TestCase(69375)]
        public void ShouldSanitizeOsXSpecificInvalidChar(int unicodeOnlyFailingOnOsX)
        {
            string fileNameOsXSpecificException = "filename" + char.ConvertFromUtf32(unicodeOnlyFailingOnOsX);
            string sanitizedFileNameOsXSpecificException = fileNameOsXSpecificException.SanitizeFilename();
            var actual = DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(sanitizedFileNameOsXSpecificException);
            Assert.That(actual);
            Assert.That(fileNameOsXSpecificException, Is.Not.EqualTo(sanitizedFileNameOsXSpecificException));
        }

        [Test]
        public void MacOsDoesNotSupportToWriteNotAssignedCodepointsWithSurrogates()
        {
            // https://unicodelookup.com/#423939/1
            var onOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs = 423939;
            string unicodeString = char.ConvertFromUtf32(onOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs);
            var expected = !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            var filenameInvalidOnMacOs = "valid" + unicodeString + "filename" + onOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs;
            var actual = DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(filenameInvalidOnMacOs);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldSanitizeValidSurrogatesWithoutFollowingCodepoint()
        {
            // https://unicodelookup.com/#557056/1
            var oneOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs = 557056;
            var sanitizedFilenames = new List<(string, int)>();
            string unicodeString = char.ConvertFromUtf32(oneOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs);
            var expected = !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            var filenameInvalidOnMacOs = "valid" + unicodeString + "filename" + oneOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs;
            var actual = DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(filenameInvalidOnMacOs);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        // U+0ECE Lao Yamakkan https://codepoints.net/U+00ECE
        [TestCase(3790)]
        // U+0CF3 Kannada Sign Combining Anusvara Above Right https://codepoints.net/U+00CF3
        [TestCase(3315)]
        // U+11F02 Kawi Sign Repha https://codepoints.net/U+11F02
        [TestCase(73474)]
        public void MacOsSupportToWriteCodePointsThatFailedOnOsXGibhutRunnersInBeginOf2024(int bogusOsXValue)
        {
            // https://unicodelookup.com/#423939/1
            var filenameInvalidOnMacOs = char.ConvertFromUtf32(bogusOsXValue) + "Filename.txt";
            var actual = DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(filenameInvalidOnMacOs);
            Assert.That(actual, Is.True);
        }
    }
}