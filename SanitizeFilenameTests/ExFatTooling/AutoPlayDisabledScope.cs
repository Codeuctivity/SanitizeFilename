using Microsoft.Win32;
using System.Runtime.InteropServices;

public class AutoPlayDisabledScope : IDisposable
{
    private const string AutoPlayRegKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer";
    private const string AutoPlayRegValue = "NoDriveTypeAutoRun";
    private const int DisableAllAutoPlay = 0xFF;

    private static int? _originalValue;
    private bool disposedValue;

    public bool AutoPlayerInitialState { get; }

    /// <summary>
    /// Temporarily disables AutoPlay if it is enabled. Returns true if it was changed.
    /// </summary>
    public AutoPlayDisabledScope()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            AutoPlayerInitialState = false;
            return;
        }

        object? current = Registry.GetValue(AutoPlayRegKey, AutoPlayRegValue, null);
        if (current is int value && value == DisableAllAutoPlay)
        {
            // Already disabled
            AutoPlayerInitialState = false;
            return;
        }

        _originalValue = current as int?;
        Registry.SetValue(AutoPlayRegKey, AutoPlayRegValue, DisableAllAutoPlay, RegistryValueKind.DWord);
        AutoPlayerInitialState = true;
    }

    /// <summary>
    /// Restores the original AutoPlay setting if it was changed.
    /// </summary>
    public static void RestoreAutoPlay()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return;
        }

        if (_originalValue.HasValue)
        {
            Registry.SetValue(AutoPlayRegKey, AutoPlayRegValue, _originalValue.Value, RegistryValueKind.DWord);
            _originalValue = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                RestoreAutoPlay();
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