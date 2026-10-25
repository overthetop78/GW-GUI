using CommonCoreHostFunctions = GWGUI.Emulation.Panasonic.Common.Functions.CoreHostFunctions;
using System.IO;

namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;
internal static class FirmwareFunctions
{
    internal static string Field(FirmwareCategory role) => FirmwareConstants.FieldPrefix + role;
    /// <summary>Retrouve le rôle enregistré à partir de son identifiant de champ stable.</summary>
    internal static FirmwareCategory Role(string fieldId) => Enum.GetValues<FirmwareCategory>().Single(role => Field(role) == fieldId);
    private static IEmulatorFirmwareAdapter? Capabilities(MachineConfiguration configuration)
    {
        var engine = new Engine();
        return configuration.Core == Emulator.None ? engine.Adapters(configuration.MachineId).OfType<IEmulatorFirmwareAdapter>().FirstOrDefault() : engine.Adapter(configuration) as IEmulatorFirmwareAdapter;
    }

    internal static IReadOnlyList<FirmwareSlot> Requirements(MachineConfiguration configuration) => Capabilities(configuration)?.GetFirmwareSlots(configuration) ?? [];
    internal static EmulationSettingsBlock Describe(MachineConfiguration configuration) => new(
        FirmwareConstants.BlockId,
        EmulationMachineTab.Rom,
        FirmwareConstants.TitleResourceKey,
        Requirements(configuration).Select(requirement => new EmulationSettingsField(requirement.FieldId, EmulationMachineTab.Rom, FirmwareConstants.BlockId, requirement.LabelResourceKey, EmulationSettingsEditor.Path, configuration.Firmware.Images.GetValueOrDefault(Role(requirement.FieldId)))).ToArray(),
        IsCompact: true
    );
    internal static FirmwareConfiguration Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var selected = new Dictionary<FirmwareCategory, string>(configuration.Firmware.Images);
        foreach (var requirement in Requirements(configuration))
        {
            if (!values.TryGetValue(requirement.FieldId, out var path))
                continue;
            selected.Remove(Role(requirement.FieldId));
            if (!string.IsNullOrWhiteSpace(path))
                selected[Role(requirement.FieldId)] = Path.GetFullPath(path);
        }

        return new(selected);
    }

    /// <summary>Inspecte un fichier avec les exigences et compatibilités déjà décrites par le backend.</summary>
    /// <param name="configuration">Configuration empruntée du modèle et de son cœur.</param>
    /// <param name="path">Fichier source conservé sous la responsabilité de son propriétaire.</param>
    /// <param name="cancellationToken">Annulation contrôlée avant les vérifications.</param>
    /// <returns>Tous les rôles compatibles, ou le candidat inconnu conservé par le parcours existant.</returns>
    internal static ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var capabilities = Capabilities(configuration);
        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>(capabilities is null ? [] : InspectFile(configuration, path, capabilities, capabilities.GetFirmwareSlots(configuration), cancellationToken));
    }

    /// <summary>Réutilise les capacités empruntées et les exigences déjà résolues pour un parcours de dossier.</summary>
    private static IReadOnlyList<EmulationFirmwareCandidate> InspectFile(MachineConfiguration configuration, string path, IEmulatorFirmwareAdapter capabilities, IReadOnlyList<FirmwareSlot> requirements, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = new List<EmulationFirmwareCandidate>();
        var matches = requirements.Where(requirement => capabilities.IsFirmwareCompatible(configuration, requirement.FieldId, path)).ToArray();
        if (matches.Length == 0)
            result.Add(new(path, path, Path.GetFileName(path), null, EmulationFirmwareCompatibility.Unknown));
        foreach (var requirement in matches)
            result.Add(new(Role(requirement.FieldId) + ":" + path, path, Path.GetFileName(path), null, EmulationFirmwareCompatibility.Compatible, requirement.FieldId));
        return result;
    }

    internal static MachineConfiguration Select(MachineConfiguration configuration, EmulationFirmwareCandidate candidate)
    {
        var requirement = Requirements(configuration).SingleOrDefault(item => item.FieldId == candidate.DestinationFieldId) ?? throw new ArgumentException(nameof(candidate));
        var selected = new Dictionary<FirmwareCategory, string>(configuration.Firmware.Images)
        {
            [Role(requirement.FieldId)] = Path.GetFullPath(candidate.Path)
        };
        var result = configuration with
        {
            Firmware = new(selected)
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
