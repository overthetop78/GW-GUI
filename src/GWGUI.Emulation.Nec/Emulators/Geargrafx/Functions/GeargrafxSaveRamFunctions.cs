using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePce.Exceptions;
using GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;

namespace GWGUI.Emulation.Nec.Emulators.Geargrafx.Functions;

internal static class GeargrafxSaveRamFunctions
{
    internal static void Load(ExternalCoreApi.GetMemoryData getData,
        ExternalCoreApi.GetMemorySize getSize, string directory)
    {
        var memory = Memory(getData, getSize, out var size);
        if (memory == IntPtr.Zero) return;
        var path = Path.Combine(directory, GeargrafxSaveRamConstants.FileName);
        if (!File.Exists(path)) return;
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length != size)
            throw new InvalidDataException(BeetlePceExceptions.InvalidSaveRamSize(path));
        Marshal.Copy(bytes, GeargrafxSaveRamConstants.FirstByteOffset, memory, size);
    }

    internal static void Save(ExternalCoreApi.GetMemoryData getData,
        ExternalCoreApi.GetMemorySize getSize, string directory)
    {
        var memory = Memory(getData, getSize, out var size);
        if (memory == IntPtr.Zero) return;
        var bytes = new byte[size];
        Marshal.Copy(memory, bytes, GeargrafxSaveRamConstants.FirstByteOffset, size);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, GeargrafxSaveRamConstants.FileName);
        var temporary = path + GeargrafxSaveRamConstants.TemporaryNameSeparator
            + Guid.NewGuid().ToString(GeargrafxSaveRamConstants.TemporaryIdFormat)
            + GeargrafxSaveRamConstants.TemporarySuffix;
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
        var length = getSize(GeargrafxSaveRamConstants.SaveRamType);
        if (length == GeargrafxSaveRamConstants.FirstByteOffset)
        {
            size = GeargrafxSaveRamConstants.FirstByteOffset;
            return IntPtr.Zero;
        }
        if (length > GeargrafxSaveRamConstants.MaximumSize)
            throw new InvalidDataException(BeetlePceExceptions.SaveRamTooLarge());
        size = (int)length;
        return getData(GeargrafxSaveRamConstants.SaveRamType);
    }
}
