
namespace SanitizeFilenameTests
{
    public class FileWriteAsserter
    {
        public static bool TryWriteFileToTempDirectory(string filename)
        {
            try
            {
                var tempPath = Path.Combine(Path.GetTempPath(), "test");
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                var filePath = Path.Combine(tempPath, filename);
                File.WriteAllText(filePath, "test");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static void AssertCollection(List<(string, int)> validFilenames)
        {
            var invalidFilenames = new List<(string, int)>();
            foreach (var validFilename in validFilenames)
            {
                if (!TryWriteFileToTempDirectory(validFilename.Item1))
                {
                    invalidFilenames.Add(validFilename);
                }
            }

            //invalidFilenames.Add(("test", 1));
            //invalidFilenames.Add(("test", 2));

            Assert.That(invalidFilenames.Count, Is.Zero, GenerateAssertionMessage(invalidFilenames));
        }

        private static string GenerateAssertionMessage(List<(string, int)> invalidFilenames)
        {
            return "Invalid chars: " + string.Join(", ", invalidFilenames.Select(x => $"{x.Item2}"));
        }
    }
}