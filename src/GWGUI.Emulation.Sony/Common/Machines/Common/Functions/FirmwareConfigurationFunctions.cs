using System.IO;
using System.Security.Cryptography;
using PlayStationFirmware = GWGUI.Emulation.Sony.Common.Machines.PlayStation.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static class FirmwareConfigurationFunctions
{
    internal static IReadOnlyList<FirmwareSlot> Slots(MachineConfiguration configuration, IEmulatorAdapter adapter) => (adapter as IEmulatorFirmwareSettings)?.GetFirmwareSlots(configuration) ?? [];

    internal static IReadOnlyList<EmulationSettingsField> Fields(MachineConfiguration configuration, IEmulatorAdapter adapter) =>
        Slots(configuration, adapter).Select(slot => new EmulationSettingsField(slot.FieldId,
            EmulationMachineTab.Rom, SettingsDescriptionFunctionsConstants.Firmware,
            slot.LabelResourceKey, EmulationSettingsEditor.Path,
            configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId)
                ?? (configuration.Model == ModelConstants.PlayStation && configuration.FirmwarePaths?.Count == 1 ? configuration.FirmwarePaths.Values.Single() : null),
            ExplanationResourceKey: slot.ExplanationResourceKey, DetailedExplanationResourceKey: slot.DetailedExplanationResourceKey,
            DefaultFolderCategory: EmulationDefaultFolderCategory.Firmware)).ToArray();

    internal static MachineConfiguration Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values, IEmulatorAdapter adapter)
    {
        var paths = new Dictionary<string, string>(configuration.FirmwarePaths ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var slot in Slots(configuration, adapter))
            if (values.TryGetValue(slot.FieldId, out var value))
            {
                if (configuration.Model == ModelConstants.PlayStation)
                    paths.Clear();
                if (string.IsNullOrWhiteSpace(value)) paths.Remove(slot.FieldId);
                else paths[slot.FieldId] = value;
            }
        return configuration with { FirmwarePaths = paths };
    }

    internal static IReadOnlyList<EmulationFirmwareCandidate> Scan(MachineConfiguration configuration, string directory, IEmulatorAdapter adapter, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directory)) return [];
        var candidates = new List<EmulationFirmwareCandidate>();
        var slots = Slots(configuration, adapter);
        foreach (var path in Directory.EnumerateFiles(directory, FirmwareCatalogConstants.SearchPattern, SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = File.OpenRead(path);
            var digest = Convert.ToHexString(SHA256.HashData(stream));
            var name = Path.GetFileName(path);
            foreach (var slot in MatchingSlots(configuration.Model, slots, name))
                candidates.Add(new EmulationFirmwareCandidate(digest + slot.FieldId, path, name, null, EmulationFirmwareCompatibility.Unknown, slot.FieldId));
        }
        return candidates;
    }

    internal static IReadOnlyList<FirmwareSlot> MatchingSlots(string model, IReadOnlyList<FirmwareSlot> slots, string fileName)
    {
        var named = slots.Where(slot => slot.CandidateFileNames?.Contains(fileName, StringComparer.OrdinalIgnoreCase) == true).ToArray();
        if (named.Length > 0) return named;
        var extension = Path.GetExtension(fileName);
        return slots.Where(slot => model == ModelConstants.PlayStation ? PlayStationFirmware.BiosExtensions.Contains(extension)
            : Path.GetExtension(slot.FileName).Equals(extension, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    internal static void CopySelectedFiles(MachineConfiguration configuration, IEmulatorAdapter adapter, string destinationDirectory)
    {
        if (configuration.Model == ModelConstants.PlayStation)
        {
            var selected = configuration.FirmwarePaths?.GetValueOrDefault(PlayStationFirmware.SelectedField);
            // Older configurations with exactly one ROM retain that selection; multiple
            // selections require the user to choose the BIOS in the ROM page.
            if (string.IsNullOrWhiteSpace(selected) && configuration.FirmwarePaths?.Count == 1)
                selected = configuration.FirmwarePaths.Values.Single();
            if (string.IsNullOrWhiteSpace(selected)) return;
            Directory.CreateDirectory(destinationDirectory);
            foreach (var fileName in Slots(configuration, adapter).Select(slot => slot.FileName)
                .Concat(new[] { PlayStationFirmware.JapanFile, PlayStationFirmware.NorthAmericaFile,
                    PlayStationFirmware.EuropeFile, PlayStationFirmware.PspFile, PlayStationFirmware.Ps3File,
                    PlayStationFirmware.OpenBiosFile }).Distinct(StringComparer.OrdinalIgnoreCase))
                File.Copy(selected, Path.Combine(destinationDirectory, fileName), true);
            return;
        }
        foreach (var slot in Slots(configuration, adapter))
        {
            var source = configuration.FirmwarePaths?.GetValueOrDefault(slot.FieldId);
            if (string.IsNullOrWhiteSpace(source)) continue;
            var destination = Path.Combine(destinationDirectory, slot.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(source, destination, true);
        }
    }
}
