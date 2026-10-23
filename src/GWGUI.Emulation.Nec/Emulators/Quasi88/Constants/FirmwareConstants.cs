namespace GWGUI.Emulation.Nec.Emulators.Quasi88.Constants;

internal static class FirmwareConstants
{
    internal const string SystemSubdirectory = "quasi88";
    internal static IReadOnlyList<FirmwareSlot> Slots { get; } =
    [
        new("configuration.firmware.n88.rom", "n88.rom", "Emulation.Nec.Firmware.N88Basic", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["n88.rom"], KnownMd5: "4f984e04a99d56c4cfe36115415d6eb8"),
        new("configuration.firmware.n88_0.rom", "n88_0.rom", "Emulation.Nec.Firmware.N88ExtensionBank0", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["n88_0.rom", "n88ext0.rom"], KnownMd5: "d675a2ca186c6efcd6277b835de4c7e5"),
        new("configuration.firmware.n88_1.rom", "n88_1.rom", "Emulation.Nec.Firmware.N88ExtensionBank1", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["n88_1.rom", "n88ext1.rom"], KnownMd5: "e844534dfe5744b381444dbe61ef1b66"),
        new("configuration.firmware.n88_2.rom", "n88_2.rom", "Emulation.Nec.Firmware.N88ExtensionBank2", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["n88_2.rom", "n88ext2.rom"], KnownMd5: "6548fa45061274dee1ea8ae1e9e93910"),
        new("configuration.firmware.n88_3.rom", "n88_3.rom", "Emulation.Nec.Firmware.N88ExtensionBank3", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["n88_3.rom", "n88ext3.rom"], KnownMd5: "fc4b76a402ba501e6ba6de4b3e8b4273"),
        new("configuration.firmware.n88n.rom", "n88n.rom", "Emulation.Nec.Firmware.NBasic", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["n88n.rom", "n80.rom"], KnownMd5: "2ff07b8769367321128e03924af668a0"),
        new("configuration.firmware.disk.rom", "disk.rom", "Emulation.Nec.Firmware.DiskController", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["disk.rom", "n88sub.rom"], KnownMd5: "793f86784e5608352a5d7f03f03e0858"),
        new("configuration.firmware.n88knj1.rom", "n88knj1.rom", "Emulation.Nec.Firmware.Kanji1", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["n88knj1.rom", "kanji1.rom"], KnownMd5: "d81c6d5d7ad1a4bbbd6ae22a01257603"),
        new("configuration.firmware.n88knj2.rom", "n88knj2.rom", "Emulation.Nec.Firmware.Kanji2", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["n88knj2.rom", "kanji2.rom"], KnownMd5: null),
        new("configuration.firmware.n88jisho.rom", "n88jisho.rom", "Emulation.Nec.Firmware.Dictionary", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["n88jisho.rom", "jisyo.rom"], KnownMd5: null),
        new("configuration.firmware.font.rom", "font.rom", "Emulation.Nec.Firmware.Font", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["font.rom"], KnownMd5: null),
        new("configuration.firmware.font2.rom", "font2.rom", "Emulation.Nec.Firmware.Font2", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["font2.rom"], KnownMd5: null),
        new("configuration.firmware.font3.rom", "font3.rom", "Emulation.Nec.Firmware.Font3", SystemSubdirectory: SystemSubdirectory, CandidateFileNames: ["font3.rom"], KnownMd5: null),
    ];
}
