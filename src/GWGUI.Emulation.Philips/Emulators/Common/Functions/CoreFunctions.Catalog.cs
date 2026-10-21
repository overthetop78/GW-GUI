using GWGUI.Emulation.Philips.Emulators.Common.Constants;
using GWGUI.Emulation.Philips.Emulators.Common.Contracts;
using GWGUI.Emulation.Philips.Emulators.Common.Exceptions;
using System.IO;
namespace GWGUI.Emulation.Philips.Emulators.Common.Functions;

internal static class EmulatorCatalogFunctions
{
    internal static EmulatorCatalogEntry Create(Emulator emulator, string id, string libraryName, string dllName, string source, string revision, params MachineModel[] models)
    {
        var archiveName = dllName + EmulatorCatalogConstants.ArchiveExtension;
        return new EmulatorCatalogEntry(emulator, id, libraryName, dllName, archiveName, new Uri(EmulatorCatalogConstants.BuildServerRoot + archiveName, UriKind.Absolute), new Uri(source, UriKind.Absolute), revision, models.ToHashSet());
    }

    internal static CoreInstallationPaths GetInstallationPaths(EmulatorCatalogEntry entry, string installationRoot, string version)
    {
        if (string.IsNullOrWhiteSpace(installationRoot))
            throw new ArgumentException(EmulatorCatalogErrors.EmptyInstallationRoot, nameof(installationRoot));
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException(EmulatorCatalogErrors.EmptyVersion, nameof(version));
        return global::GWGUI.Emulation.EmulationSdk.CoreReleases.CreateInstallationPaths(Path.GetFullPath(installationRoot), entry.Id,
            version, entry.DllName, EmulatorCatalogConstants.ManifestFileName);
    }

    internal static string GetActiveManifestPath(EmulatorCatalogEntry entry, string installationRoot)
    {
        if (string.IsNullOrWhiteSpace(installationRoot))
            throw new ArgumentException(EmulatorCatalogErrors.EmptyInstallationRoot, nameof(installationRoot));
        return Path.Combine(Path.GetFullPath(installationRoot), entry.Id, EmulatorCatalogConstants.ActiveManifestFileName);
    }
}
