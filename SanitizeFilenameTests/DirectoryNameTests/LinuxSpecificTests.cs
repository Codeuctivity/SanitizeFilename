using SanitizeFilenameTests;
using System.Runtime.InteropServices;

namespace DirectoryNameTests
{
    internal class LinuxSpecificTests : SanitizeFilenamesTestsBase
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
        public void ShouldBehaviorOsDependentOnCreatingDirectoryWithMoreThan255Bytes()
        {
            if (IsRunningOnNet4x())
            {
                Assert.Pass("Test is not thought to be run with .net framwework / unicode 8");
            }

            var expected = !RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var directoryNameTooLongForLinux = new string('a', 248) + "👩🏽‍🚒";
            var actual = DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(directoryNameTooLongForLinux);
            Assert.That(actual, Is.EqualTo(expected), "Filenames that exceed utf-8 255 byte length are expected to be valid on Windows and OsX (beyond 255 chars) and to be invalid on Linux. This expectation failed.");
        }

        public DirectoryWriteAsserter DirectoryWriteAsserter { get; }
    }
}