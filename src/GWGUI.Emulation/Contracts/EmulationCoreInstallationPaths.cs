namespace GWGUI.Emulation.Contracts;

public sealed record EmulationCoreInstallationPaths(
    string VersionDirectory,
    string LibraryPath,
    string ManifestPath);
