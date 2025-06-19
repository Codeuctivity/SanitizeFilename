using Codeuctivity;
using SanitizeFilenameTests.ExFatTooling;

namespace SanitizeFilenameTests
{
    internal class WindowsExFatSpecificTests : SanitizeFilenamesTestsBase
    {
        public string GetOrCreateExFatPartitionFailReason { get; set; }
        private FileWriteAsserter? ExFatFileWriteAsserter { get; set; }

        [OneTimeSetUp]
        public void SetUp()
        {
            ExFatFileWriteAsserter = ExFatFileWriteAsserterFactory.TryGetOrCreateExFatPartition(out var reason);
            GetOrCreateExFatPartitionFailReason = reason;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            ExFatFileWriteAsserter?.Dispose();
        }

        [Test, Platform("Win")]
        public void ShouldBehaviorSpecificOnExFat()
        {
            if (ExFatFileWriteAsserter == null)
            {
                Assert.Ignore(GetOrCreateExFatPartitionFailReason);
            }

            //https://learn.microsoft.com/en-us/windows/win32/fileio/exfat-specification#table-35-invalid-filename-characters
            var invalidChars = new[] {
                 '\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
                 '\u0008', '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u000E', '\u000F',
                 '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017',
                 '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F',
                 '\"', '*', '/', ':', '<', '>', '?', '\\', '|'
             };

            foreach (var invalidOnExFat in invalidChars)
            {
                var filenameInvalidOnExFat = "valid" + invalidOnExFat + "filename";
                var actual = ExFatFileWriteAsserter.TryWriteFileToTempDirectory(filenameInvalidOnExFat);
                Assert.That(actual, Is.False,
                    $"Expected writing file with name '{filenameInvalidOnExFat}' to fail on exFAT, but it succeeded.");
                var sanitizedFilename = filenameInvalidOnExFat.SanitizeFilename();
                var actualSanitized = ExFatFileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename);
                Assert.That(actualSanitized, Is.True,
                    $"Expected writing sanitized file with name '{sanitizedFilename}' to succeed on exFAT, but it failed.");
            }
        }
    }
}