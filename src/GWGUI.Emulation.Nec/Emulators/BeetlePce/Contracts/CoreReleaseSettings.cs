namespace GWGUI.Emulation.Nec.Emulators.BeetlePce.Contracts;

internal sealed record CoreReleaseSettings(
    Uri OfficialArchiveUri,
    string ArchiveLibraryName,
    string InstalledLibraryName,
    string DisplayName);
