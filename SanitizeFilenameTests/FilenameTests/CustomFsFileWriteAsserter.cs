namespace SanitizeFilenameTests
{
    public class CustomFsFileWriteAsserter : FileWriteAsserter
    {
        public CustomFsFileWriteAsserter()
        {
            // Try to create a ramdisk with exFAT on Linux
            // Requires root privileges and exfat-utils/exfatprogs installed

            TempPath = Path.Combine(Path.GetTempPath(), "test" + Guid.NewGuid());
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);

            var ramdiskPath = TempPath;
            var sizeMb = 1; // 1MB ramdisk
            var mountCmd = $"mount -t tmpfs -o size={sizeMb}M tmpfs \"{ramdiskPath}\"";
            var mkfsCmd = $"mkfs.exfat \"{ramdiskPath}\"";

            // Mount tmpfs
            var procMount = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"sudo {mountCmd}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            procMount.Start();
            string mountStdOut = procMount.StandardOutput.ReadToEnd();
            string mountStdErr = procMount.StandardError.ReadToEnd();
            procMount.WaitForExit();
            if (procMount.ExitCode != 0)
            {
                // fails on codespace dev container
                throw new InvalidOperationException(
                    $"Failed to mount tmpfs at {ramdiskPath}. Exit code: {procMount.ExitCode}\nSTDOUT: {mountStdOut}\nSTDERR: {mountStdErr}"
                );
            }

            // Format as exFAT
            var procMkfs = System.Diagnostics.Process.Start("bash", $"-c \"sudo {mkfsCmd}\"");
            procMkfs.WaitForExit();
            if (procMkfs.ExitCode != 0)
            {
                throw new InvalidOperationException($"Failed to format ramdisk as exFAT at {ramdiskPath}. Exit code: {procMkfs.ExitCode}");
            }

        }
    }
}