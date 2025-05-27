# SanitizeFilename

Sanitizes file and directory names to ensure compatibility with Windows (NTFS), Linux (ext4), and macOS (APFS).

Implements rules documented by [Microsoft](https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file#naming-conventions) + file name length truncation to 255 bytes - common on [many modern](https://en.wikipedia.org/wiki/Comparison_of_file_systems) file systems. Runs on any .net8 target platform.

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
