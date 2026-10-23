using System.IO;

namespace GWGUI.Emulation.Nec.Common.Machines.Common.Functions;

internal static class FirmwareFunctions
{
    /// <summary>Consulte le catalogue du backend par le contrat de réglages firmware commun.</summary>
    /// <param name="configuration">Configuration du modèle et du cœur sélectionnés.</param>
    /// <returns>Emplacements du backend, ou liste vide s’il ne fournit pas ce catalogue.</returns>
    internal static IReadOnlyList<FirmwareSlot> Slots(MachineConfiguration configuration) =>
        new Engine().SelectedAdapter(configuration) is IEmulatorFirmwareSettings settings ? settings.GetFirmwareSlots(configuration) : [];

    internal static MachineConfiguration Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var paths = new Dictionary<string, string>(configuration.FirmwarePaths ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var slot in Slots(configuration))
            if (values.TryGetValue(slot.FieldId, out var value))
            {
                if (string.IsNullOrWhiteSpace(value)) paths.Remove(slot.FieldId);
                else paths[slot.FieldId] = value;
            }
        return configuration with { FirmwarePaths = paths };
    }

    /// <summary>Inspecte un fichier et conserve tous ses rôles de firmware possibles.</summary>
    /// <param name="configuration">Configuration empruntée dont les emplacements sont consultés.</param>
    /// <param name="path">Fichier source, conservé en lecture seule.</param>
    /// <param name="cancellationToken">Annulation contrôlée avant la lecture du fichier.</param>
    /// <returns>Tous les candidats reconnus ou possibles, avec leurs identités et destinations.</returns>
    internal static ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var slots = Slots(configuration);
        if (slots.Count != 0) return ValueTask.FromResult(InspectFile(path, slots, cancellationToken));
        if (!ModelCatalog.Get(configuration.Model).SupportsCdDrive) return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>([]);
        var firmware = FirmwareCatalog.Inspect(path);
        var compatible = firmware.CompatibleModels.Contains(configuration.Model, StringComparer.OrdinalIgnoreCase);
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>([new(firmware.Sha256, firmware.Path,
            firmware.Name ?? Path.GetFileName(firmware.Path), firmware.Version,
            compatible ? !firmware.IsVerified ? EmulationFirmwareCompatibility.Unknown : firmware.IsOfficial ? EmulationFirmwareCompatibility.Official : EmulationFirmwareCompatibility.Compatible
                : firmware.IsKnown ? EmulationFirmwareCompatibility.Incompatible : EmulationFirmwareCompatibility.Unknown,
            compatible || !firmware.IsKnown ? SettingsConstants.FirmwarePath : null)]);
    }

    /// <summary>Réutilise les emplacements déjà obtenus par le parcours de dossier et libère les lectures avant le retour.</summary>
    private static IReadOnlyList<EmulationFirmwareCandidate> InspectFile(string path, IReadOnlyList<FirmwareSlot> slots, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var inspected = FirmwareCatalog.Inspect(path);
        var name = Path.GetFileName(path);
        var recognized = slots.Where(slot => slot.Aliases.Contains(name, StringComparer.OrdinalIgnoreCase) || slot.KnownMd5?.Equals(inspected.Md5, StringComparison.OrdinalIgnoreCase) == true).ToArray();
        var targets = recognized.Length != 0 ? recognized : slots.Where(slot => Path.GetExtension(slot.FileName).Equals(Path.GetExtension(path), StringComparison.OrdinalIgnoreCase));
        return targets.Select(slot => new EmulationFirmwareCandidate(
            inspected.Sha256 + slot.FieldId, inspected.Path,
            recognized.Length != 0 ? name : slot.FileName + Path.DirectorySeparatorChar + name,
            null, recognized.Length != 0 && slot.KnownMd5?.Equals(inspected.Md5, StringComparison.OrdinalIgnoreCase) == true ? EmulationFirmwareCompatibility.Official : EmulationFirmwareCompatibility.Unknown,
            slot.FieldId)).ToArray();
    }
}
