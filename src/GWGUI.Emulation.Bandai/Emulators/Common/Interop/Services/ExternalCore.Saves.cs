using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Bandai.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private string? _batterySavePath;
    private bool _batteryMemoryReady;

    private static (nint Pointer, int Length) BatteryMemory(ExternalCoreExports exports)
    {
        var size = exports.GetMemorySize(MediaConstants.BatteryMemoryId);
        if (size == 0) return (nint.Zero, 0);
        if (size > MediaConstants.MaximumBatterySaveBytes)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported,
                ErrorMessages.ContentLoadFailed);
        var pointer = exports.GetMemoryData(MediaConstants.BatteryMemoryId);
        if (pointer == nint.Zero)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported,
                ErrorMessages.ContentLoadFailed);
        return (pointer, checked((int)size));
    }

    private void RestoreBatteryMemory()
    {
        if (_batterySavePath is null) return;
        var memory = BatteryMemory(RequireExports());
        if (memory.Length == 0) return;
        if (File.Exists(_batterySavePath))
        {
            if (new FileInfo(_batterySavePath).Length != memory.Length)
                throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported,
                    ErrorMessages.ContentLoadFailed);
            var bytes = File.ReadAllBytes(_batterySavePath);
            Marshal.Copy(bytes, 0, memory.Pointer, bytes.Length);
        }
        _batteryMemoryReady = true;
    }

    private void WriteBatteryMemory(ExternalCoreExports exports)
    {
        if (!_batteryMemoryReady || _batterySavePath is null) return;
        var memory = BatteryMemory(exports);
        if (memory.Length == 0) return;
        var bytes = new byte[memory.Length];
        Marshal.Copy(memory.Pointer, bytes, 0, bytes.Length);
        Directory.CreateDirectory(Path.GetDirectoryName(_batterySavePath)!);
        var temporary = _batterySavePath + ConfigurationStoreConstants.TemporaryNameSeparator
            + Guid.NewGuid().ToString(ConfigurationStoreConstants.MachineIdentifierFormat)
            + ConfigurationStoreConstants.TemporaryFileSuffix;
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                       ConfigurationStoreConstants.WriteBufferSize, FileOptions.WriteThrough))
            {
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporary, _batterySavePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
