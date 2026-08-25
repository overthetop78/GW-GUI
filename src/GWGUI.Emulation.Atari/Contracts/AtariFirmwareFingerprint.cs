namespace GWGUI.Emulation.Atari.Contracts;

public sealed record AtariFirmwareFingerprint(
    AtariFirmwareHashAlgorithm Algorithm, string Value, AtariStRegion? Region = null);
