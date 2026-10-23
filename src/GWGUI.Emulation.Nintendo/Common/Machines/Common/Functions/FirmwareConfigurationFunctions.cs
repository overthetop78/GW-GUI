using CommonCoreHostFunctions = GWGUI.Emulation.Nintendo.Common.Functions.CoreHostFunctions;
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
            candidates.AddRange(InspectFile(path, slots, CancellationToken.None));
        return candidates;
    }

    /// <summary>Inspecte un fichier et conserve tous ses rôles de firmware possibles.</summary>
    /// <param name="configuration">Configuration empruntée dont les emplacements sont consultés.</param>
    /// <param name="path">Fichier source, conservé en lecture seule.</param>
    /// <param name="adapter">Backend emprunté fournissant ses emplacements de firmware.</param>
    /// <param name="cancellationToken">Annulation contrôlée avant la lecture du fichier.</param>
    /// <returns>Tous les candidats reconnus ou possibles, avec leurs identités et destinations.</returns>
    internal static ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFileAsync(MachineConfiguration configuration, string path, IEmulatorAdapter adapter, CancellationToken cancellationToken) =>
        ValueTask.FromResult(InspectFile(path, adapter.GetFirmwareSlots(configuration), cancellationToken));

    /// <summary>Réutilise les emplacements déjà obtenus par le parcours de dossier et libère les lectures avant le retour.</summary>
    private static IReadOnlyList<EmulationFirmwareCandidate> InspectFile(string path, IReadOnlyList<FirmwareSlot> slots, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var candidates = new List<EmulationFirmwareCandidate>();
        using var stream = File.OpenRead(path);
        var digest = CommonCoreHostFunctions.StateFiles.HashStream(stream);
        var name = Path.GetFileName(path);
        var exact = slots.Where(slot => Path.GetFileName(slot.FileName).Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
        var targets = exact.Length > 0 ? exact : slots.Where(slot => Path.GetExtension(slot.FileName).Equals(Path.GetExtension(path), StringComparison.OrdinalIgnoreCase));
        foreach (var slot in targets)
            candidates.Add(new EmulationFirmwareCandidate(digest + slot.FieldId, path, slot.FileName + Path.DirectorySeparatorChar + name, null, EmulationFirmwareCompatibility.Unknown, slot.FieldId));
        return candidates;
    }

}
