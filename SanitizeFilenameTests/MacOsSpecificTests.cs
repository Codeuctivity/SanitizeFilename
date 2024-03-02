using Codeuctivity;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SanitizeFilenameTests
{
    internal class MacOsSpecificTests
    {
        [Test]
        public void InvalidCharsInMacOsFilenameShouldFailToWriteFile()
        {
            foreach (var invalidOnMacOs in SanitizeFilename.InvalidCharsInMacOsFileNames)
            {
                var filenameInvalidOnMacOs = invalidOnMacOs + "Filename.txt";
                var actual = FileWriteAsserter.TryWriteFileToTempDirectory(filenameInvalidOnMacOs);
                Assert.That(actual, Is.Not.EqualTo(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)));
            }
        }

        [Test]
        public void ShouldProofThatThereAreOnlyKnownExceptionsInListOfInvalidUnicodeCodePoints()
        {
            foreach (var item in SanitizeFilename.InvalidCharsInMacOsFileNames)
            {
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


