namespace GWGUI.Emulation.Atari;

public sealed record AtariScannedFirmware(
    string Path, long? SizeBytes, string? Md5, AtariFirmwareDetectionStatus Detection,
    AtariFirmwareDefinition? Definition, AtariFirmwareCompatibility Compatibility,
    bool IsDuplicate, string? ReadError);
