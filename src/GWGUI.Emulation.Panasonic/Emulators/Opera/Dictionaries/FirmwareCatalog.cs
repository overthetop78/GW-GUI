using NativeFirmwareConstants = GWGUI.Emulation.Panasonic.Emulators.Opera.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Dictionaries;

internal static class FirmwareCatalog
{
    internal static IReadOnlyList<FirmwareRequirement> Requirements(MachineModel model)
    {
        if (model != MachineModel.ThreeDO)
            throw new ArgumentOutOfRangeException(nameof(model));
        return[
            new(FirmwareRole.SystemRom, FirmwareFunctions.Field(FirmwareRole.SystemRom), "Emulation.Firmware.Rom.System", NativeFirmwareConstants.SystemFiles, true),
            new(FirmwareRole.FontRom, FirmwareFunctions.Field(FirmwareRole.FontRom), "Emulation.Panasonic.Firmware.Font", NativeFirmwareConstants.FontFiles, false)
        ];
    }
}
