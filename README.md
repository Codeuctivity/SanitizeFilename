# SanitizeFilename

Sanitizes filenames

[![.NET build and test](https://github.com/Codeuctivity/SanitizeFilename/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Codeuctivity/SanitizeFilename/actions/workflows/dotnet.yml) [![NuGet](https://img.shields.io/nuget/v/Codeuctivity.SanitizeFilename.svg)](https://www.nuget.org/packages/Codeuctivity.SanitizeFilename/) [![Donate](https://img.shields.io/static/v1?label=Paypal&message=Donate&color=informational)](https://www.paypal.com/donate?hosted_button_id=7M7UFMMRTS7UE)

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
