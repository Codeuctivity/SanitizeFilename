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

        public FileWriteAsserter FileWriteAsserter { get; }
    }
}