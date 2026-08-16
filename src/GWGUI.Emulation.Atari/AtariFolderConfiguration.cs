namespace GWGUI.Emulation.Atari;

public sealed record AtariFolderConfiguration(
    string? Shared = null,
    string? Floppies = null,
    string? Cassettes = null,
    string? Cartridges = null,
    string? CompactDiscs = null,
    string? HardDisks = null,
    string? States = null,
    string? Captures = null);
