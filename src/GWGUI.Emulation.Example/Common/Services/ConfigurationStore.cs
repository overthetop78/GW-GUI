using CommonCoreHostFunctions = GWGUI.Emulation.Example.Common.Functions.CoreHostFunctions;
using System.IO;
using GWGUI.Emulation;
using GWGUI.Emulation.Example.Common.Constants;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Functions;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Charge, enregistre et supprime les configurations persistantes du module.</summary>
/// <remarks>Chaque machine possède un sous-dossier identifié par son GUID. Les chemins des contenus sont enregistrés
    /// relativement au répertoire de référence lorsque cela est possible ; l’écriture atomique est assurée par
    /// l’interface commune. Les médias restent empruntés et ne sont ni déplacés ni copiés par le magasin.</remarks>
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

    /// <summary>Résout le dossier des mémoires persistantes propres à une machine.</summary>
    /// <param name="id">Identité non vide de la configuration à laquelle appartiennent ces mémoires.</param>
    /// <returns>Chemin du dossier Memory sous le dossier de la machine, sans création du dossier.</returns>
    /// <exception cref="ArgumentException">L’identité reçue est vide.</exception>
    /// <remarks>Les cartes mémoire et sauvegardes du cœur conservées ici survivent aux sessions.
    /// Le consommateur qui ouvre leurs fichiers ferme ses flux dans son parcours de libération.</remarks>
    public string MemoryDirectory(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException(nameof(id));
        return Path.Combine(_directory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat), CoreDirectoryConstants.MemoryDirectoryName);
    }

    /// <summary>Lit les documents du magasin et reconstruit leurs configurations avec les chemins de contenu résolus.</summary>
    /// <param name="cancellationToken">Annulation de la lecture des documents.</param>
    /// <returns>Configurations chargées par le lecteur de documents commun.</returns>
    /// <remarks>Le lecteur crée la racine si nécessaire et énumère les documents dans l’ordre ordinal
    /// insensible à la casse. Il ignore les documents JSON illisibles ou invalides selon sa politique commune,
    /// mais propage l’annulation. La restauration résout les chemins et vérifie le schéma du module.</remarks>
    public Task<IReadOnlyList<MachineConfiguration>> LoadAllAsync(CancellationToken cancellationToken = default)
        => CommonCoreHostFunctions.ConfigurationFiles.ReadDocumentsAsync(_directory, ConfigurationStoreConstants.MachineFileName, ConfigurationStoreFunctions.Deserialize,
            document => ConfigurationStoreFunctions.FromDocument(document, _pathBase), cancellationToken);

    /// <summary>Enregistre un document de configuration sous le verrou d’écriture partagé.</summary>
    /// <param name="configuration">Configuration à sérialiser dans le sous-dossier de son identifiant.</param>
    /// <param name="cancellationToken">Annulation de l’attente du verrou et de l’écriture.</param>
    /// <returns>Tâche achevée après remplacement atomique du document.</returns>
    /// <exception cref="ArgumentNullException">La configuration est null.</exception>
    /// <exception cref="InvalidDataException">Le cœur ou les firmwares obligatoires n’ont pas été sélectionnés.</exception>
    /// <exception cref="FileNotFoundException">Un contenu référencé par la configuration n’est pas présent.</exception>
    public async Task SaveAsync(MachineConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        await CommonCoreHostFunctions.ConfigurationFiles.WithSaveGateAsync(SaveGate, () =>
        {
            // Les contrôles de présence sont effectués sous le verrou, juste avant la projection et l’écriture du document.
            ConfigurationFunctions.ValidateForSave(configuration);
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
        CommonCoreHostFunctions.ConfigurationFiles.DeleteDirectoryIfExists(Path.Combine(_directory, id.ToString(ConfigurationStoreConstants.MachineIdentifierFormat)), ConfigurationStoreConstants.RecursiveDirectoryDelete);
    }

}

