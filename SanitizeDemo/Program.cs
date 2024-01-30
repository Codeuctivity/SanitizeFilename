using Codeuctivity;

string unsafeString = "file*Name";
string safeFileName = unsafeString.Sanitize();
Console.WriteLine($"Unsafe: {unsafeString}");
Console.WriteLine($"Sanitized: {safeFileName}");

string safeFileNameOptionalReplacementChar = unsafeString.Sanitize(' ');
Console.WriteLine($"Sanitized: {safeFileNameOptionalReplacementChar}");