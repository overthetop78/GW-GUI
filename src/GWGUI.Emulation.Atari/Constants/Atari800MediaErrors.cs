namespace GWGUI.Emulation.Atari.Constants;

internal static class Atari800MediaErrors
{
    internal const string UnsupportedMediaCategory = "Atari800 accepts floppy, cassette and cartridge media only.";
    internal const string InvalidExtension = "The content extension does not match the configured Atari800 media type.";
    internal const string ComputerMediaOn5200 = "Atari 8-bit computer media cannot be mounted on an Atari 5200.";
    internal const string ConsoleMediaOnComputer = "Atari 5200 cartridge media cannot be mounted on an Atari 8-bit computer.";
    internal const string CartridgeTypeInvalid = "The Atari cartridge type identifier must be positive.";
    internal const string DynamicCartridgeUnsupported = "Atari800 cartridge replacement requires a clean core restart.";
    internal const string MediaControlRequired = "Atari800 did not expose its disk and cassette control interface.";
}
