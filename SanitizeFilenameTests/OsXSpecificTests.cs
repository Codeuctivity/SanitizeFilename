using Codeuctivity;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SanitizeFilenameTests
{
    internal class OsXSpecificTests
    {
        public OsXSpecificTests()
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

        [Test, TestCaseSource(typeof(SanitizeFilename), nameof(SanitizeFilename.InvalidCodePointInOsXFileNames))]
        public void ShouldSanitizeOsXSpecificInvalidChars(int i)
        {
            string fileNameOsXSpecificException = "filename" + char.ConvertFromUtf32(i);
            string sanitizedFileNameOsXSpecificException = fileNameOsXSpecificException.SanitizeFilename();
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFileNameOsXSpecificException);
            Assert.That(actual, $"Expected the fileNameOsXSpecificException {i:X4} to be sanitized and usable on any OS.");
            Assert.That(fileNameOsXSpecificException, Is.Not.EqualTo(sanitizedFileNameOsXSpecificException));
        }

        [Test]
        // https://codepoints.net/U+11F02?lang=en
        [TestCase(73474)]
        [TestCase(69375)]
        public void ShouldSanitizeOsXSpecificInvalidChar(int unicodeOnlyFailingOnOsX)
        {
            string fileNameOsXSpecificException = "filename" + char.ConvertFromUtf32(unicodeOnlyFailingOnOsX);
            string sanitizedFileNameOsXSpecificException = fileNameOsXSpecificException.SanitizeFilename();
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFileNameOsXSpecificException);
            Assert.That(actual);
            Assert.That(fileNameOsXSpecificException, Is.Not.EqualTo(sanitizedFileNameOsXSpecificException));
        }

        [Test, TestCaseSource(typeof(SanitizeFilename), nameof(SanitizeFilename.InvalidCodePointInOsXFileNames))]
        public void ShouldBehaviorOsDependentOnWritingFilenameWithKnownOsXSpecificExceptions(int invalidOnMacOs)
        {
            var expected = !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            var filenameInvalidOnMacOs = char.ConvertFromUtf32(invalidOnMacOs) + "Filename.txt";
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(filenameInvalidOnMacOs);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldProofThatThereAreOnlyKnownExceptionsInListOfInvalidUnicodeCodePoints()
        {
            foreach (var item in SanitizeFilename.InvalidCodePointInOsXFileNames)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(item);

                if (item == 3315)
                {
                    Assert.That(category, Is.EqualTo(UnicodeCategory.SpacingCombiningMark));
                }
                else if (item == 3790)
                {
                    Assert.That(category, Is.EqualTo(UnicodeCategory.NonSpacingMark));
                }
                else if (item == 73474)
                {
                    Assert.That(category, Is.EqualTo(UnicodeCategory.OtherLetter));
                }
                else
                {
                    Assert.That(category, Is.EqualTo(UnicodeCategory.OtherNotAssigned));
                }
            }
        }

        [Test]
        public void MacOsDoesNotSupportToWriteNotAssignedCodepointsWithSurrogates()
        {
            // https://unicodelookup.com/#423939/1
            var onOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs = 423939;
            string unicodeString = char.ConvertFromUtf32(onOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs);
            var expected = !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            var filenameInvalidOnMacOs = "valid" + unicodeString + "filename" + onOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs;
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(filenameInvalidOnMacOs);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ShouldSanitizeValidSurrogatesWithoutFollowingCodepoint()
        {
            // https://unicodelookup.com/#557056/1  
            var oneOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs = 557056;
            var sanitizedFilenames = new List<(string, int)>();
            string unicodeString = char.ConvertFromUtf32(oneOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs);
            var expected = !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            var filenameInvalidOnMacOs = "valid" + unicodeString + "filename" + oneOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs;
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(filenameInvalidOnMacOs);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}