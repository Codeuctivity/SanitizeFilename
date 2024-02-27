using Codeuctivity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace SanitizeFilenameTests
{
    internal class MacOsSpecificTest
    {
        [Test, Platform(Include = "MacOsX")]
        public void InvalidCharsInMacOsFilenameShouldFailToWriteFile()
        {
            var validFilenames = new List<(string, int)>();
            foreach (var item in SanitizeFilename.InvalidCharsInMacOsFileNames)
            {

                var invalidFilename = item + ".txt";
                var sanitizedFilename = invalidFilename.SanitizeFilename();
                Assert.That(sanitizedFilename, Is.Not.EqualTo(invalidFilename));
                validFilenames.Add((sanitizedFilename, item));
            }

            FileWriteAsserter.AssertCollection(validFilenames);
        }


        [Test]
        public void ExperimentWithUnicodeException()
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


