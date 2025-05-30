using System.Diagnostics;

namespace SanitizeFilenameTests
{
    // that wont run on wsl, gihtug code spaces and github actions https://github.com/microsoft/WSL/issues/11801
    public class CustomFsFileWriteAsserter : FileWriteAsserter
    {
        public CustomFsFileWriteAsserter()
        {
            // Requires root privileges and exfat-utils/exfatprogs installed

            var sizeMb = 16; // 16MB disk image, safe for exFAT
            var diskImagePath = Path.Combine(TempPath, "exfat.img");

            // 1. Create a file to act as a block device
            var createImgCmd = $"dd if=/dev/zero of=\"{diskImagePath}\" bs=1M count={sizeMb}";
            var procImg = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"sudo {createImgCmd}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            procImg.Start();
            string imgStdOut = procImg.StandardOutput.ReadToEnd();
            string imgStdErr = procImg.StandardError.ReadToEnd();
            procImg.WaitForExit();
            if (procImg.ExitCode != 0)
                throw new InvalidOperationException(
                    $"Failed to create disk image. Exit code: {procImg.ExitCode}\nSTDOUT: {imgStdOut}\nSTDERR: {imgStdErr}"
                );

            // 2. Format the file as exFAT
            var mkfsCmd = $"sudo mkfs.exfat -n testlabel \"{diskImagePath}\"";
            var mkfsProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{mkfsCmd}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            mkfsProcess.Start();
            mkfsProcess.WaitForExit();
            if (mkfsProcess.ExitCode != 0)
            {
                var errorOutput = mkfsProcess.StandardError.ReadToEnd();
                throw new InvalidOperationException($"Failed to format disk image as exFAT. Exit code: {mkfsProcess.ExitCode}. Error: {errorOutput}");
            }

            // 3. Mount the file as a loop device
            TempPath = Path.Combine(Path.GetTempPath(), "test" + Guid.NewGuid());
            Directory.CreateDirectory(TempPath);

            // this will fail when running on code spaces or similar environments where mounting is locked down
            var mountCmd = $"mount -o loop \"{diskImagePath}\" \"{TempPath}\"";
            var procMount = new Process
            {
                StartInfo = new ProcessStartInfo
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
                throw new InvalidOperationException(
                    $"Failed to mount exFAT image at {TempPath}. Exit code: {procMount.ExitCode}\nSTDOUT: {mountStdOut}\nSTDERR: {mountStdErr}"
                );
        }
    }
}