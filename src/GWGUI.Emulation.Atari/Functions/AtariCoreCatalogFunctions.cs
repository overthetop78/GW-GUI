namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariCoreCatalogFunctions
{
    internal static AtariCoreCatalogEntry Create(AtariEmulator emulator, string id, string libraryName,
        string dllName, string source, string revision, params AtariMachineModel[] models)
    {
        var archiveName = dllName + AtariCoreCatalogConstants.ArchiveExtension;
        return new AtariCoreCatalogEntry(emulator, id, libraryName, dllName, archiveName,
            new Uri(AtariCoreCatalogConstants.BuildServerRoot + archiveName, UriKind.Absolute),
            new Uri(source, UriKind.Absolute), revision, models.ToHashSet());
    }

    internal static AtariCoreInstallationPaths GetInstallationPaths(AtariCoreCatalogEntry entry,
        string installationRoot, string version)
    {
        if (string.IsNullOrWhiteSpace(installationRoot))
            throw new ArgumentException(AtariCoreCatalogErrors.EmptyInstallationRoot, nameof(installationRoot));
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException(AtariCoreCatalogErrors.EmptyVersion, nameof(version));
        var versionDirectory = Path.Combine(Path.GetFullPath(installationRoot), entry.Id, version);
        return new AtariCoreInstallationPaths(versionDirectory,
            Path.Combine(versionDirectory, entry.DllName),
            Path.Combine(versionDirectory, AtariCoreCatalogConstants.ManifestFileName));
    }

    internal static string GetActiveManifestPath(AtariCoreCatalogEntry entry, string installationRoot)
    {
        if (string.IsNullOrWhiteSpace(installationRoot))
            throw new ArgumentException(AtariCoreCatalogErrors.EmptyInstallationRoot, nameof(installationRoot));
        return Path.Combine(Path.GetFullPath(installationRoot), entry.Id,
            AtariCoreCatalogConstants.ActiveManifestFileName);
    }

    internal static IReadOnlyDictionary<AtariMachineModel, AtariEmulator> CreateModelAssociations(
        IReadOnlyList<AtariCoreCatalogEntry> entries)
    {
        if (entries.Select(entry => entry.Emulator).Distinct().Count() != entries.Count
            || entries.Select(entry => entry.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != entries.Count)
            throw new InvalidDataException(AtariCoreCatalogErrors.DuplicateCore);

        var result = new Dictionary<AtariMachineModel, AtariEmulator>();
        foreach (var entry in entries)
        {
            foreach (var model in entry.Models)
            {
                if (!result.TryAdd(model, entry.Emulator))
                    throw new InvalidDataException(AtariCoreCatalogErrors.DuplicateModel);
            }
        }
        if (Enum.GetValues<AtariMachineModel>().Any(model => !result.ContainsKey(model)))
            throw new InvalidDataException(AtariCoreCatalogErrors.MissingModel);
        return result;
    }
}
