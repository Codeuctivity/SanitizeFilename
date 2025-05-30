using Codeuctivity;

namespace SanitizeFilenameTests
{
    internal class WindowsSpecificTests
    {
        public WindowsSpecificTests()
        {
            FileWriteAsserter = new FileWriteAsserter();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (Directory.Exists(FileWriteAsserter.TempPath))
                Directory.Delete(FileWriteAsserter.TempPath, true);
        }

        public FileWriteAsserter FileWriteAsserter { get; }

        [Test]
        public void ShouldBehaviorOsDependentOnWritingFilenameWithKnownWindowsSpecificExceptions()
        {
#if !WINDOWS
            Assert.Ignore("Test is only applicable on Windows.");
#endif
            foreach (var invalidOnWindows in SanitizeFilename.InvalidCharsInWindowsFileNames)
            {
                var filenameInvalidOnWindows = "valid" + invalidOnWindows + "filename";
                var actual = FileWriteAsserter.TryWriteFileToTempDirectory(filenameInvalidOnWindows);
                Assert.That(actual, Is.False);
            }
        }
    }
}