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
            FileWriteAsserter.Dispose();
        }

        public FileWriteAsserter FileWriteAsserter { get; }

        [Test, Platform("Win")]
        public void ShouldBehaviorOsDependentOnWritingFilenameWithKnownWindowsSpecificExceptions()
        {
            foreach (var invalidOnWindows in SanitizeFilename.InvalidCharsInWindowsFileNames)
            {
                var filenameInvalidOnWindows = "valid" + invalidOnWindows + "filename";
                var actual = FileWriteAsserter.TryWriteFileToTempDirectory(filenameInvalidOnWindows);
                Assert.That(actual, Is.False,
                    $"Expected writing file with name '{filenameInvalidOnWindows}' to fail on Windows, but it succeeded.");
            }
        }
    }
}