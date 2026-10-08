using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static class FirmwareConfigurationFunctions
{
    internal static IReadOnlyList<EmulationSettingsField> Fields(MachineConfiguration configuration, IEmulatorAdapter adapter) =>
        adapter.GetFirmwareSlots(configuration).Select(slot => new EmulationSettingsField(slot.FieldId,
            EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.Firmware,
            slot.LabelResourceKey, EmulationSettingsEditor.Path,
            configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId),
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware)).ToArray();

    internal static MachineConfiguration Apply(MachineConfiguration configuration,
        IReadOnlyDictionary<string, string?> values, IEmulatorAdapter adapter)
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

    internal static IReadOnlyList<EmulationFirmwareCandidate> Scan(MachineConfiguration configuration,
        string directory, IEmulatorAdapter adapter, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directory)) return [];
        var candidates = new List<EmulationFirmwareCandidate>();
        var slots = adapter.GetFirmwareSlots(configuration);
        foreach (var path in Directory.EnumerateFiles(directory, FirmwareCatalogConstants.SearchPattern, SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = File.OpenRead(path);
            var digest = Convert.ToHexString(SHA256.HashData(stream));
            var name = Path.GetFileName(path);
            foreach (var slot in slots.Where(slot => Path.GetExtension(slot.FileName)
                .Equals(Path.GetExtension(path), StringComparison.OrdinalIgnoreCase)))
                candidates.Add(new EmulationFirmwareCandidate(digest + slot.FieldId, path,
                    (slot.DisplayName ?? slot.FileName) + Path.DirectorySeparatorChar + name, null,
                    EmulationFirmwareCompatibility.Unknown, slot.FieldId));
        }
        return candidates;
    }

    internal static void CopySelectedFiles(MachineConfiguration configuration, IEmulatorAdapter adapter,
        string destinationDirectory)
    {
        foreach (var slot in adapter.GetFirmwareSlots(configuration))
        {
            var source = configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId);
            if (string.IsNullOrWhiteSpace(source)) continue;
            var destination = Path.Combine(destinationDirectory, slot.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination, true);
        }
    }
}
