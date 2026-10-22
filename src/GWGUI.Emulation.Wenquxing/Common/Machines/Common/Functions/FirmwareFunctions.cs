using CommonCoreHostFunctions = GWGUI.Emulation.Wenquxing.Common.Functions.CoreHostFunctions;
using System.IO;

namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;
internal static class FirmwareFunctions
{
    internal static string Field(FirmwareRole role) => FirmwareConstants.FieldPrefix + role;
    private static IEmulatorFirmwareCapabilities? Capabilities(MachineConfiguration configuration)
    {
        var engine = new Engine();
        return configuration.Core == Emulator.None ? engine.Adapters(configuration.MachineId).OfType<IEmulatorFirmwareCapabilities>().FirstOrDefault() : engine.Adapter(configuration) as IEmulatorFirmwareCapabilities;
    }

    internal static IReadOnlyList<FirmwareRequirement> Requirements(MachineConfiguration configuration) => Capabilities(configuration)?.DescribeFirmware(configuration) ?? [];
    internal static EmulationSettingsBlock Describe(MachineConfiguration configuration) => new(
        FirmwareConstants.BlockId,
        EmulationMachineTab.Rom,
        FirmwareConstants.TitleResourceKey,
        Requirements(configuration).Select(requirement => new EmulationSettingsField(requirement.FieldId, EmulationMachineTab.Rom, FirmwareConstants.BlockId, requirement.DisplayResourceKey, EmulationSettingsEditor.Path, configuration.Firmware.Images.GetValueOrDefault(requirement.Role))).ToArray(),
        IsCompact: true
    );
    internal static FirmwareConfiguration Apply(MachineConfiguration configuration, IReadOnlyDictionary<string, string?> values)
    {
        var selected = new Dictionary<FirmwareRole, string>(configuration.Firmware.Images);
        foreach (var requirement in Requirements(configuration))
        {
            if (!values.TryGetValue(requirement.FieldId, out var path))
                continue;
            selected.Remove(requirement.Role);
            if (!string.IsNullOrWhiteSpace(path))
                selected[requirement.Role] = Path.GetFullPath(path);
        }

        return new(selected);
    }

    internal static ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanAsync(string directory, MachineConfiguration configuration, CancellationToken cancellationToken)
    {
        var result = new List<EmulationFirmwareCandidate>();
        var capabilities = Capabilities(configuration);
        if (capabilities is null)
            return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>(result);
        var requirements = capabilities.DescribeFirmware(configuration);
        CommonCoreHostFunctions.FirmwareProcessing.ScanRecursive(directory, path =>
        {
            var matches = requirements.Where(requirement => capabilities.IsFirmwareCompatible(configuration, requirement.Role, path)).ToArray();
            if (matches.Length == 0)
                result.Add(new(path, path, Path.GetFileName(path), null, EmulationFirmwareCompatibility.Unknown));
            foreach (var requirement in matches)
                result.Add(new(requirement.Role + ":" + path, path, Path.GetFileName(path), null, EmulationFirmwareCompatibility.Compatible, requirement.FieldId));
        }, cancellationToken);

        return ValueTask.FromResult<IReadOnlyList<EmulationFirmwareCandidate>>(result);
    }

    internal static MachineConfiguration Select(MachineConfiguration configuration, EmulationFirmwareCandidate candidate)
    {
        var requirement = Requirements(configuration).SingleOrDefault(item => item.FieldId == candidate.DestinationFieldId) ?? throw new ArgumentException(nameof(candidate));
        var selected = new Dictionary<FirmwareRole, string>(configuration.Firmware.Images)
        {
            [requirement.Role] = Path.GetFullPath(candidate.Path)
        };
        var result = configuration with
        {
            Firmware = new(selected)
        };
        ConfigurationFunctions.Validate(result);
        return result;
    }
}
