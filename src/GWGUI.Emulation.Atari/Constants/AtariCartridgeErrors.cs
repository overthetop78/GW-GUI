namespace GWGUI.Emulation.Atari.Constants;

internal static class AtariCartridgeErrors
{
    internal const string UnsupportedCore = "The selected Atari core does not use the shared cartridge controller.";
    internal const string CartridgeRequired = "The selected Atari machine requires cartridge media.";
    internal const string ExtensionUnsupported = "The cartridge extension is not supported by the selected Atari core.";
    internal const string FileUnreadable = "The Atari cartridge cannot be opened for reading.";
    internal const string ReplacementFailed = "The Atari core rejected the replacement cartridge.";
    internal const string RollbackFailed = "The Atari core rejected both the replacement and the previous cartridge.";
    internal const string EjectionUnsupported = "This Atari core cannot remain powered on without a cartridge.";
    internal const string RegionUnsupported = "The selected Atari core does not expose cartridge region selection.";
    internal const string SecamUnsupported = "The selected Atari core does not expose SECAM cartridge timing.";
}
