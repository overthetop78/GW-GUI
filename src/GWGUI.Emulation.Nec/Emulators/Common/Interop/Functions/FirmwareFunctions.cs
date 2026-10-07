using System.IO;
using Quasi = GWGUI.Emulation.Nec.Emulators.Quasi88.Constants;
using Neko = GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants;
using Kai = GWGUI.Emulation.Nec.Emulators.NP2Kai.Constants;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;

internal static class FirmwareFunctions
{
    internal static IReadOnlyList<FirmwareSlot> Slots(string emulatorId) => emulatorId switch
    {
        Quasi.CoreConstants.Id => Quasi.FirmwareConstants.Slots,
        Neko.CoreConstants.Id => Neko.FirmwareConstants.Slots,
        Kai.CoreConstants.Id => Kai.FirmwareConstants.Slots,
        _ => []
    };

    internal static IReadOnlyList<EmulationSettingsField> Fields(MachineConfiguration configuration) =>
        Slots(configuration.EmulatorId).Select(slot => new EmulationSettingsField(slot.FieldId,
            EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.Firmware,
            slot.LabelResourceKey, EmulationSettingsEditor.Path,
            configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId),
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware)).ToArray();

    internal static MachineConfiguration Apply(MachineConfiguration configuration,
        IReadOnlyDictionary<string, string?> values)
    {
        var paths = new Dictionary<string, string>(configuration.FirmwarePaths
            ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var slot in Slots(configuration.EmulatorId))
            if (values.TryGetValue(slot.FieldId, out var value))
            {
                if (string.IsNullOrWhiteSpace(value)) paths.Remove(slot.FieldId);
                else paths[slot.FieldId] = value;
            }
        return configuration with { FirmwarePaths = paths };
    }

    internal static IReadOnlyList<EmulationFirmwareCandidate> Scan(MachineConfiguration configuration,
        string directory)
    {
        if (!Directory.Exists(directory)) return [];
        var slots = Slots(configuration.EmulatorId);
        return Directory.EnumerateFiles(directory, FirmwareCatalogConstants.SearchPattern, SearchOption.AllDirectories)
            .SelectMany(path =>
            {
                var inspected = FirmwareCatalog.Inspect(path);
                var name = Path.GetFileName(path);
                var recognized = slots.Where(slot => slot.Aliases.Contains(name,
                    StringComparer.OrdinalIgnoreCase) || slot.KnownMd5?.Equals(inspected.Md5,
                        StringComparison.OrdinalIgnoreCase) == true).ToArray();
                var targets = recognized.Length != 0 ? recognized : slots.Where(slot =>
                    Path.GetExtension(slot.FileName).Equals(Path.GetExtension(path),
                        StringComparison.OrdinalIgnoreCase));
                return targets.Select(slot => new EmulationFirmwareCandidate(
                    inspected.Sha256 + slot.FieldId, inspected.Path,
                    recognized.Length != 0 ? name : slot.FileName + Path.DirectorySeparatorChar + name,
                    null, recognized.Length != 0 && slot.KnownMd5?.Equals(inspected.Md5,
                        StringComparison.OrdinalIgnoreCase) == true
                        ? EmulationFirmwareCompatibility.Official : EmulationFirmwareCompatibility.Unknown,
                    slot.FieldId));
            }).ToArray();
    }

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        foreach (var slot in Slots(configuration.EmulatorId))
            if (configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId) is { } path)
            {
                var directory = Path.Combine(systemDirectory, slot.SystemSubdirectory);
                Directory.CreateDirectory(directory);
                File.Copy(path, Path.Combine(directory, slot.FileName), true);
            }
    }
}
