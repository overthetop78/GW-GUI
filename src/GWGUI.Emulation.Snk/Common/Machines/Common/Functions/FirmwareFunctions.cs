using System.IO;

namespace GWGUI.Emulation.Snk.Common.Machines.Common.Functions;
internal static class FirmwareFunctions
{
    internal static string Field(FirmwareCategory role) => FirmwareConstants.FieldPrefix + role;
    internal static bool IsField(string field) => Enum.GetValues<FirmwareCategory>().Any(role => field == Field(role));
    internal static IReadOnlyList<FirmwareCategory> Roles(MachineModel model, Emulator core) => core != Emulator.None && EmulatorCatalog.CreateAdapter(core)is IEmulatorFirmwareSettings adapter ? adapter.FirmwareRoles(model) : [];
    internal static bool IsRequired(MachineModel model, Emulator core, FirmwareCategory role) => core != Emulator.None && EmulatorCatalog.CreateAdapter(core)is IEmulatorFirmwareSettings adapter && adapter.IsFirmwareRequired(model, role);
    internal static IReadOnlyList<FirmwareConfiguration> Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var result = configuration.Firmwares.ToList();
        foreach (var role in Roles(configuration.Model, configuration.Core))
        {
            if (!values.TryGetValue(Field(role), out var path))
                continue;
            result.RemoveAll(item => item.Category == role);
            if (!string.IsNullOrWhiteSpace(path))
                result.Add(new(role, path, IsRequired(configuration.Model, configuration.Core, role), IsOriginalFirmware: false));
        }

        return result;
    }

    internal static ValueTask<EmulationFirmwareCandidate> ScanFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken)
    {
        var model = configuration.Model;
        var core = configuration.Core;
        if (!ConfigurationFunctions.IsCompatible(model, core))
            throw new ArgumentOutOfRangeException(nameof(core));
        cancellationToken.ThrowIfCancellationRequested();
        var absolute = Path.GetFullPath(path);
        return core != Emulator.None && EmulatorCatalog.CreateAdapter(core)is IEmulatorFirmwareAdapter adapter 
            ? adapter.ScanFirmwareFileAsync(configuration, absolute, cancellationToken)
            : ValueTask.FromResult(new EmulationFirmwareCandidate(absolute, absolute, Path.GetFileName(absolute), null, EmulationFirmwareCompatibility.Unknown));
    }

    internal static FirmwareConfiguration Select(EmulationFirmwareCandidate candidate, MachineModel model, Emulator core)
    {
        if (candidate.Compatibility is not EmulationFirmwareCompatibility.Official and not EmulationFirmwareCompatibility.Compatible and not EmulationFirmwareCompatibility.PartiallyCompatible || !IsField(candidate.DestinationFieldId ?? string.Empty))
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        var roles = Roles(model, core).Where(role => Field(role) == candidate.DestinationFieldId).ToArray();
        if (roles.Length != 1)
            throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareCannotBeSelected);
        return new(roles[0], candidate.Path, IsRequired(model, core, roles[0]), IsOriginalFirmware: false);
    }
}