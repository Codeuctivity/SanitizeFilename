namespace SanitizeFilenameTests
{
    [Parallelizable(ParallelScope.Fixtures)]
    public class SanitizeFilenamesTestsBase
    {
        public static bool IsRunningOnNet4x()
        {
            var version = Environment.Version;
            return version.Major == 4;
        }
    }
}