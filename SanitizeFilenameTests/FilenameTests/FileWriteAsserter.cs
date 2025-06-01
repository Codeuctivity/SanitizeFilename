using System.Runtime.InteropServices;

namespace SanitizeFilenameTests
{
    public class FileWriteAsserter : IDisposable
    {
        private bool disposedValue;

        public FileWriteAsserter(bool createAndUseExFatPartition = false)
        {
            TempPath = Path.Combine(Path.GetTempPath(), "test" + Guid.NewGuid());
            CreateAndUseExFatPartition = createAndUseExFatPartition;
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);

            if (createAndUseExFatPartition)
                CreateExFatFileSystemIfNeeded(false);
        }

        public string ExfatDrive { get; private set; }
        public string TempPath { get; set; }
        public bool CreateAndUseExFatPartition { get; }

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

        private void CreateExFatFileSystemIfNeeded(bool useExistingExFatPartition)
        {
            // Only attempt on Windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            // Only create if not already created in this session
            string vhdxPath = TempPath + @"\exfat-test.vhdx";
            // exFAT volume label max length is 11 characters
            string exfatLabel = Guid.NewGuid().ToString("N").Substring(0, 11);
            string? exfatDrive = null;

            // Check if already mounted
            if (useExistingExFatPartition)
            {
                var getVolume = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = $"-NoProfile -Command \"Get-Volume -FileSystemLabel '{exfatLabel}' | Select-Object -ExpandProperty DriveLetter\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                getVolume.Start();
                exfatDrive = getVolume.StandardOutput.ReadToEnd().Trim();
                getVolume.WaitForExit();

                if (!string.IsNullOrEmpty(exfatDrive))
                {
                    TempPath = exfatDrive + @":\test" + Guid.NewGuid();
                    if (!Directory.Exists(TempPath))
                        Directory.CreateDirectory(TempPath);
                    return;
                }
            }

            // Create and mount exFAT VHDX if not present
            string psScript = $@"
if (-not (Test-Path '{vhdxPath}')) {{
    New-VHD -Path '{vhdxPath}' -SizeBytes 100MB -Dynamic | Out-Null
}}
Mount-VHD -Path '{vhdxPath}' | Out-Null
Start-Sleep -Seconds 2
$disk = Get-Disk | Where-Object {{$_.Location -like '*{System.IO.Path.GetFileName(vhdxPath)}*'}} | Where-Object {{$_.PartitionStyle -eq 'RAW' -or $_.PartitionStyle -eq 'GPT'}}
if ($disk -and $disk.PartitionStyle -eq 'RAW') {{
    Initialize-Disk -Number $disk.Number -PartitionStyle GPT -PassThru | Out-Null
    $partition = New-Partition -DiskNumber $disk.Number -UseMaximumSize -AssignDriveLetter
    Format-Volume -Partition $partition -FileSystem exFAT -NewFileSystemLabel '{exfatLabel}' -Confirm:$false | Out-Null
}}
$vol = Get-Volume -FileSystemLabel '{exfatLabel}'
$vol.DriveLetter
";

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -Command \"{psScript.Replace("\"", "`\"")}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            exfatDrive = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(exfatDrive))
            {
                ExfatDrive = exfatDrive;
                TempPath = exfatDrive + @":\test" + Guid.NewGuid();
                if (!Directory.Exists(TempPath))
                    Directory.CreateDirectory(TempPath);
            }
            else
            {
                throw new InvalidOperationException("Failed to create or mount exFAT VHDX for testing.");
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
                    Directory.Delete(TempPath, true);

                    if (CreateAndUseExFatPartition && !string.IsNullOrEmpty(ExfatDrive))
                    {
                        // Unmount the exFAT drive
                        var unmountProcess = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "powershell",
                                Arguments = $"-NoProfile -Command \"Dismount-VHD -Path '{ExfatDrive}'\"",
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
                    }
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