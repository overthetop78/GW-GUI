namespace GWGUI.App.Controls;

internal static class AtariEmulationConstants
{
    internal const int DisplayIdentifierLength = 8;
    internal const string IdentifierFormat = "N";
    internal const string AtariTitle = "Atari";
    internal const string AmigaTitle = "Amiga";
    internal const string HomeGlyph = "\uE80F";
    internal const string CardStyleResource = "Card";
    internal const string MainTabItemStyleResource = "MainTabItemStyle";
    internal const string StatusIconButtonStyleResource = "StatusIconButton";
    internal const string ConfigurationResource = "Emulation.Configuration";
    internal const string OpenResource = "Emulation.OpenMachine";
    internal const string MachinesAutomationResource = "Emulation.MachinesTab";
    internal const string WelcomeResource = "Emulation.WelcomeText";
    internal const string WelcomeTabResource = "Emulation.WelcomeTab";
    internal const string CloseResource = "Common.Close";
    internal const string StartingResource = "Status.ReadyShort";
    internal const string RunningResource = "Status.Running";
    internal const string StoppedResource = "Status.ReadyShort";
    internal const string ConfigurationOpeningContext = "Opening Atari configuration";
    internal const string MissingFirmwareFormat = "Required Atari firmware '{0}' is not configured for {1}.";
    internal const string MissingFirmwareFileFormat = "Configured Atari firmware '{0}' was not found: {1}";
    internal const string MissingMediaFileFormat = "Configured Atari media was not found: {0}";
    internal const string MissingHostExecutable = "The Atari host executable path is unavailable.";
}
