using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Exceptions;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePce.Functions;

internal static class CoreSaveRamFunctions
{
    internal static void Load(ExternalCoreApi.GetMemoryData getData,
        ExternalCoreApi.GetMemorySize getSize, string directory)
    {
        var memory = Memory(getData, getSize, out var size);
        if (memory == IntPtr.Zero) return;
        var path = Path.Combine(directory, CoreSaveRamConstants.FileName);
        if (!File.Exists(path)) return;
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length != size)
            throw new InvalidDataException(BeetlePceExceptions.InvalidSaveRamSize(path));
        Marshal.Copy(bytes, CoreSaveRamConstants.FirstByteOffset, memory, size);
    }

    internal static void Save(ExternalCoreApi.GetMemoryData getData,
        ExternalCoreApi.GetMemorySize getSize, string directory)
    {
        var memory = Memory(getData, getSize, out var size);
        if (memory == IntPtr.Zero) return;
        var bytes = new byte[size];
        Marshal.Copy(memory, bytes, CoreSaveRamConstants.FirstByteOffset, size);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, CoreSaveRamConstants.FileName);
        var temporary = path + CoreSaveRamConstants.TemporaryNameSeparator
            + Guid.NewGuid().ToString(CoreSaveRamConstants.TemporaryIdFormat)
            + CoreSaveRamConstants.TemporarySuffix;
        try
        {
            File.WriteAllBytes(temporary, bytes);
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static nint Memory(ExternalCoreApi.GetMemoryData getData,
        ExternalCoreApi.GetMemorySize getSize, out int size)
    {
        var length = getSize(CoreSaveRamConstants.SaveRamType);
        if (length == CoreSaveRamConstants.FirstByteOffset)
        {
            size = CoreSaveRamConstants.FirstByteOffset;
            return IntPtr.Zero;
        }
        if (length > CoreSaveRamConstants.MaximumSize)
            throw new InvalidDataException(BeetlePceExceptions.SaveRamTooLarge());
        size = (int)length;
        return getData(CoreSaveRamConstants.SaveRamType);
    }
}
