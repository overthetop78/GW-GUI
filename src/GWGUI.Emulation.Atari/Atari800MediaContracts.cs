namespace GWGUI.Emulation.Atari;

internal enum Atari800ContentType
{
    Floppy,
    Cassette,
    ComputerCartridge,
    ConsoleCartridge
}

internal sealed record Atari800PreparedMedia(
    AtariMediaConfiguration Configuration,
    Atari800ContentType ContentType,
    string RuntimePath,
    AtariSessionMedia? SessionMedia);
