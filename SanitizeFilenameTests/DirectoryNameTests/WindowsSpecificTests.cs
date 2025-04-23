using Codeuctivity;

namespace DirectoryNameTests
{
    internal class WindowsSpecificTests
    {
        public WindowsSpecificTests()
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

        [Test, Platform("Win")]
        public void ShouldBehaviorOsDependentOnWritingFilenameWithKnownWindowsSpecificExceptions()
        {
            foreach (var invalidOnWindows in SanitizeFilename.InvalidCharsInWindowsFileNames)
            {
                var filenameInvalidOnMacOs = "valid" + invalidOnWindows + "filename";
                var actual = DirectoryWriteAsserter.TryCreateDirectoryToTempDirectory(filenameInvalidOnMacOs);
                Assert.That(actual, Is.False);
            }
        }
    }
}