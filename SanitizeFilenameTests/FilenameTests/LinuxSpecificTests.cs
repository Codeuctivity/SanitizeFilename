using SanitizeFilenameTests.ExFatTooling;
using System.Runtime.InteropServices;

namespace SanitizeFilenameTests
{
    internal class LinuxSpecificTests : SanitizeFilenamesTestsBase
    {
        public string GetOrCreateExFatPartitionFailReason { get; set; }
        private FileWriteAsserter? ExFatFileWriteAsserter { get; set; }

        public LinuxSpecificTests()
        {
            FileWriteAsserter = new FileWriteAsserter();
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            ExFatFileWriteAsserter = ExFatTooling.ExFatFileWriteAsserter.TryGetOrCreateExFatPartition(out var reason);
            GetOrCreateExFatPartitionFailReason = reason;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            FileWriteAsserter.Dispose();
            ExFatFileWriteAsserter?.Dispose();
        }

        [Test]
        public void ShouldBehaviorOsDependentOnWritingFilenameWithMoreThan255Bytes()
        {
            if (IsRunningOnNet4x())
            {
                Assert.Pass("Test is not thought to be run with .net framework / unicode 8");
            }

            var expected = !RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var fileNameTooLongForLinux = new string('a', 248) + "👩🏽‍🚒";
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(fileNameTooLongForLinux);
            Assert.That(actual, Is.EqualTo(expected), "Filenames that exceed utf-8 255 byte length are expected to be valid on Windows and OsX (beyond 255 chars) and to be invalid on Linux. This expectation failed.");

            ExFatTooling.ExFatFileWriteAsserter.Assert(expected, ExFatFileWriteAsserter, fileNameTooLongForLinux, GetOrCreateExFatPartitionFailReason, "Filenames that exceed utf-8 255 byte length are expected to be valid on Windows and OsX (beyond 255 chars) and to be invalid on Linux. This expectation failed using exFat.");
        }

        public FileWriteAsserter FileWriteAsserter { get; }
    }
}