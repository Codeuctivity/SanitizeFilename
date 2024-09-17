using System.Runtime.InteropServices;

namespace DirectoryNameTests
{
    internal class LinuxSpecificTests
    {
        public LinuxSpecificTests()
        {
            DirectoryWriteAsserter = new DirectoryWriteAsserter();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (Directory.Exists(DirectoryWriteAsserter.TempPath))
                Directory.Delete(DirectoryWriteAsserter.TempPath, true);
        }

        [Test]
        public void ShouldBehaviorOsDependentOnWritingFilenameWithMoreThan255Bytes()
        {
            var expected = !RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var fileNameTooLongForLinux = new string('a', 248) + "👩🏽‍🚒";
            var actual = DirectoryWriteAsserter.TryWriteDirectoryToTempDirectory(fileNameTooLongForLinux);
            Assert.That(actual, Is.EqualTo(expected), "Filenames that exceed utf-8 255 byte length are expected to be valid on Windows and OsX (beyond 255 chars) and to be invalid on Linux. This expectation failed.");
        }

        public DirectoryWriteAsserter DirectoryWriteAsserter { get; }
    }
}