using System.IO;
namespace GWGUI.Emulation.Snk.Emulators.Common.Functions;

internal static class EmulatorCatalogFunctions
{
    internal static EmulatorCatalogEntry Create(Emulator emulator, string id, string libraryName, string dllName, Uri archiveUri, string source, string revision, params MachineModel[] models)
    {
        var archiveName = dllName + EmulatorCatalogConstants.ArchiveExtension;
        return new EmulatorCatalogEntry(emulator, id, libraryName, dllName, archiveName, archiveUri, new Uri(source, UriKind.Absolute), revision, models.ToHashSet());
    }

    internal static CoreInstallationPaths GetInstallationPaths(EmulatorCatalogEntry entry, string installationRoot, string version)
    {
        if (string.IsNullOrWhiteSpace(installationRoot)) throw new ArgumentException(EmulatorCatalogErrors.EmptyInstallationRoot, nameof(installationRoot));
        if (string.IsNullOrWhiteSpace(version)) throw new ArgumentException(EmulatorCatalogErrors.EmptyVersion, nameof(version));
        var versionDirectory = Path.Combine(Path.GetFullPath(installationRoot), entry.Id, version);
        return new CoreInstallationPaths(versionDirectory, Path.Combine(versionDirectory, entry.DllName), Path.Combine(versionDirectory, EmulatorCatalogConstants.ManifestFileName));
    }

    internal static string GetActiveManifestPath(EmulatorCatalogEntry entry, string installationRoot)
    {
        if (string.IsNullOrWhiteSpace(installationRoot)) throw new ArgumentException(EmulatorCatalogErrors.EmptyInstallationRoot, nameof(installationRoot));
        return Path.Combine(Path.GetFullPath(installationRoot), entry.Id, EmulatorCatalogConstants.ActiveManifestFileName);
    }

}
