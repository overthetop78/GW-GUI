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
    internal const string MissingFirmwareResource = "Emulation.AtariError.RequiredFirmwareMissing";
    internal const string MissingFirmwareFileResource = "Emulation.AtariError.FirmwareFileMissing";
    internal const string MissingMediaFileResource = "Emulation.AtariError.MediaFileMissing";
    internal const string MissingHostExecutableResource = "Emulation.AtariError.HostExecutableMissing";
    internal const string FirmwareRoleContextKey = "firmwareRole";
    internal const string ModelContextKey = "model";
    internal const string PathContextKey = "path";
}
