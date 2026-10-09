using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private ExternalCoreApi.GetMemoryData? _getMemoryData;
    private ExternalCoreApi.GetMemorySize? _getMemorySize;
    private string? _saveMemoryPath;
    private bool _saveMemoryReady;

    private (nint Pointer, int Size) SaveMemory()
    {
        var size = _getMemorySize?.Invoke(SaveMemoryConstants.SaveRamId)
            ?? ExternalCoreInteropConstants.EmptyNativeSize;
        if (size == ExternalCoreInteropConstants.EmptyNativeSize) return default;
        if (size > SaveMemoryConstants.MaximumManagedBufferSize)
            throw new InvalidOperationException(CoreExceptions.InvalidStateSize(size));
        var pointer = _getMemoryData?.Invoke(SaveMemoryConstants.SaveRamId) ?? nint.Zero;
        return pointer == nint.Zero ? default : (pointer, checked((int)size));
    }

    private void RestoreSaveMemory()
    {
        if (_saveMemoryPath is null || !File.Exists(_saveMemoryPath)) return;
        var memory = SaveMemory();
        if (memory.Pointer == nint.Zero) return;
        var bytes = File.ReadAllBytes(_saveMemoryPath);
        if (bytes.Length != memory.Size)
            throw new InvalidDataException(CoreExceptions.InvalidStateSize((nuint)bytes.Length));
        Marshal.Copy(bytes, BufferConstants.FirstBufferIndex, memory.Pointer, bytes.Length);
    }

    private void PersistSaveMemory()
    {
        if (!_saveMemoryReady || _saveMemoryPath is null) return;
        var memory = SaveMemory();
        if (memory.Pointer == nint.Zero) return;
        var bytes = new byte[memory.Size];
        Marshal.Copy(memory.Pointer, bytes, BufferConstants.FirstBufferIndex, bytes.Length);
        var temporaryPath = _saveMemoryPath + SaveMemoryConstants.TemporaryFileSuffix;
        try
        {
            File.WriteAllBytes(temporaryPath, bytes);
            File.Move(temporaryPath, _saveMemoryPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
