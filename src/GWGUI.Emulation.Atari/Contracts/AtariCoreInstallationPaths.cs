namespace GWGUI.Emulation.Atari.Contracts;

public sealed record AtariCoreInstallationPaths(
    string VersionDirectory,
    string LibraryPath,
    string ManifestPath);
