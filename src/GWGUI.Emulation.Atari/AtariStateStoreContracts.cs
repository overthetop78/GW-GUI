namespace GWGUI.Emulation.Atari;

public enum AtariStoredStateKind
{
    Quick,
    Named
}

public sealed record AtariStoredStateMetadata(
    string Name,
    AtariStoredStateKind Kind,
    DateTimeOffset CreatedAtUtc,
    string StateFileName,
    string? CaptureFileName,
    AtariCoreKind Core,
    string CoreName,
    string CoreVersion,
    AtariMachineModel Model,
    string ConfigurationSha256,
    string ContentSha256);
