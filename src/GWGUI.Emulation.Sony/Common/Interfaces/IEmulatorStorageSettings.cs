namespace GWGUI.Emulation.Sony.Common.Interfaces;

/// <summary>Organisation du stockage et des cartes mémoire propres à un backend.</summary>
/// <remarks>Les lecteurs sont décrits par IEmulatorMediaCapabilities et les formats de contenu par
/// IEmulatorContentFormats. Les indices, dossiers et clés d’options gardent les conventions du cœur.</remarks>
internal interface IEmulatorStorageSettings : IEmulatorMediaCapabilities
{
    /// <summary>Résout le nombre d’emplacements de cartes mémoire du modèle courant.</summary>
    /// <param name="configuration">Configuration empruntée déterminant les emplacements disponibles.</param>
    /// <returns>Nombre d’emplacements prévu par le modèle, avant une éventuelle spécialisation du backend.</returns>
    int GetMemoryCardCount(MachineConfiguration configuration) => configuration.Model switch
    {
        ModelConstants.Psp => 1,
        ModelConstants.PlayStation or ModelConstants.PlayStation2 => StorageSettingsFunctionsConstants.MemoryCardCount,
        _ => 0
    };
    /// <summary>Énumère les indices techniques des cartes mémoire disponibles.</summary>
    /// <param name="configuration">Configuration dont le nombre d’emplacements est consulté.</param>
    /// <returns>Indices à partir de zéro, ou organisation particulière fournie par le backend.</returns>
    IEnumerable<int> GetMemoryCardIndexes(MachineConfiguration configuration) => Enumerable.Range(0, GetMemoryCardCount(configuration));
    /// <summary>Fournit le libellé propre à un emplacement, ou null pour la présentation commune.</summary>
    /// <param name="index">Indice technique de la carte mémoire.</param>
    /// <returns>Libellé particulier du backend, lorsqu’il existe.</returns>
    string? GetMemoryCardDisplayLabel(int index) => null;
    /// <summary>Clé native d’insertion de la carte mémoire, ou null lorsque le cœur utilise un autre mécanisme.</summary>
    string? MemoryCardInsertedOptionKey => null;
    /// <summary>Indique si le cœur impose les noms des dossiers de cartes mémoire.</summary>
    bool HasFixedMemoryCardDirectories => false;
    /// <summary>Résout le dossier imposé par le cœur pour l’emplacement demandé.</summary>
    /// <param name="index">Indice technique de la carte mémoire.</param>
    /// <returns>Nom du dossier imposé, ou null si son choix est libre.</returns>
    string? GetMemoryCardDirectory(int index) => null;
}
