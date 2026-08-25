namespace GWGUI.Emulation.Atari.Contracts;

public sealed record AtariCoreRelease(
    AtariEmulator Emulator,
    string Id,
    string DeclaredVersion,
    Uri DownloadUri,
    DateTimeOffset PublishedUtc,
    long? ExpectedArchiveSize);
