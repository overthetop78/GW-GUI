using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Security.Cryptography;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class FirmwareRuntimeFunctions
{
    internal static IReadOnlyList<(string Source, string Native, int Size)> Prepare(MachineConfiguration configuration, string emulatorId,
        CoreFirmwarePersistenceSettings settings, string systemDirectory, string saveDirectory)
    {
        var bios = configuration.FirmwarePaths?.GetValueOrDefault(settings.IdentityFieldId);
        if (string.IsNullOrWhiteSpace(bios)) return [];
        string identity;
        using (var stream = File.OpenRead(bios)) identity = CommonCoreHostFunctions.StateFiles.HashStream(stream);
        var directory = Path.Combine(saveDirectory, emulatorId, settings.DirectoryName, identity);
        return settings.Files.Select(file => PrepareFile(file.FieldId, file.FileName, file.Size)).ToArray();

        (string Source, string Native, int Size) PrepareFile(string field, string nativeFile, int size)
        {
            var native = Path.Combine(systemDirectory, nativeFile);
            var selected = configuration.FirmwarePaths?.GetValueOrDefault(field);
            var source = string.IsNullOrWhiteSpace(selected) ? Path.Combine(directory, Path.GetFileName(nativeFile)) : Path.GetFullPath(selected);
            // A selected dump takes precedence over generated data from an earlier run.
            if (!string.IsNullOrWhiteSpace(selected) || File.Exists(source))
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
                MemoryCardFunctions.PersistFiles([(file.Source, file.Native)], file.Size);
            }
            catch (Exception error) { (errors ??= []).Add(error); }
        }
        if (errors is not null) throw new AggregateException(errors);
    }
}
