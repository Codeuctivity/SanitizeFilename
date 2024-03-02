using Codeuctivity;
using System.Runtime.InteropServices;

namespace SanitizeFilenameTests
{
    internal class LinuxSpecificTests
    {
        [Test]
        public void ShouldFailToWriteUnpairedSurrogateOnlyGithubRunner()
        {
            string unpairedSurrogate = "\uD800";
            var fileNameWithUnpairedSurrogate = unpairedSurrogate + "fileName.txt";
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(fileNameWithUnpairedSurrogate);
            Assert.That(actual, Is.Not.EqualTo(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)));
        }

        [Test]
        public void ShouldFailToWriteFilenameWithMoreThan255Bytes()
        {
            var fileNameWithUnpairedSurrogate = new string('a', 248) + "👩🏽‍🚒";
            var actual = FileWriteAsserter.TryWriteFileToTempDirectory(fileNameWithUnpairedSurrogate);
            Assert.That(actual, Is.Not.EqualTo(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)));
        }
    }
}