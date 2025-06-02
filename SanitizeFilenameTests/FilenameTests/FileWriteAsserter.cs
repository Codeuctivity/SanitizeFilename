using System.Runtime.InteropServices;

namespace SanitizeFilenameTests
{
    public class FileWriteAsserter : IDisposable
    {
        private bool disposedValue;

        public FileWriteAsserter(bool createAndUseExFatPartition = false)
        {
            TempPath = Path.Combine(Path.GetTempPath(), "test" + Guid.NewGuid());
            InitialTempPath = TempPath;
            CreateAndUseExFatPartition = createAndUseExFatPartition;
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);

            if (createAndUseExFatPartition)
                CreateExFatFileSystemIfNeeded(false);
        }

        public string ExfatVhdxPath { get; private set; }
        public string ExfatDrive { get; private set; }
        public string TempPath { get; set; }
        public string InitialTempPath { get; }
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
            string vhdxFileName = System.IO.Path.GetFileName(vhdxPath);
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
                    ExfatDrive = exfatDrive;
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
$disk = Get-Disk | Where-Object {{ $_.Location -like '*{vhdxFileName}*' }}
if ($disk) {{
    if ($disk.PartitionStyle -eq 'RAW') {{
        Initialize-Disk -Number $disk.Number -PartitionStyle GPT -PassThru -ErrorAction SilentlyContinue | Out-Null
    }}
Start-Sleep -Seconds 5
    $partition = Get-Partition -DiskNumber $disk.Number | Where-Object {{ $_.Type -ne 'Reserved' }} | Select-Object -First 1
    if (-not $partition) {{
        $partition = New-Partition -DiskNumber $disk.Number -UseMaximumSize -AssignDriveLetter
    }}
    $vol = $partition | Get-Volume
    if ($vol.FileSystem -ne 'exFAT') {{
        Format-Volume -Partition $partition -FileSystem exFAT -NewFileSystemLabel '{exfatLabel}' -Confirm:$false | Out-Null
        $vol = $partition | Get-Volume
    }}
    $vol.DriveLetter
}}
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
                ExfatVhdxPath = vhdxPath;
                TempPath = stdOut + @":\test" + Guid.NewGuid();
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
                    if (CreateAndUseExFatPartition && !string.IsNullOrEmpty(ExfatVhdxPath))
                    {
                        // Unmount the exFAT VHDX by its file path
                        var unmountProcess = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "powershell",
                                Arguments = $"-NoProfile -Command \"Dismount-VHD -Path '{ExfatVhdxPath}'\"",
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

                        if (File.Exists(ExfatVhdxPath))
                        {
                            File.Delete(ExfatVhdxPath);
                        }
                    }
                    Directory.Delete(InitialTempPath, true);
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