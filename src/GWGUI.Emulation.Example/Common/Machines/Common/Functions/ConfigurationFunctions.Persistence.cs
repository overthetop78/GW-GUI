using System.IO;
using System.Text.Json;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Projection du document commun et résolution des chemins de configuration.</summary>
/// <remarks>Le format de données et l’écriture atomique utilisent les contrats du SDK. Le module conserve
/// la responsabilité de la validité de son modèle et de ses choix de lecteurs.</remarks>
internal static class ConfigurationStoreFunctions
{
    /// <summary>Conventions JSON communes au chargement et à l’enregistrement du document du module.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = ConfigurationStoreConstants.WriteIndentedJson };

    /// <summary>Désérialise l’élément prêté par le lecteur de fichiers du SDK.</summary>
    /// <param name="root">Racine JSON valide uniquement pendant l’appel.</param>
    /// <returns>Document indépendant de la durée de vie du lecteur JSON.</returns>
    internal static ConfigurationDocument Deserialize(JsonElement root) => root.Deserialize<ConfigurationDocument>(JsonOptions) ?? throw new InvalidDataException();

    /// <summary>Produit le document de stockage en convertissant les seuls chemins de contenus et de dossiers.</summary>
    /// <param name="configuration">Configuration validée à enregistrer.</param>
    /// <param name="pathBase">Racine des chemins relatifs, fournie par le propriétaire du magasin.</param>
    /// <returns>Document avec identités et associations inchangées.</returns>
    internal static ConfigurationDocument ToDocument(MachineConfiguration configuration, string pathBase) => new(
        configuration.SchemaVersion, configuration.Id, configuration.Model, configuration.Core,
        configuration.Firmwares.Select(item => item with { Path = EmulationSdk.ConfigurationFiles.StorePath(item.Path, pathBase)! }).ToArray(),
        configuration.Media.Select(item => item with { Path = EmulationSdk.ConfigurationFiles.StorePath(item.Path, pathBase)! }).ToArray(),
        configuration.Options, configuration.Input, StoreFolders(configuration.Folders, pathBase), configuration.AudioEnabled);

    /// <summary>Contrôle le document avant de reconstruire sa configuration et de résoudre ses chemins.</summary>
    /// <param name="document">Document désérialisé, dont les identités n’ont pas encore été validées.</param>
    /// <param name="pathBase">Racine empruntée de résolution des chemins relatifs.</param>
    /// <returns>Configuration restaurée et validée, sans démarrer de cœur.</returns>
    internal static MachineConfiguration FromDocument(ConfigurationDocument document, string pathBase)
    {
        if (document.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion || document.Id == Guid.Empty
            || document.Firmwares is null || document.Media is null || document.Options is null || document.Input is null || document.Folders is null)
            throw new InvalidDataException();
        var result = new MachineConfiguration(document.Model,
            document.Firmwares.Select(item => item with { Path = EmulationSdk.ConfigurationFiles.ResolvePath(item.Path, pathBase)! }).ToArray(),
            document.Media.Select(item => item with { Path = EmulationSdk.ConfigurationFiles.ResolvePath(item.Path, pathBase)! }).ToArray(),
            document.Options, document.Input, document.Id, document.SchemaVersion, document.AudioEnabled, ResolveFolders(document.Folders, pathBase), document.Core);
        ConfigurationFunctions.Validate(result);
        return result;
    }

    /// <summary>Écrit le document par le service d’écriture atomique existant.</summary>
    /// <param name="path">Destination possédée par le magasin de configurations.</param>
    /// <param name="document">Document complet dont les chemins sont déjà convertis pour le stockage.</param>
    /// <param name="cancellationToken">Signal d’annulation de l’écriture.</param>
    /// <returns>Tâche terminée après publication atomique et nettoyage du fichier temporaire.</returns>
    internal static Task WriteDocumentAtomicallyAsync(string path, ConfigurationDocument document, CancellationToken cancellationToken) => EmulationSdk.ConfigurationFiles.WriteDocumentAsync(path, document, JsonOptions, cancellationToken);

    /// <summary>Convertit les dossiers personnels vers leur représentation de stockage.</summary>
    /// <param name="folders">Dossiers absolus de la configuration.</param>
    /// <param name="pathBase">Racine relative du magasin.</param>
    /// <returns>Descriptions de dossiers sans création ni déplacement de leurs contenus.</returns>
    private static FolderConfiguration StoreFolders(FolderConfiguration folders, string pathBase) => new(
        EmulationSdk.ConfigurationFiles.StorePath(folders.Shared, pathBase), EmulationSdk.ConfigurationFiles.StorePath(folders.Floppies, pathBase),
        EmulationSdk.ConfigurationFiles.StorePath(folders.States, pathBase), EmulationSdk.ConfigurationFiles.StorePath(folders.Captures, pathBase));

    /// <summary>Résout les dossiers personnels d’un document depuis la racine du magasin.</summary>
    /// <param name="folders">Chemins enregistrés dans le document.</param>
    /// <param name="pathBase">Racine relative du magasin.</param>
    /// <returns>Descriptions absolues ; aucun dossier n’est ouvert ou créé.</returns>
    private static FolderConfiguration ResolveFolders(FolderConfiguration folders, string pathBase) => new(
        EmulationSdk.ConfigurationFiles.ResolvePath(folders.Shared, pathBase), EmulationSdk.ConfigurationFiles.ResolvePath(folders.Floppies, pathBase),
        EmulationSdk.ConfigurationFiles.ResolvePath(folders.States, pathBase), EmulationSdk.ConfigurationFiles.ResolvePath(folders.Captures, pathBase));
}
