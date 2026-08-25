namespace GWGUI.Emulation.Atari.Contracts;


public sealed record AtariStoredStateMetadata(
    string Name,
    AtariStoredStateCategory Category,
    DateTimeOffset CreatedAtUtc,
    string StateFileName,
    string? CaptureFileName,
    AtariEmulator Core,
    string CoreName,
    string CoreVersion,
    AtariMachineModel Model,
    string ConfigurationSha256,
    string ContentSha256);
