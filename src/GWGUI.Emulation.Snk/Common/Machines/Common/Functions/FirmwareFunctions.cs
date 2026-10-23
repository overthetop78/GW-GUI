using System.IO;

namespace GWGUI.Emulation.Snk.Common.Machines.Common.Functions;
internal static class FirmwareFunctions
{
    internal static string Field(FirmwareCategory role) => FirmwareConstants.FieldPrefix + role;
    internal static bool IsField(string field) => Enum.GetValues<FirmwareCategory>().Any(role => field == Field(role));
    /// <summary>Résout les emplacements du cœur choisi avec la configuration complète.</summary>
    internal static IReadOnlyList<FirmwareSlot> Slots(MachineConfiguration configuration) => configuration.Core != Emulator.None ? EmulatorCatalog.CreateAdapter(configuration.Core).GetFirmwareSlots(configuration) : [];
    /// <summary>Retrouve le rôle enregistré à partir de son champ de sélection stable.</summary>
    private static FirmwareCategory Role(string fieldId) => Enum.GetValues<FirmwareCategory>().Single(role => Field(role) == fieldId);
    internal static IReadOnlyList<FirmwareConfiguration> Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var result = configuration.Firmwares.ToList();
        foreach (var slot in Slots(configuration))
        {
            var role = Role(slot.FieldId);
            if (!values.TryGetValue(slot.FieldId, out var path))
                continue;
            result.RemoveAll(item => item.Category == role);
            if (!string.IsNullOrWhiteSpace(path))
                result.Add(new(role, path, slot.IsRequired, IsOriginalFirmware: false));
        }

        return result;
    }

    internal static ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken)
    {
        var model = configuration.Model;
        var core = configuration.Core;
        if (!ConfigurationFunctions.IsCompatible(model, core))
            throw new ArgumentOutOfRangeException(nameof(core));
        cancellationToken.ThrowIfCancellationRequested();
        var absolute = Path.GetFullPath(path);
        return core != Emulator.None && EmulatorCatalog.CreateAdapter(core)is IEmulatorFirmwareAdapter adapter 
            ? adapter.ScanFirmwareFileAsync(configuration, absolute, cancellationToken)
            : ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>([new EmulationFirmwareCandidate(absolute, absolute, Path.GetFileName(absolute), null, EmulationFirmwareCompatibility.Unknown)]);
    }

    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate, MachineConfiguration configuration)
    {
        if (candidate.Compatibility is not EmulationFirmwareCompatibility.Official and not EmulationFirmwareCompatibility.Compatible and not EmulationFirmwareCompatibility.PartiallyCompatible || !IsField(candidate.DestinationFieldId ?? string.Empty))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        var slots = Slots(configuration).Where(slot => slot.FieldId == candidate.DestinationFieldId).ToArray();
        if (slots.Length != 1)
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        return new(Role(slots[0].FieldId), candidate.Path, slots[0].IsRequired, IsOriginalFirmware: false);
    }
}