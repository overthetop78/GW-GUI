namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Constants;

internal static class FirmwareConstants
{
    internal const long RomSizeBytes = 1024 * 1024;
    internal const long Fz1KanjiRomSizeBytes = 933636;
    internal const string DefaultSystemFile = "panafz1.bin";
    internal const string BuiltInSystemRom = "opera-boot-internal";
    internal const string DefaultFontFile = "panafz1-kanji.bin";
    internal static IReadOnlyList<string> SystemFiles { get; } = ["panafz1.bin", "panafz1j.bin", "panafz1j-norsa.bin", "panafz10.bin", "panafz10-norsa.bin", "panafz10e-anvil.bin", "panafz10e-anvil-norsa.bin", "goldstar.bin", "sanyotry.bin", "3do_arcade_saot.bin"];
    internal static IReadOnlyList<string> FontFiles { get; } = ["panafz1-kanji.bin", "panafz1j-kanji.bin", "panafz10ja-anvil-kanji.bin"];
}
