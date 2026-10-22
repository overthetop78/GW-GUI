using NativeFirmwareConstants = GWGUI.Emulation.Panasonic.Emulators.Opera.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Dictionaries;
internal static class FirmwareCatalog
{
    internal static IReadOnlyList<FirmwareRequirement> Requirements(MachineModel model)
    {
        if (model != MachineModel.ThreeDO)
            throw new ArgumentOutOfRangeException(nameof(model));
        return[
            new(FirmwareCategory.SystemRom, FirmwareFunctions.Field(FirmwareCategory.SystemRom), "Emulation.Firmware.Rom.System", NativeFirmwareConstants.SystemFiles, false),
            new(FirmwareCategory.FontRom, FirmwareFunctions.Field(FirmwareCategory.FontRom), "Emulation.Panasonic.Firmware.Font", NativeFirmwareConstants.FontFiles, false)
        ];
    }
}
