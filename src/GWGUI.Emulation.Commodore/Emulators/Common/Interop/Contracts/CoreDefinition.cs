namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Contracts;

internal sealed record CoreDefinition(string LibraryName, string LibraryFile,
    string? DownloadUrl, CoreRelease? RequiredRelease = null);
