using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Functions;

internal static class FirmwareConfigurationFunctions
{
    internal static IReadOnlyList<EmulationSettingsField> Fields(MachineConfiguration configuration, IEmulatorAdapter adapter) =>
        adapter.GetFirmwareSlots(configuration).Select(slot => new EmulationSettingsField(slot.FieldId,
            EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.Firmware,
            slot.LabelResourceKey, EmulationSettingsEditor.Path,
            configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId),
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware)).ToArray();

    internal static MachineConfiguration Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values, IEmulatorAdapter adapter)
    {
        var paths = new Dictionary<string, string>(configuration.FirmwarePaths ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var slot in adapter.GetFirmwareSlots(configuration))
            if (values.TryGetValue(slot.FieldId, out var value))
            {
                if (string.IsNullOrWhiteSpace(value)) paths.Remove(slot.FieldId);
                else paths[slot.FieldId] = value;
            }
        return configuration with { FirmwarePaths = paths };
    }

    internal static IReadOnlyList<EmulationFirmwareCandidate> Scan(MachineConfiguration configuration, string directory, IEmulatorAdapter adapter)
    {
        if (!Directory.Exists(directory)) return [];
        var slots = adapter.GetFirmwareSlots(configuration);
        var candidates = new List<EmulationFirmwareCandidate>();
        foreach (var path in Directory.EnumerateFiles(directory, FirmwareCatalogConstants.SearchPattern, SearchOption.AllDirectories))
        {
            using var stream = File.OpenRead(path);
            var digest = Convert.ToHexString(SHA256.HashData(stream));
            var name = Path.GetFileName(path);
            var exact = slots.Where(slot => Path.GetFileName(slot.FileName).Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
            var targets = exact.Length > 0 ? exact : slots.Where(slot =>
                Path.GetExtension(slot.FileName).Equals(Path.GetExtension(path), StringComparison.OrdinalIgnoreCase));
            foreach (var slot in targets)
                candidates.Add(new EmulationFirmwareCandidate(digest + slot.FieldId, path,
                    slot.FileName + Path.DirectorySeparatorChar + name, null,
                    EmulationFirmwareCompatibility.Unknown, slot.FieldId));
        }
        return candidates;
    }

}
