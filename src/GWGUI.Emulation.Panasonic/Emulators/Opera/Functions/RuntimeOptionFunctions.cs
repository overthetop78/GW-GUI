using System.Globalization;
using GWGUI.Emulation.Panasonic.Emulators.Opera.Constants;
using NativeFirmwareConstants = GWGUI.Emulation.Panasonic.Emulators.Opera.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Functions;
internal static class RuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        ConfigurationFunctions.Validate(configuration);
        var values = configuration.Options.Where(option => option.Key.StartsWith(RuntimeOptionConstants.NativePrefix, StringComparison.Ordinal)).ToDictionary(option => option.Key, option => option.Value, StringComparer.Ordinal);
        values[RuntimeOptionConstants.Bios] = configuration.Firmware.Images.ContainsKey(FirmwareCategory.SystemRom)
            ? FirmwareRuntimeFunctions.FileName(configuration, FirmwareCategory.SystemRom)
            : NativeFirmwareConstants.BuiltInSystemRom;
        values[RuntimeOptionConstants.Font] = configuration.Firmware.Images.ContainsKey(FirmwareCategory.FontRom) ? FirmwareRuntimeFunctions.FileName(configuration, FirmwareCategory.FontRom) : RuntimeOptionConstants.NoFont;
        if (!values.ContainsKey(RuntimeOptionConstants.ActiveDevices))
            values[RuntimeOptionConstants.ActiveDevices] = Enumerable.Range(ControllerConstants.FirstPort,
                ControllerConstants.PortCount).Where(port => InputSettingsFunctions.Peripheral(configuration.Input,
                port) != PeripheralCategory.None).Select(port => port + 1).DefaultIfEmpty(0).Max().ToString(CultureInfo.InvariantCulture);
        return values;
    }
}
