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

        [Test, TestCaseSource(typeof(SanitizeFilename), nameof(SanitizeFilename.InvalidCharsInWindowsFileNames)), Platform("Win")]
        public void ShouldBehaviorOsDependentOnWritingFilenameWithKnownWindowsSpecificExceptions(char invalidOnWindows)
        {
            var filenameInvalidOnMacOs = "valid" + invalidOnWindows + "filename";
            var actual = DirectoryWriteAsserter.TryWriteDirectoryToTempDirectory(filenameInvalidOnMacOs);
            Assert.That(actual, Is.False);
        }
    }
}