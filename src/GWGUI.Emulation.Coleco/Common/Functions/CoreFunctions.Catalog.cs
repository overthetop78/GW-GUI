using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation;
using GWGUI.Emulation.Coleco.Emulators.Common.Constants;
using GWGUI.Emulation.Coleco.Emulators.Common.Contracts;
using GWGUI.Emulation.Coleco.Emulators.Common.Exceptions;
using System.IO;
namespace GWGUI.Emulation.Coleco.Common.Functions;

internal static class EmulatorCatalogFunctions
{
    /// <summary>Fournit le contrat commun de diagnostic des bibliothèques de cœur.</summary>
    /// <value>Service emprunté du SDK, disponible pour tout module par son interface publique.</value>
    /// <remarks>Le service inspecte les fichiers sans charger ni démarrer le cœur et ferme ses flux de lecture. Les chemins, identités et callbacks de validation restent fournis par le module ; aucune identité de module connue du SDK n’est nécessaire.</remarks>
    internal static IEmulationCoreDiagnostics Diagnostics => EmulationSdk.CoreDiagnostics;

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
        return CoreReleaseFunctions.Releases.CreateInstallationPaths(Path.GetFullPath(installationRoot), entry.Id,
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
