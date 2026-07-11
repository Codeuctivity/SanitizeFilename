using Codeuctivity;

namespace SanitizeFilenameTests.FilenameTests
{
    [Parallelizable(ParallelScope.Fixtures)]
    internal class TrunacateLongFilenames : SanitizeFilenamesTestsBase
    {
        public TrunacateLongFilenames()
        {
            FileWriteAsserter = new FileWriteAsserter();
        }

        public FileWriteAsserter FileWriteAsserter { get; }

        [OneTimeTearDown]
        public void TearDown()
        {
            FileWriteAsserter.Dispose();
        }

        [Test]
        [TestCase(true, "a.txt")]
        [TestCase(false, "aaaaaa")]
        public void ShouldTruncateFileExtensionSpecificBehaviour(bool preserveFileNameExtension, string exectedSanitizedFilenameEnd)
        {
            var filename = new string('a', 300);
            filename += ".txt";
            var sanitizedFilename = filename.SanitizeFilename(preserveFileNameExtension: preserveFileNameExtension);

            Assert.That(sanitizedFilename, Does.EndWith(exectedSanitizedFilenameEnd));
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
            Assert.That(System.Text.Encoding.UTF8.GetByteCount(sanitizedFilename), Is.LessThanOrEqualTo(255));
        }

        [Test]
        [TestCase(true, ".aaaaaa")]
        [TestCase(false, "file")]
        public void ShouldPreserveExtensionEvenWhenExceedingMaxLength(bool preserveFileNameExtension, string exectedSanitizedFilenameStart)
        {
            // Create a filename where even the extension alone exceeds the max length when combined with minimal filename
            var veryLongExtension = "." + new string('a', 300); // 251 bytes for extension alone
            var filename = "file" + veryLongExtension;
            var sanitizedFilename = filename.SanitizeFilename(preserveFileNameExtension: preserveFileNameExtension);

            // Should preserve the extension despite length constraints
            Assert.That(sanitizedFilename, Does.StartWith(exectedSanitizedFilenameStart));
            Assert.That(System.Text.Encoding.UTF8.GetByteCount(sanitizedFilename), Is.LessThanOrEqualTo(255));
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
        }

        [Test]
        [TestCase("shortname.txt")]
        [TestCase("another.doc")]
        [TestCase("file.json")]
        public void ShouldPreserveExtensionForShortFileNames(string filename)
        {
            var sanitizedFilename = filename.SanitizeFilename(preserveFileNameExtension: true);

            var extension = Path.GetExtension(filename);
            Assert.That(sanitizedFilename, Does.EndWith(extension));
            Assert.That(sanitizedFilename, Is.EqualTo(filename));
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
        }

        [Test]
        public void ShouldHandleFileWithoutExtension()
        {
            var filename = new string('a', 300); // No extension
            var sanitizedFilename = filename.SanitizeFilename(preserveFileNameExtension: true);

            Assert.That(System.Text.Encoding.UTF8.GetByteCount(sanitizedFilename), Is.LessThanOrEqualTo(255));
            Assert.That(FileWriteAsserter.TryWriteFileToTempDirectory(sanitizedFilename), Is.True);
        }
    }
}