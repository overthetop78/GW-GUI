namespace GWGUI.Emulation.Atari;

public sealed record AtariFirmwareFingerprint(
    AtariFirmwareHashAlgorithm Algorithm, string Value, AtariStRegion? Region = null);
