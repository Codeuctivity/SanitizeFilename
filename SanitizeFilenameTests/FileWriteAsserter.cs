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
    }
}