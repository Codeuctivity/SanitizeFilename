using SanitizeFilenameTests.ExFatTooling;

namespace SanitizeFilenameTests
{
    public class FileWriteAsserter : IDisposable
    {
        private bool disposedValue;

        public FileWriteAsserter(string? tempPath = null, string? disposableVhdxPath = null)
        {
            DisposableVhdxPath = disposableVhdxPath;
            TempPath = tempPath ?? Path.Combine(Path.GetTempPath(), "test" + Guid.NewGuid());

            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
        }

        public string? DisposableVhdxPath { get; }
        public string TempPath { get; set; }

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (DisposableVhdxPath != null)
                        ExFatFileWriteAsserterFactory.UnmountAndDeleteImage(DisposableVhdxPath);
                    else
                        Directory.Delete(TempPath, true);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
