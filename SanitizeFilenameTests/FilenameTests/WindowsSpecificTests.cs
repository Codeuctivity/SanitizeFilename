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

        [Test, Platform("Win")]
        public void ShouldBehaviorOsDependentOnWritingFilenameWithKnownWindowsSpecificExceptions()
        {
            foreach (var invalidOnWindows in SanitizeFilename.InvalidCharsInWindowsFileNames)
            {
                var filenameInvalidOnMacOs = "valid" + invalidOnWindows + "filename";
                var actual = FileWriteAsserter.TryWriteFileToTempDirectory(filenameInvalidOnMacOs);
                Assert.That(actual, Is.False);
            }
        }
    }
}