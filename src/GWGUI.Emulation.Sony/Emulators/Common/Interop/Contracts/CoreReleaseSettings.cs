namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;

internal sealed record CoreReleaseSettings(
    Uri OfficialArchiveUri,
    string ArchiveLibraryName,
    string InstalledLibraryName,
    string DisplayName);
