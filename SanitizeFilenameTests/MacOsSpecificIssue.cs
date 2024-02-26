using Codeuctivity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Unicode;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SanitizeFilenameTests
{
    internal class MacOsSpecificIssue
    {
        [Test]
        public void ExperimentWithUnicodeException()
        {
            foreach (var item in SanitizeFilename.InvalidCharsInMacOsFileNames)
            {
                var charInfo = UnicodeInfo.GetCharInfo(item);
                Console.WriteLine(UnicodeInfo.GetDisplayText(charInfo));
                Console.WriteLine(charInfo.Name ?? charInfo.OldName);
                Console.WriteLine(charInfo.Category);

                Assert.That(charInfo.Category, Is.EqualTo(UnicodeCategory.OtherNotAssigned));
            }
        }

        [Test]
        public void ReproducesIssueThatOnlyOccuresRunningOnMacosNormalize()
        {
            // All values listed in InvalidCharsInMacOsFileNames trigger an exception when trying to create a file with that name on MacOS
            // This test is a minimal example to reproduce the issue using the character '줰' (U+C930) which trigger an exception in MacOS

            var code = 51504;
            var surrogate = char.ConvertFromUtf32(code);

            // fully decompose valid, https://developer.apple.com/library/archive/technotes/tn/tn1150.html#UnicodeSubtleties indicates that the file system uses fully decomposed unicode
            var decomposed = surrogate.Normalize(NormalizationForm.FormD) ?? string.Empty;
            var valid = $"filename{decomposed}.txt";

            File.WriteAllText(valid, "content");
        }

        [Test]
        public void ReproducesIssueThatOnlyOccuresRunningOnMacos()
        {
            // All values listed in InvalidCharsInMacOsFileNames trigger an exception when trying to create a file with that name on MacOS
            // This test is a minimal example to reproduce the issue using the character '줰' (U+C930) which trigger an exception in MacOS

            var code = 51504;
            var surrogate = char.ConvertFromUtf32(code);
            PrintCodePointInfo(code);
            var valid = $"filename{surrogate}.txt";

            File.WriteAllText(valid, "content");
        }

        private static void PrintCodePointInfo(int codePoint)
        {
            var charInfo = UnicodeInfo.GetCharInfo(codePoint);
            Console.WriteLine(UnicodeInfo.GetDisplayText(charInfo));
            Console.WriteLine("U+" + codePoint.ToString("X4"));
            Console.WriteLine(charInfo.Name ?? charInfo.OldName);
            Console.WriteLine(charInfo.Category);
        }
    }
}


