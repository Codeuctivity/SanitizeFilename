using Codeuctivity;
using System.Runtime.InteropServices;

namespace SanitizeFilenameTests
{
    internal class WindowsExFatSpecificTests : SanitizeFilenamesTestsBase
    {
        public WindowsExFatSpecificTests()
        {
            FileWriteAsserter = new FileWriteAsserter(true);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            FileWriteAsserter.Dispose();
        }

        [Test]
        public void ShouldBehaviorSpecificOnExFat()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Ignore("CustomFsFileWriteAsserter is only applicable on Windows.");
            }

            // Skip if not running as administrator
            if (!IsRunningAsAdministrator())
            {
                Assert.Ignore("Test requires administrator privileges to create and mount exFAT VHD.");
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
                var actual = FileWriteAsserter.TryWriteFileToTempDirectory(filenameInvalidOnExFat);
                Assert.That(actual, Is.False,
                    $"Expected writing file with name '{filenameInvalidOnExFat}' to fail on exFAT, but it succeeded.");
                var sanitizedFilename = filenameInvalidOnExFat.SanitizeFilename();
                var actualSanitized = FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename);
                Assert.That(actualSanitized, Is.True,
                    $"Expected writing sanitized file with name '{sanitizedFilename}' to succeed on exFAT, but it failed.");
            }
        }

        private static bool IsRunningAsAdministrator()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return false;
            using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        public FileWriteAsserter FileWriteAsserter { get; }
    }
}