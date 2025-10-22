# SanitizeFilename

Sanitizes file and directory names to ensure compatibility with Windows (NTFS & exFat), Linux (ext4), and macOS (APFS).

[![.NET build and test](https://github.com/Codeuctivity/SanitizeFilename/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Codeuctivity/SanitizeFilename/actions/workflows/dotnet.yml) [![NuGet](https://img.shields.io/nuget/v/Codeuctivity.SanitizeFilename.svg)](https://www.nuget.org/packages/Codeuctivity.SanitizeFilename/) [![Donate](https://img.shields.io/static/v1?label=Paypal&message=Donate&color=informational)](https://www.paypal.com/donate?hosted_button_id=7M7UFMMRTS7UE)

Implements rules documented by [Microsoft](https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file#naming-conventions) + file name length truncation to 255 bytes, which is common on [many modern](https://en.wikipedia.org/wiki/Comparison_of_file_systems) file systems + some unicode edge cases. Runs on any .NET platform.

## Example

```csharp
using Codeuctivity;

string unsafeString = "file*Name";
string safeFileName = unsafeString.SanitizeFilename();
Console.WriteLine($"Unsafe: {unsafeString}");
//Unsafe: file*Name
Console.WriteLine($"Sanitized: {safeFileName}");
//Sanitized: file_Name

string safeFileNameOptionalReplacementChar = unsafeString.SanitizeFilename(' ');
Console.WriteLine($"SafeFileNameOptionalReplacementChar: {safeFileNameOptionalReplacementChar}");
//SafeFileNameOptionalReplacementChar: file Name
```

## Rules

Restrictions of Windows, Linux and macOS are all combined to an replacement pattern, that will sanitize any filename to be compatible with any of the OS and common filesystem restrictions.

| Pattern                       | OS that don't support pattern | OS that support pattern | Example            |
| ----------------------------- | ----------------------------- | ----------------------- | ------------------ |
| Reserved keywords             | Windows                       | Linux, macOS             | CON, PRN, AUX, ... |
| Reserved chars                | Linux, Windows, macOS           |                         | '/', '\0'          |
| Reserved chars windows       | Windows                       | Linux, macOS             | '\\\', '""', ...   |
| Invalid trailing chars        | Windows                       | Linux, macOS             | ' ', ','           |
| Max length Linux              | Linux,                        | [Windows, macOS](https://github.com/Codeuctivity/SanitizeFilename/blob/387103492098cd9cef0f8596a96dc6c2dfe2eba3/SanitizeFilenameTests/FilenameTests/LinuxSpecificTests.cs#L20)          | 255 bytes          |
| Max length                    | Linux, Windows, macOS           |                         | 255 chars          |
| Unpaired Unicode surrogates   | macOS, Linux                    | Windows                 | U+D800 - U+DFFF    |
| NotAssigned to Unicode        | macOS                           | Linux, Windows          | U+67803, ...       |
| "New" Unicode (today 17+) | macOS                           | Linux, Windows          | 🫩 [🫈](https://emojipedia.org/hairy-creature), ...  |

## .NET framework support

- Support for legacy .NET versions will be maintained as long as it is [funded](https://github.com/sponsors/Codeuctivity).
- Support for .NET Framework 4.6.2 and higher was added in Version [2.0.145](https://www.nuget.org/packages/Codeuctivity.SanitizeFilename/2.0.145).
- Edge case Unicode sanitization: [.NET Framework](https://learn.microsoft.com/en-us/dotnet/framework/whats-new/#character-categories) uses Unicode 8.0, while .NET 8+ uses a newer version to detect unpaired surrogates and unassigned code points.
  - This is relevant when dealing with emoticons.
  - For example, [&#34;💏🏻&#34;](https://emojipedia.org/kiss-light-skin-tone) will be sanitized when running on .NET Framework 4.8, while it is supported as a valid filename on modern filesystems

## Test setup

The exFat specific tests are skipped as long as no exFat filesystem is available. Use this snippet to enable them:

### Windows

```powershell
$vhdpath = [System.IO.Path]::Combine($env:TEMP, 'ExFatTestContainer.vhd')
Remove-Item $vhdpath -ErrorAction SilentlyContinue
$vhdsize = 100MB
New-VHD -Path $vhdpath -Dynamic -SizeBytes $vhdsize | Mount-VHD -Passthru |Initialize-Disk -Passthru |New-Partition -AssignDriveLetter -UseMaximumSize |Format-Volume -FileSystem 'exFAT' -Confirm:$false  -NewFileSystemLabel '{exfatLabel}' -Force|Out-Null
```

Running as admin will automatically create and mount a exFat drive while tests are running.
