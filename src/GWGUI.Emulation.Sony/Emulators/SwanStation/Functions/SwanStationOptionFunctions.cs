using FirmwareConstants = GWGUI.Emulation.Sony.Emulators.SwanStation.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Functions;

internal static class SwanStationOptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(
            configuration.Options ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var region in FirmwareConstants.Regions.Values)
        {
            var fileName = HasSelection(region.FieldId) ? region.FileName
                : HasSelection(FirmwareConstants.PspField) ? FirmwareConstants.PspFile
                : HasSelection(FirmwareConstants.Ps3Field) ? FirmwareConstants.Ps3File
                : region.FileName;
            options[region.PathKey] = fileName;
        }
        return configuration with { Options = options };

        bool HasSelection(string fieldId) =>
            !string.IsNullOrWhiteSpace(configuration.FirmwarePaths?.GetValueOrDefault(fieldId));
    }
}


