namespace GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants;

internal static class FirmwareConstants
{
    internal const string SystemSubdirectory = "np2";
    internal static IReadOnlyList<FirmwareSlot> Slots { get; } =
    [
        new("configuration.firmware.font.bmp", "font.bmp", "Emulation.Nec.Firmware.FontBitmap", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["font.bmp"], KnownMd5: null),
        new("configuration.firmware.FONT.ROM", "FONT.ROM", "Emulation.Nec.Firmware.Pc98Font", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["FONT.ROM"], KnownMd5: null),
        new("configuration.firmware.bios.rom", "bios.rom", "Emulation.Nec.Firmware.Bios", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["bios.rom"], KnownMd5: null),
        new("configuration.firmware.itf.rom", "itf.rom", "Emulation.Nec.Firmware.Interface", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["itf.rom"], KnownMd5: null),
        new("configuration.firmware.sound.rom", "sound.rom", "Emulation.Nec.Firmware.Sound", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["sound.rom"], KnownMd5: null),
        new("configuration.firmware.2608_BD.WAV", "2608_BD.WAV", "Emulation.Nec.Firmware.BassDrum", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["2608_BD.WAV"], KnownMd5: null),
        new("configuration.firmware.2608_SD.WAV", "2608_SD.WAV", "Emulation.Nec.Firmware.SnareDrum", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["2608_SD.WAV"], KnownMd5: null),
        new("configuration.firmware.2608_TOP.WAV", "2608_TOP.WAV", "Emulation.Nec.Firmware.Cymbal", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["2608_TOP.WAV"], KnownMd5: null),
        new("configuration.firmware.2608_HH.WAV", "2608_HH.WAV", "Emulation.Nec.Firmware.HiHat", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["2608_HH.WAV"], KnownMd5: null),
        new("configuration.firmware.2608_TOM.WAV", "2608_TOM.WAV", "Emulation.Nec.Firmware.Tom", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["2608_TOM.WAV"], KnownMd5: null),
        new("configuration.firmware.2608_RIM.WAV", "2608_RIM.WAV", "Emulation.Nec.Firmware.Rimshot", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["2608_RIM.WAV"], KnownMd5: null),
    ];
}
