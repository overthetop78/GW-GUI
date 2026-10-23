using NativeFirmwareConstants = GWGUI.Emulation.Panasonic.Emulators.Opera.Constants.FirmwareConstants;

namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Dictionaries;
internal static class FirmwareCatalog
{
    internal static IReadOnlyList<FirmwareSlot> Requirements(MachineModel model)
    {
        if (model != MachineModel.ThreeDO)
            throw new ArgumentOutOfRangeException(nameof(model));
        return[
            new(FirmwareFunctions.Field(FirmwareCategory.SystemRom), NativeFirmwareConstants.SystemFiles[0], "Emulation.Firmware.Rom.System", false, CandidateFileNames: NativeFirmwareConstants.SystemFiles),
            new(FirmwareFunctions.Field(FirmwareCategory.FontRom), NativeFirmwareConstants.FontFiles[0], "Emulation.Panasonic.Firmware.Font", false, CandidateFileNames: NativeFirmwareConstants.FontFiles)
        ];
    }
}
