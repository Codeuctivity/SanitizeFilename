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

        [Test, TestCaseSource(typeof(SanitizeFilename), nameof(SanitizeFilename.InvalidCharsInMacOsFileNames))]
        public void ShouldSanitizeOsXSpecificInvalidChars(int i)
        {
            string fileNameOsXSpecificException = "filename" + (char)i;
            string sanitizedFileNameOsXSpecificException = fileNameOsXSpecificException.SanitizeFilename();
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFileNameOsXSpecificException);
            Assert.That(actual, $"Expected the fileNameOsXSpecificException {i:X4} to be sanitized and usable on any OS.");
            Assert.That(fileNameOsXSpecificException, Is.Not.EqualTo(sanitizedFileNameOsXSpecificException));
        }

        [Test, TestCaseSource(typeof(SanitizeFilename), nameof(SanitizeFilename.InvalidCharsInMacOsFileNames))]
        public void ShouldBehaviorOsDependentOnWritingFilenameWithKnownOsXSpecificExceptions(char invalidOnMacOs)
        {
            var expected = !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            var filenameInvalidOnMacOs = invalidOnMacOs + "Filename.txt";
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(filenameInvalidOnMacOs);
            Assert.That(actual, Is.EqualTo(expected));
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
        public void ShouldSanitizeNotAssignedCodepointWithSurrogates()
        {
            // https://unicodelookup.com/#423939/1  u+67803
            var oneOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs = 423939;
            var sanitizedFilenames = new List<(string, int)>();
            string unicodeString = char.ConvertFromUtf32(oneOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs);
            var mightBeValid = "valid" + unicodeString + "filename" + oneOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs;

            var sanitizedFilename = mightBeValid.SanitizeFilename();
            Assert.That(sanitizedFilename, Is.Not.EqualTo(mightBeValid));

            lock (sanitizedFilenames)
            {
                sanitizedFilenames.Add((sanitizedFilename, oneOfManyValuesFoundByRunningEveryPossibleUTF16ValueAgainstMacOs));
            }

            FileWriteAsserter.AssertCollection(sanitizedFilenames);
        }
    }
}