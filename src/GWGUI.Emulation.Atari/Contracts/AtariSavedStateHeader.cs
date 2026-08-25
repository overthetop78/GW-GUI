namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariSavedStateHeader(
    int FormatVersion,
    AtariEmulator Core,
    string CoreName,
    string CoreVersion,
    string CoreSha256,
    AtariMachineModel Model,
    string ConfigurationSha256,
    string ContentSha256,
    string StateSha256);
