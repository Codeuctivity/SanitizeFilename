using System.Runtime.InteropServices;

namespace SanitizeFilenameTests.ExFatTooling
{
    public class ExFatFileWriteAsserter : FileWriteAsserter
    {
        public static FileWriteAsserter? TryGetOrCreateExFatPartition(out string reason)
        {
            reason = string.Empty;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (TryGetExFatPartition(out string path))
                    return new FileWriteAsserter(path);

                reason = "ExFatFileWriteAsserterFactory is only applicable on Windows.";
                return null;
            }

            if (RuntimeInformation.OSArchitecture == Architecture.Arm || RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                if (TryGetExFatPartition(out string path))
                    return new FileWriteAsserter(path);

                reason = "Test is skipped on Windows ARM because VHD mounting is not supported.";
                return null;
            }

            if (!IsRunningAsAdministrator())
            {
                if (TryGetExFatPartition(out string path))
                    return new FileWriteAsserter(path);

                reason = "Test requires administrator privileges to create and mount exFAT VHD or an mounted ExFat drive.";
                return null;
            }

            var imageTempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var exFatPartitionCreateInfo = CreateAndMountExFatPartition(imageTempPath);
            FileWriteAsserter fileWriteAsserter = new(exFatPartitionCreateInfo.TempPathInExFatPartition, exFatPartitionCreateInfo.VhdxPath);

            return fileWriteAsserter;
        }

        private static bool TryGetExFatPartition(out string path)
        {
            path = string.Empty;
            try
            {
                var exFatDrives = DriveInfo.GetDrives().ToList();
                var exFatDrive = DriveInfo.GetDrives().FirstOrDefault(fs => fs.IsReady && fs.DriveFormat == "exFAT");
                var fuseDrive = DriveInfo.GetDrives().FirstOrDefault(fs => fs.IsReady && fs.DriveFormat == "fuse" && fs.Name.Contains("exfat_test_mount"));

                if (exFatDrive != null)
                {
                    string testDir = Path.Combine(exFatDrive.RootDirectory.FullName, "test" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(testDir);
                    path = testDir;
                    return true;
                }

                if (fuseDrive != null)
                {
                    string testDir = Path.Combine(fuseDrive.RootDirectory.FullName, "test" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(testDir);
                    path = testDir;
                    return true;
                }
            }
            catch
            {
                // Ignore exceptions and return false
            }
            return false;
        }

        private static bool IsRunningAsAdministrator()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return false;
            using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        private static (string TempPathInExFatPartition, string VhdxPath) CreateAndMountExFatPartition(string ImageTempPath)
        {
            using AutoPlayDisabledScope autoPlayDisabledScope = new AutoPlayDisabledScope();

            var vhdxPath = Path.Combine(ImageTempPath, $"exfat-test-{Guid.NewGuid():N}.vhdx");
            string vhdxFileName = Path.GetFileName(vhdxPath);
            // exFAT volume label max length is 11 characters
            string exfatLabel = Guid.NewGuid().ToString("N").Substring(0, 11);

            string psScript = $@"

$vhdpath = '{vhdxPath}'
$vhdsize = 100MB
New-VHD -Path $vhdpath -Dynamic -SizeBytes $vhdsize | Mount-VHD -Passthru | Initialize-Disk -Passthru | Out-Null
Start-Sleep -Seconds 2
$disk = Get-Disk | Where-Object {{ $_.Location -like '*{vhdxFileName}*' }}
$partition = New-Partition -DiskNumber $disk.Number -UseMaximumSize -AssignDriveLetter
Format-Volume -Partition $partition -FileSystem 'exFAT' -Confirm:$false -NewFileSystemLabel '{exfatLabel}' -Force | Out-Null
$partition.DriveLetter
";
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -Command \"{psScript.Replace("\"", "`\"")}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string stdOut = process.StandardOutput.ReadToEnd().Trim();
            string stdErr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0 || !string.IsNullOrEmpty(stdErr))
            {
                throw new InvalidOperationException(
                    $"Failed to create or mount exFAT VHDX for testing. Exit code: {process.ExitCode}. Output: {stdOut} Error: {stdErr}"
                );
            }

            if (!string.IsNullOrEmpty(stdOut))
            {
                var TempPathOnExFat = stdOut + @":\test" + Guid.NewGuid();
                if (!Directory.Exists(TempPathOnExFat))
                    Directory.CreateDirectory(TempPathOnExFat);

                return (TempPathOnExFat, vhdxPath);
            }
            else
            {
                throw new InvalidOperationException("Failed to create or mount exFAT VHDX for testing.");
            }
        }

        public static void UnmountAndDeleteImage(string exfatVhdxPath)
        {
            if (!string.IsNullOrEmpty(exfatVhdxPath))
            {
                // Unmount the exFAT VHDX by its file path
                var unmountProcess = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = $"-NoProfile -Command \"Dismount-VHD -Path '{exfatVhdxPath}'\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                unmountProcess.Start();
                string stdOut = unmountProcess.StandardOutput.ReadToEnd();
                string stdErr = unmountProcess.StandardError.ReadToEnd();
                unmountProcess.WaitForExit();
                if (unmountProcess.ExitCode != 0)
                {
                    throw new InvalidOperationException($"Failed to dismount exFAT VHD. Exit code: {unmountProcess.ExitCode}. Output: {stdOut} Error: {stdErr}");
                }

                if (File.Exists(exfatVhdxPath))
                {
                    File.Delete(exfatVhdxPath);
                }
            }
        }

        internal static bool SystemIsSupported()
        {
            // if running on windows and not ARM, we can use exFat tooling
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && (RuntimeInformation.OSArchitecture != Architecture.Arm && RuntimeInformation.OSArchitecture != Architecture.Arm64))
            {
                return true;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux and OSX are not supported for exFAT tooling
                return true;
            }

            return false;
        }

        internal static void Assert(bool expected, FileWriteAsserter? exFatFileWriteAsserter, string fileNameTooLongForLinux, string getOrCreateExFatPartitionFailReason, string assertMessage)
        {
            if (!ExFatFileWriteAsserter.SystemIsSupported())
                return;


            NUnit.Framework.Assert.That(exFatFileWriteAsserter, Is.Not.Null, "ExFatFileWriteAsserter should not be null if the system is supported and the partition was created successfully. Setup a exFat drive to run this test or ignore the outcome here, it will be setup and tested on github action runs.");

            var actualExFat = exFatFileWriteAsserter?.TryWriteFileToTempDirectory(fileNameTooLongForLinux);
            NUnit.Framework.Assert.That(actualExFat, Is.EqualTo(expected), assertMessage);
        }
    }
}