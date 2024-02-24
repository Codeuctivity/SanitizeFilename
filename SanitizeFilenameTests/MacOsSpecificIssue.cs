using Codeuctivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SanitizeFilenameTests
{
    internal class MacOsSpecificIssue
    {
        [Test]
        public void ReproducesIssueThatOnlyOccuresRunningOnMacos()
        {
            // All values listed in InvalidCharsInMacOsFileNames trigger an exception when trying to create a file with that name on MacOS
            // This test is a minimal example to reproduce the issue using the character '줰' (U+C930) which trigger an exception in MacOS

            var filenameThatCantBeWrittonOnMacOs = "filename줰.txt";
            File.WriteAllText(filenameThatCantBeWrittonOnMacOs, "content");

            char aValidChar = (char)930;
            var valid = "valid" + new string(aValidChar, 1) + "filename";
            File.WriteAllText(filenameThatCantBeWrittonOnMacOs, "content");

            Assert.Pass("This test will only fail on MacOS");
        }
    }
}
