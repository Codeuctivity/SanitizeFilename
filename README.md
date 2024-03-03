# SanitizeFilename

Sanitizes filenames

[![.NET build and test](https://github.com/Codeuctivity/SanitizeFilename/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Codeuctivity/SanitizeFilename/actions/workflows/dotnet.yml) [![NuGet](https://img.shields.io/nuget/v/Codeuctivity.SanitizeFilename.svg)](https://www.nuget.org/packages/Codeuctivity.SanitizeFilename/) [![Donate](https://img.shields.io/static/v1?label=Paypal&message=Donate&color=informational)](https://www.paypal.com/donate?hosted_button_id=7M7UFMMRTS7UE)

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

## Rules

Restrictions of Windwos, Linux and MacOs are alle combined to an replacemant pattern, that will sanitize any filename to be compatible with any of the OS and common filesystem restrictions.

| Pattern                          | OS that dont support pattern | OS that support pattern | Example            |
| -------------------------------- | ---------------------------- | ----------------------- | ------------------ |
| Reserverd keywords               | Windows                      | Linux, MacOs           | CON, PRN, AUX, ... |
| Reserved chars                   | Linux, Windows, MacOs        |                         | '/', '\0'          |
| Reserved chars windows          | Windows                      | Linux, MacOs           | '\\\', '""'        |
| NotAssigned to unicode           | MacOs                        | Linux, Windows          |                    |
| InvalidTrailingChars             | Windows                      | Linux, MacOs           |                    |
| Max length Linux                 |                              | Windows, MacOs         |                    |
| Max length others                |                              |                         |                    |
| Unpaired surrogates              |                              |                         |                    |
| UnicodeCategory.OtherNotAssigned |                              |                         |                    |
