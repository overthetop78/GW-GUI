using System.IO;
using System.Security.Cryptography;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using FirmwareConstants = GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants.FirmwareConstants;
using NativeMemoryCards = GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.MemoryCardFunctions;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static IReadOnlyList<(string Source, string Native, int Size)> Prepare(MachineConfiguration configuration, string systemDirectory, string saveDirectory)
    {
        var bios = configuration.FirmwarePaths?.GetValueOrDefault(FirmwareConstants.SystemField);
        if (string.IsNullOrWhiteSpace(bios)) return [];
        string identity;
        using (var stream = File.OpenRead(bios)) identity = Convert.ToHexString(SHA256.HashData(stream));
        var directory = Path.Combine(saveDirectory, Pcsx2Constants.Id, FirmwareConstants.PersistentDirectory, identity);
        return
        [
            PrepareFile(FirmwareConstants.NvramField, FirmwareConstants.NvramFile, FirmwareConstants.NvramSize),
            PrepareFile(FirmwareConstants.MechanicsField, FirmwareConstants.MechanicsFile, FirmwareConstants.MechanicsSize)
        ];

        (string Source, string Native, int Size) PrepareFile(string field, string nativeFile, int size)
        {
            var native = Path.Combine(systemDirectory, nativeFile);
            var selected = configuration.FirmwarePaths?.GetValueOrDefault(field);
            var source = string.IsNullOrWhiteSpace(selected) ? Path.Combine(directory, Path.GetFileName(nativeFile)) : Path.GetFullPath(selected);
            // Explicit selections were prepared by CopySelectedFiles. Restore generated
            // BIOS data only when the user has not selected a replacement dump.
            if (string.IsNullOrWhiteSpace(selected) && File.Exists(source))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(native)!);
                File.Copy(source, native, overwrite: true);
            }
            return (source, native, size);
        }
    }

    internal static void Persist(IReadOnlyList<(string Source, string Native, int Size)> files)
    {
        List<Exception>? errors = null;
        foreach (var file in files)
        {
            try
            {
                if (!File.Exists(file.Native)) continue;
                Directory.CreateDirectory(Path.GetDirectoryName(file.Source)!);
                NativeMemoryCards.PersistFiles([(file.Source, file.Native)], file.Size);
            }
            catch (Exception error) { (errors ??= []).Add(error); }
        }
        if (errors is not null) throw new AggregateException(errors);
    }
}
