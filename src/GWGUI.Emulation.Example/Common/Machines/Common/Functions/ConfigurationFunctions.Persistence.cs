using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Projection de la configuration vers son document persistant et résolution des chemins enregistrés.</summary>
/// <remarks>Le document conserve l’identité, les options et les montages. Les chemins situés sous la base sont
/// enregistrés relativement à cette base ; leur résolution retrouve ensuite les emplacements utilisables.
/// Les opérations de fichier sont confiées au service partagé, sans ouvrir ni déplacer les médias référencés.</remarks>
internal static class ConfigurationStoreFunctions
{
    // Conventions JSON du document ; conserver les mêmes options lors de la lecture et de l’écriture.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = ConfigurationStoreConstants.WriteIndentedJson
    };

    /// <summary>Désérialise un élément JSON vers le document de configuration du module.</summary>
    /// <param name="root">Élément emprunté au document JSON de l’appelant, valide pendant la désérialisation.</param>
    /// <returns>Document typé ; ses contrôles de structure sont effectués lors de FromDocument.</returns>
    /// <exception cref="InvalidDataException">La valeur JSON représente null au lieu d’un document.</exception>
    internal static ConfigurationDocument Deserialize(JsonElement root) => root.Deserialize<ConfigurationDocument>(JsonOptions) ?? throw new InvalidDataException();

    /// <summary>Construit le document à écrire avec les chemins de firmwares, médias et dossiers sous leur forme de stockage.</summary>
    /// <param name="configuration">Configuration empruntée, dont les listes et le dictionnaire d’options sont projetés dans le document.</param>
    /// <param name="pathBase">Racine servant à calculer les chemins relatifs des données situées sous cette base.</param>
    /// <returns>Nouveau document de la version courante ; les associations d’entrée sont conservées par référence.</returns>
    internal static ConfigurationDocument ToDocument(MachineConfiguration configuration, string pathBase) => new(
        ConfigurationStoreConstants.CurrentSchemaVersion,
        configuration.Id,
        configuration.Model,
        configuration.Core,
        configuration.Firmwares.Select(firmware => firmware with
        {
            Path = StorePath(firmware.Path, pathBase)!
        }).ToArray(),
        configuration.Media.Select(media => media with
        {
            Path = StorePath(media.Path, pathBase)!
        }).ToArray(),
        new Dictionary<string, string>(configuration.Options),
        configuration.Input,
        StoreFolders(configuration.Folders, pathBase),
        configuration.AudioEnabled);

    /// <summary>Contrôle les champs obligatoires du document puis reconstruit une configuration avec ses chemins résolus.</summary>
    /// <param name="document">Document emprunté issu de la désérialisation.</param>
    /// <param name="pathBase">Racine utilisée pour résoudre les chemins relatifs enregistrés.</param>
    /// <returns>Configuration passant par son constructeur et sa validation Common.</returns>
    /// <exception cref="InvalidDataException">Schéma, identité, collections obligatoires ou valeurs d’énumération invalides.</exception>
    /// <remarks>Les diagnostics de validation du constructeur restent propagés à l’appelant.</remarks>
    internal static MachineConfiguration FromDocument(ConfigurationDocument document, string pathBase)
    {
        if (document.SchemaVersion != ConfigurationStoreConstants.CurrentSchemaVersion)
            throw new InvalidDataException();
        if (document.Id == Guid.Empty)
            throw new InvalidDataException();
        if (document.Firmwares is null || document.Media is null || document.Options is null
            || document.Input is null || document.Folders is null)
            throw new InvalidDataException();
        if (!Enum.IsDefined(document.Model) || !Enum.IsDefined(document.Core))
            throw new InvalidDataException();
        return new MachineConfiguration(document.Model,
            document.Firmwares.Select(firmware => firmware with
            {
                Path = ResolvePath(firmware.Path, pathBase)!
            }).ToArray(),
            document.Media.Select(media => media with
            {
                Path = ResolvePath(media.Path, pathBase)!
            }).ToArray(),
            document.Options,
            document.Input,
            document.Id,
            document.SchemaVersion,
            document.AudioEnabled,
            ResolveFolders(document.Folders, pathBase),
            document.Core);
    }

    /// <summary>Prépare un chemin relatif à la base lorsqu’il reste dans celle-ci, sinon conserve sa forme absolue.</summary>
    /// <param name="path">Chemin à enregistrer ; null, chaîne vide ou composée d’espaces sont conservés tels quels.</param>
    /// <param name="pathBase">Racine utilisée pour déterminer si le chemin peut être stocké relativement.</param>
    /// <returns>Chemin relatif avec séparateurs canoniques, chemin absolu extérieur, ou valeur vide reçue.</returns>
    internal static string? StorePath(string? path, string pathBase)
        => ConfigurationFileAccessFunctions.StorePath(path, pathBase);

    /// <summary>Résout un chemin relatif enregistré par rapport à la base du document.</summary>
    /// <param name="path">Chemin stocké ; les valeurs vides et les chemins déjà entièrement qualifiés sont conservés.</param>
    /// <param name="pathBase">Racine de résolution des chemins relatifs.</param>
    /// <returns>Chemin utilisable ou valeur reçue lorsqu’elle ne nécessite pas de résolution.</returns>
    internal static string? ResolvePath(string? path, string pathBase)
        => ConfigurationFileAccessFunctions.ResolvePath(path, pathBase);

    /// <summary>Écrit le document dans un fichier temporaire puis remplace sa destination.</summary>
    /// <param name="path">Chemin du document cible, distinct des fichiers médias décrits dans ses données.</param>
    /// <param name="document">Document emprunté, stable pendant sa sérialisation asynchrone.</param>
    /// <param name="cancellationToken">Annulation transmise à la sérialisation et au vidage du flux.</param>
    /// <returns>Opération achevée après l’écriture et le remplacement du document.</returns>
    /// <remarks>Le service partagé ferme son flux et nettoie le fichier temporaire dans son parcours de fin,
    /// y compris après une erreur. Les erreurs d’accès et d’annulation restent visibles par l’appelant.</remarks>
    internal static Task WriteDocumentAtomicallyAsync(string path, ConfigurationDocument document, CancellationToken cancellationToken)
        =>
            ConfigurationFileAccessFunctions.WriteDocumentAtomicallyAsync(path, document, JsonOptions, cancellationToken);

    /// <summary>Prépare chacun des quatre dossiers avec la même règle de stockage des chemins.</summary>
    /// <param name="folders">Chemins empruntés de la configuration.</param>
    /// <param name="pathBase">Racine commune servant au calcul des chemins relatifs.</param>
    /// <returns>Nouveau contrat de dossiers contenant les valeurs à enregistrer.</returns>
    private static FolderConfiguration StoreFolders(FolderConfiguration folders, string pathBase) => new(StorePath(folders.Shared, pathBase), StorePath(folders.Floppies, pathBase), StorePath(folders.States, pathBase), StorePath(folders.Captures, pathBase));

    /// <summary>Résout chacun des quatre dossiers avec la même règle de lecture des chemins.</summary>
    /// <param name="folders">Chemins stockés dans le document.</param>
    /// <param name="pathBase">Racine de résolution des chemins relatifs.</param>
    /// <returns>Nouveau contrat de dossiers utilisable par la configuration.</returns>
    private static FolderConfiguration ResolveFolders(FolderConfiguration folders, string pathBase) => new(ResolvePath(folders.Shared, pathBase), ResolvePath(folders.Floppies, pathBase), ResolvePath(folders.States, pathBase), ResolvePath(folders.Captures, pathBase));
}
