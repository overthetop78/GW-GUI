using GWGUI.Emulation.Acorn.Emulators.Common.Exceptions;
using GWGUI.Emulation.Acorn.Emulators.Common.Contracts;
using GWGUI.Emulation.Acorn.Emulators.Common.Constants;
using System.IO;
namespace GWGUI.Emulation.Acorn.Emulators.Common.Functions;

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

    internal static IReadOnlyDictionary<MachineModel, Emulator> CreateModelAssociations(IReadOnlyList<EmulatorCatalogEntry> entries)
    {
        if (entries.Select(entry => entry.Emulator).Distinct().Count() != entries.Count
            || entries.Select(entry => entry.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != entries.Count)
            throw new InvalidDataException(EmulatorCatalogErrors.DuplicateCore);

        var result = new Dictionary<MachineModel, Emulator>();
        foreach (var entry in entries)
        {
            foreach (var model in entry.Models)
            {
                result.TryAdd(model, entry.Emulator);
            }
        }

        return result;
    }
}

