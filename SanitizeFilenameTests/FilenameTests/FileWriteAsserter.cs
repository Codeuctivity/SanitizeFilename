namespace SanitizeFilenameTests
{
    public class FileWriteAsserter
    {
        public FileWriteAsserter()
        {
            TempPath = Path.Combine(Path.GetTempPath(), "test" + Guid.NewGuid());
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
        }

        public string TempPath { get; }

        internal void AssertCollection(List<(string, int)> validFilenames)
        {
            var invalidFilenames = new List<(string, int)>();

            Parallel.ForEach(validFilenames, validFilename =>
            {
                if (!TryWriteFileToTempDirectory(validFilename.Item1))
                {
                    lock (invalidFilenames)
                    {
                        invalidFilenames.Add(validFilename);
                    }
                }
            });

            //invalidFilenames.Add(("test", 1));
            //invalidFilenames.Add(("test", 2));

            Assert.That(invalidFilenames.OrderBy(x => x.Item2), Is.Empty, GenerateAssertionMessage(invalidFilenames));
        }

        public bool TryWriteFileToTempDirectory(string filename)
        {
            try
            {
                var filePath = Path.Combine(TempPath, filename);
                File.WriteAllText(filePath, "a");

                var fileExists = IsFileWithNameFound(filename);

                if (fileExists)
                    File.Delete(filePath);
                return fileExists;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsFileWithNameFound(string filename)
        {
            string[] allFiles = Directory.GetFiles(TempPath);

            return allFiles.SingleOrDefault(x => x.EndsWith(filename)) != null;
        }

        private static string GenerateAssertionMessage(List<(string, int)> invalidFilenames)
        {
            return "Invalid chars: " + string.Join(", ", invalidFilenames.Select(x => $"{x.Item2}"));
        }
    }
}