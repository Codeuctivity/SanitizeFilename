# SanitizeFilename

Sanitizes file and directory names in a manner that is compatible with Windows, Linux and OsX.

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

Restrictions of Windows, Linux and OsX are alle combined to an replacement pattern, that will sanitize any filename to be compatible with any of the OS and common filesystem restrictions.

| Pattern                          | OS that don't support pattern | OS that support pattern | Example            |
| -------------------------------- | ----------------------------- | ----------------------- | ------------------ |
| Reserved keywords                | Windows                       | Linux, OsX              | CON, PRN, AUX, ... |
| Reserved chars                   | Linux, Windows, OsX           |                         | '/', '\0'          |
| Reserved chars windows           | Windows                       | Linux, OsX              | '\\\', '""', ...   |
| Invalid trailing chars           | Windows                       | Linux, OsX              | ' ', ','           |
| Max length Linux                 | Linux,                        | Windows, OsX            | 255 bytes          |
| Max length                       | Linux, Windows, OsX           |                         | 255 chars          |
| Unpaired Unicode surrogates      | OsX, Linux                    | Windows                 | U+D800 - U+DFFF    |
| NotAssigned to Unicode           | OsX                           | Linux, Windows          | U+67803, ...       |

## .net framework support

- Support for legacy .NET versions will be maintained as long as it is [funded](https://github.com/sponsors/Codeuctivity).
- Support for .NET Framework 4.6.2 and higher was added in Version 1.x.
- Edge case Unicode sanitization: [.NET Framework](https://learn.microsoft.com/en-us/dotnet/framework/whats-new/#character-categories) uses Unicode 8.0, while .NET 8+ uses a newer version to detect unpaired surrogates and unassigned code points.
	- This is relevant when dealing with emoticons.
	- For example, ["💏🏻"](https://emojipedia.org/kiss-light-skin-tone) will be sanitized when running on .NET Framework 4.8, while it is supported as a valid filename on modern filesystems