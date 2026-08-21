using System.Windows.Media;

namespace GWGUI.App.Constants;

internal static class EmulationCoreManagementConstants
{
    internal const string SearchResource = "Emulation.Core.NameSearch";
    internal const string DownloadResource = "Emulation.Core.NameDownload";
    internal const string CancelResource = "Common.Cancel";
    internal const string CancelledResource = "Operation.Cancelled";
    internal const string SearchPromptResource = "Emulation.Core.NameSearchPrompt";
    internal const string SearchingResource = "Emulation.Core.NameSearching";
    internal const string VersionsFoundResource = "Emulation.Core.NameVersionsFound";
    internal const string NoneFoundResource = "Emulation.Core.NameNoneFound";
    internal const string DownloadingResource = "Emulation.Core.NameDownloading";
    internal const string InstalledPathResource = "Emulation.Core.NameInstalledPath";
    internal const string NotInstalledResource = "Emulation.Core.NameNotInstalled";
    internal const string InstalledResource = "Emulation.Core.NameInstalled";
    internal const string EmulatorResource = "Emulation.Core.Emulator";
    internal const string ProjectVersionResource = "Emulation.Core.NameProjectVersion";
    internal const string RequiredVersionResource = "Emulation.Core.NameRequiredVersion";
    internal const string LatestVersionResource = "Emulation.Core.NameLatestVersion";
    internal const string VersionsControlName = "CoreVersions";
    internal const string SearchControlName = "SearchCoreVersions";
    internal const string DownloadControlName = "DownloadCoreVersion";
    internal const string CancelControlName = "CancelCoreDownload";
    internal const string ProgressControlName = "CoreDownloadProgress";
    internal const string StatusControlName = "CoreStatus";
    internal const string SearchGlyph = "\uE721";
    internal const string DownloadGlyph = "\uE896";
    internal const double InitialProgress = 0D;
    internal const double CompletedProgress = 1D;
    internal static readonly Color ErrorBackground = Color.FromRgb(255, 241, 241);
    internal static readonly Color ErrorBorder = Color.FromRgb(210, 75, 75);
    internal static readonly Color ErrorText = Color.FromRgb(150, 25, 25);
}
