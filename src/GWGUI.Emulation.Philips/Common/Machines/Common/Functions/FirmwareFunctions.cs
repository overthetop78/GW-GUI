using System.IO;

namespace GWGUI.Emulation.Philips.Common.Machines.Common.Functions;

internal static class FirmwareFunctions
{
    internal static string Field(FirmwareCategory role) => FirmwareConstants.FieldPrefix + role;
    internal static bool IsField(string field) => Enum.GetValues<FirmwareCategory>().Any(role => field == Field(role));
    internal static IReadOnlyList<FirmwareCategory> Roles(MachineModel model) =>
        ConfigurationFunctions.Family(model) switch
        {
            MachineFamily.P2000 => [],
            MachineFamily.Videopac => [FirmwareCategory.SystemBios, FirmwareCategory.VoiceSamples],
            _ => [FirmwareCategory.SystemBios]
        };

    internal static IReadOnlyList<FirmwareConfiguration> Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var result = configuration.Firmwares.ToList();
        foreach (var role in Roles(configuration.Model))
        {
            if (!values.TryGetValue(Field(role), out var path)) continue;
            result.RemoveAll(item => item.Category == role);
            if (!string.IsNullOrWhiteSpace(path)) result.Add(new(role, path, role == FirmwareCategory.SystemBios));
        }
        return result;
    }

    internal static ValueTask<EmulationFirmwareCandidate> ScanFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) =>
        new Engine().Adapter(configuration.Core) is IEmulatorFirmwareAdapter firmware
            ? firmware.ScanFirmwareFileAsync(configuration, path, cancellationToken)
            : ValueTask.FromResult(new EmulationFirmwareCandidate(path, path, Path.GetFileName(path), null, EmulationFirmwareCompatibility.Incompatible, null));

    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate)
    {
        if (candidate.Compatibility != EmulationFirmwareCompatibility.Compatible
            || !IsField(candidate.DestinationFieldId ?? string.Empty))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        var role = Enum.GetValues<FirmwareCategory>().Single(role => Field(role) == candidate.DestinationFieldId);
        return new(role, candidate.Path, role == FirmwareCategory.SystemBios, true);
    }
}
