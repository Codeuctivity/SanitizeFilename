namespace DirectoryNameTests
{
    public class DirectoryWriteAsserter
    {
        public DirectoryWriteAsserter()
        {
            TempPath = Path.Combine(Path.GetTempPath(), "test" + Guid.NewGuid());
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
        }

        public string TempPath { get; }

        internal void AssertCollection(List<(string, int)> validDirectoryNames)
        {
            var invalidDirectoryNames = new List<(string, int)>();

            Parallel.ForEach(validDirectoryNames, validDirectoryName =>
            {
                if (!TryWriteDirectoryToTempDirectory(validDirectoryName.Item1))
                {
                    lock (invalidDirectoryNames)
                    {
                        invalidDirectoryNames.Add(validDirectoryName);
                    }
                }
            });

            Assert.That(invalidDirectoryNames.OrderBy(x => x.Item2), Is.Empty, GenerateAssertionMessage(invalidDirectoryNames));
        }

        public bool TryWriteDirectoryToTempDirectory(string directoryName)
        {
            var directoryPath = Path.Combine(TempPath, directoryName);

            try
            {
                Directory.CreateDirectory(directoryPath);

                var directroyExists = IsFileDirectoryNameFound(directoryName);

                if (directroyExists)
                    Directory.Delete(directoryPath);
                return directroyExists;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsFileDirectoryNameFound(string filename)
        {
            string[] allFiles = Directory.GetDirectories(TempPath);

            return allFiles.SingleOrDefault(x => x.EndsWith(filename)) != null;
        }

        private static string GenerateAssertionMessage(List<(string, int)> invalidDirectoryNames)
        {
            return "Invalid chars: " + string.Join(", ", invalidDirectoryNames.Select(x => $"{x.Item2}"));
        }
    }
}