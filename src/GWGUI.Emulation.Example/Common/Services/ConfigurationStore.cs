using System.IO;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Charge, enregistre et supprime les configurations persistantes du module.</summary>
/// <remarks>Chaque machine possède un sous-dossier identifié par son GUID. Les chemins des contenus sont enregistrés
/// relativement au répertoire de référence lorsque cela est possible ; l’écriture atomique est assurée par les fonctions communes.</remarks>
public sealed class ConfigurationStore
{
    /// <summary>Répertoire absolu contenant les sous-dossiers des machines.</summary>
    private readonly string _directory;
    /// <summary>Répertoire absolu servant à stocker et résoudre les chemins relatifs des contenus.</summary>
    private readonly string _pathBase;
    /// <summary>Verrou partagé entre les instances du magasin pour sérialiser leurs enregistrements.</summary>
    private static readonly SemaphoreSlim SaveGate = new(ConfigurationStoreConstants.InitialWriterCount, ConfigurationStoreConstants.MaximumWriterCount);

    /// <summary>Configure l’emplacement du magasin sans créer ni lire ses fichiers.</summary>
    /// <param name="directory">Répertoire du magasin, converti en chemin absolu.</param>
    /// <param name="pathBase">Référence des chemins de contenu ; le répertoire du magasin est utilisé si cette valeur est null.</param>
    /// <exception cref="ArgumentException">Le répertoire du magasin est vide ou blanc, ou un chemin est invalide.</exception>
    public ConfigurationStore(string directory, string? pathBase = null)
    {
        _directory = Path.GetFullPath(string.IsNullOrWhiteSpace(directory) ? throw new ArgumentException(nameof(directory)) : directory);
        _pathBase = Path.GetFullPath(pathBase ?? directory);
    }

    /// <summary>Lit les documents du magasin et reconstruit leurs configurations avec les chemins de contenu résolus.</summary>
    /// <param name="cancellationToken">Annulation de la lecture des documents.</param>
    /// <returns>Configurations chargées par le lecteur de documents commun.</returns>
    public Task<IReadOnlyList<MachineConfiguration>> LoadAllAsync(CancellationToken cancellationToken = default)
        => ConfigurationFileAccessFunctions.ReadDocumentsAsync(_directory, ConfigurationPaths,
            static root => ConfigurationStoreFunctions.Deserialize(root),
            document => ConfigurationStoreFunctions.FromDocument(document, _pathBase), cancellationToken);

    /// <summary>Enregistre un document de configuration sous le verrou d’écriture partagé.</summary>
    /// <param name="configuration">Configuration à sérialiser dans le sous-dossier de son identifiant.</param>
    /// <param name="cancellationToken">Annulation de l’attente du verrou et de l’écriture.</param>
    /// <returns>Tâche achevée après remplacement atomique du document.</returns>
    /// <exception cref="ArgumentNullException">La configuration est null.</exception>
    public async Task SaveAsync(MachineConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        await ConfigurationFileAccessFunctions.WithSaveGateAsync(SaveGate, () =>
        {
            var document = ConfigurationStoreFunctions.ToDocument(configuration, _pathBase);
            var machineDirectory = Path.Combine(_directory, configuration.Id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
            return ConfigurationStoreFunctions.WriteDocumentAtomicallyAsync(Path.Combine(machineDirectory, ConfigurationStoreConstants.MachineFileName), document, cancellationToken);
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Supprime le sous-dossier persistant d’une machine et son contenu.</summary>
    /// <param name="id">Identifiant non vide de la machine à retirer du magasin.</param>
    /// <exception cref="ArgumentException">L’identifiant est vide.</exception>
    /// <remarks>Les images référencées par les chemins de contenu ne sont pas supprimées par cette opération.</remarks>
    public void Delete(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException(nameof(id));
        ConfigurationFileAccessFunctions.DeleteDirectoryIfExists(Path.Combine(_directory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)), ConfigurationStoreConstants.RecursiveDirectoryDelete);
    }

    /// <summary>Énumère les chemins des documents de machine sous le répertoire du magasin.</summary>
    /// <returns>Chemins fournis au lecteur commun, avec le nom de fichier fixé par les constantes du module.</returns>
    private IEnumerable<string> ConfigurationPaths() => ConfigurationFileAccessFunctions.EnumerateDocumentPaths(_directory, ConfigurationStoreConstants.MachineFileName);
}

