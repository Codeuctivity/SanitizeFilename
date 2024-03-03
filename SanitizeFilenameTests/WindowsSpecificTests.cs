using Codeuctivity;
using System.Globalization;

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

        [Test, TestCaseSource(typeof(SanitizeFilename), nameof(SanitizeFilename.InvalidCharsInWindowsFileNames)), Platform("Win")]
        public void ShouldBehaviorOsDependentOnWritingFilenameWithKnownWindowsSpecificExceptions(char invalidOnWindows)
        {
            var filenameInvalidOnMacOs = "valid" + invalidOnWindows + "filename";
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(filenameInvalidOnMacOs);
            Assert.That(actual, Is.False);
        }

        [Test]
        public void ShouldProofThatThereAreOnlyKnownExceptionsInListOfInvalidUnicodeCodePoints()
        {
            foreach (var item in SanitizeFilename.InvalidCharsInWindowsFileNames)
            {
                if (item < 200)
                {
                    continue;
                }

                var category = CharUnicodeInfo.GetUnicodeCategory(item);

                if (item == (char)3315)
                {
                    Assert.That(category, Is.EqualTo(UnicodeCategory.SpacingCombiningMark));
                }
                else if (item == (char)3790)
                {
                    Assert.That(category, Is.EqualTo(UnicodeCategory.NonSpacingMark));
                }
                else
                {
                    Assert.That(category, Is.EqualTo(UnicodeCategory.OtherNotAssigned));
                }
            }
        }
    }
}