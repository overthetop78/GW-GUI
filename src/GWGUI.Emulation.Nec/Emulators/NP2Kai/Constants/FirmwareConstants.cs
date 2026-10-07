namespace GWGUI.Emulation.Nec.Emulators.NP2Kai.Constants;

internal static class FirmwareConstants
{
    internal const string SystemSubdirectory = "np2kai";
    internal static IReadOnlyList<FirmwareSlot> Slots { get; } =
    [
        new("configuration.firmware.font.bmp", "font.bmp", "Emulation.Nec.Firmware.FontBitmap", SystemSubdirectory, ["font.bmp"], null),
        new("configuration.firmware.FONT.ROM", "FONT.ROM", "Emulation.Nec.Firmware.Pc98Font", SystemSubdirectory, ["FONT.ROM"], null),
        new("configuration.firmware.bios.rom", "bios.rom", "Emulation.Nec.Firmware.Bios", SystemSubdirectory, ["bios.rom"], null),
        new("configuration.firmware.itf.rom", "itf.rom", "Emulation.Nec.Firmware.Interface", SystemSubdirectory, ["itf.rom"], null),
        new("configuration.firmware.sound.rom", "sound.rom", "Emulation.Nec.Firmware.Sound", SystemSubdirectory, ["sound.rom"], null),
        new("configuration.firmware.2608_BD.WAV", "2608_BD.WAV", "Emulation.Nec.Firmware.BassDrum", SystemSubdirectory, ["2608_BD.WAV"], null),
        new("configuration.firmware.2608_SD.WAV", "2608_SD.WAV", "Emulation.Nec.Firmware.SnareDrum", SystemSubdirectory, ["2608_SD.WAV"], null),
        new("configuration.firmware.2608_TOP.WAV", "2608_TOP.WAV", "Emulation.Nec.Firmware.Cymbal", SystemSubdirectory, ["2608_TOP.WAV"], null),
        new("configuration.firmware.2608_HH.WAV", "2608_HH.WAV", "Emulation.Nec.Firmware.HiHat", SystemSubdirectory, ["2608_HH.WAV"], null),
        new("configuration.firmware.2608_TOM.WAV", "2608_TOM.WAV", "Emulation.Nec.Firmware.Tom", SystemSubdirectory, ["2608_TOM.WAV"], null),
        new("configuration.firmware.2608_RIM.WAV", "2608_RIM.WAV", "Emulation.Nec.Firmware.Rimshot", SystemSubdirectory, ["2608_RIM.WAV"], null),
    ];
}
