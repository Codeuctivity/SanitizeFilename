# SanitizeFilename

Sanitizes filenames

Implements rules documented by [Microsoft](https://docs.microsoft.com/en-us/windows/win32/fileio/naming-a-file#naming-conventions). Runs on any .net8 target platform.

## Example

```csharp
using Codeuctivity;

string unsafeString = "file*Name";
string safeFileName = unsafeString.Sanitize();
Console.WriteLine($"Unsafe: {unsafeString}");
Console.WriteLine($"Sanitized: {safeFileName}");

string safeFileNameOptionalReplacementChar = unsafeString.Sanitize(' ');
Console.WriteLine($"Sanitized: {safeFileNameOptionalReplacementChar}");

//Unsafe: file*Name
//Sanitized: file_Name
//Sanitized: file Name
```