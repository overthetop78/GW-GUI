namespace GWGUI.Emulation.Amstrad.Emulators.Common.Contracts;

internal sealed record CoreDefinition(string EmulatorId, string LibraryName, string LibraryFile,
    string HostCommand, Uri DownloadUri, string LatestReleaseDisplayName,
    string ReleaseProviderSuffix, string Architecture, Action<string>? VerifyInstallation = null);
