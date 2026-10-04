using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Exceptions;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Functions;

internal static class PcFxBackupMemoryFunctions
{
    internal static void ValidateExternalCard(string path)
    {
        if (!File.Exists(path) || new FileInfo(path).Length != PcFxBackupMemoryConstants.BankSize)
            throw new InvalidDataException(BeetlePcfxExceptions.InvalidBackupSize(path));
    }

    internal static void EnsureInternalFile(string directory)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, PcFxBackupMemoryConstants.InternalFileName);
        if (File.Exists(path)) return;
        using var backup = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        backup.SetLength(PcFxBackupMemoryConstants.BankSize);
    }

    internal static void Load(ExternalCoreApi.GetMemoryData getData,
        ExternalCoreApi.GetMemorySize getSize, string directory, string? externalPath)
    {
        var memory = Memory(getData, getSize);
        LoadBank(Path.Combine(directory, PcFxBackupMemoryConstants.InternalFileName), memory);
        if (!string.IsNullOrWhiteSpace(externalPath))
            LoadBank(externalPath, memory + PcFxBackupMemoryConstants.BankSize);
    }

    internal static void Save(ExternalCoreApi.GetMemoryData getData,
        ExternalCoreApi.GetMemorySize getSize, string directory, string? externalPath)
    {
        var memory = Memory(getData, getSize);
        SaveBank(Path.Combine(directory, PcFxBackupMemoryConstants.InternalFileName), memory);
        if (!string.IsNullOrWhiteSpace(externalPath))
            SaveBank(externalPath, memory + PcFxBackupMemoryConstants.BankSize);
    }

    private static nint Memory(ExternalCoreApi.GetMemoryData getData,
        ExternalCoreApi.GetMemorySize getSize)
    {
        var size = getSize(PcFxBackupMemoryConstants.SaveRamType);
        var memory = getData(PcFxBackupMemoryConstants.SaveRamType);
        if (memory == IntPtr.Zero || size != PcFxBackupMemoryConstants.BankCount
            * PcFxBackupMemoryConstants.BankSize)
            throw new InvalidDataException(BeetlePcfxExceptions.UnexpectedBackupMemorySize());
        return memory;
    }

    private static void LoadBank(string path, nint memory)
    {
        if (!File.Exists(path)) return;
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length != PcFxBackupMemoryConstants.BankSize)
            throw new InvalidDataException(BeetlePcfxExceptions.InvalidBackupSize(path));
        if (bytes.All(value => value == PcFxBackupMemoryConstants.EmptyFileByte)) return;
        Marshal.Copy(bytes, PcFxBackupMemoryConstants.FirstByteOffset, memory, bytes.Length);
    }

    private static void SaveBank(string path, nint memory)
    {
        var bytes = new byte[PcFxBackupMemoryConstants.BankSize];
        Marshal.Copy(memory, bytes, PcFxBackupMemoryConstants.FirstByteOffset, bytes.Length);
        var directory = Path.GetDirectoryName(path);
        if (directory is not null) Directory.CreateDirectory(directory);
        var temporary = path + PcFxBackupMemoryConstants.TemporaryNameSeparator
            + Guid.NewGuid().ToString(PcFxBackupMemoryConstants.TemporaryIdFormat)
            + PcFxBackupMemoryConstants.TemporarySuffix;
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
}
