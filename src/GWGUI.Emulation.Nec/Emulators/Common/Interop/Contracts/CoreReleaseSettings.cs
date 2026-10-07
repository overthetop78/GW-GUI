namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

internal sealed record CoreReleaseSettings(
    Uri OfficialArchiveUri,
    string ArchiveLibraryName,
    string InstalledLibraryName,
    string DisplayName);
