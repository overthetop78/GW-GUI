namespace GWGUI.Emulation.Interfaces;

public interface IEmulationMedia
{
    /// <summary>Consulte le statut détaillé du jeu de disques courant sans changer les montages.</summary>
    /// <param name="cancellationToken">Annulation de la consultation en attente.</param>
    /// <returns>Instantané géré du nombre, de la sélection et des descriptions des images, avec leur état d'éjection.</returns>
    /// <remarks>La machine exécute cette lecture sur la même file que ses autres commandes. Le statut décrit
    /// le jeu de disques annoncé par le cœur, distinct de la liste de tous les médias montés.</remarks>
    ValueTask<DiskStatus> GetDiskStatusAsync(CancellationToken cancellationToken = default);

    IReadOnlyList<EmulationMedia> MountedMedia { get; }
    ValueTask InsertAsync(EmulationMedia media, CancellationToken cancellationToken = default);
    ValueTask EjectAsync(EmulationMediaSlot slot, CancellationToken cancellationToken = default);
    ValueTask SelectDiskAsync(EmulationMediaSlot slot, int index, CancellationToken cancellationToken = default);
    /// <summary>Enregistre explicitement les changements d’un média sur le thread de son cœur.</summary>
    /// <param name="slot">Lecteur à enregistrer ; les autres montages restent inchangés.</param>
    /// <param name="cancellationToken">Annulation de la commande en attente.</param>
    /// <returns>Attente de la publication selon la politique du backend.</returns>
    /// <remarks>Le fichier utilisateur reste possédé par l’appelant. Le propriétaire du cœur sérialise cette
    /// commande avec ses frames et ses autres opérations de médias ; aucun cœur supplémentaire n’est créé.</remarks>
    ValueTask SaveChangesAsync(EmulationMediaSlot slot, CancellationToken cancellationToken = default);
    /// <summary>Consulte les changements possibles d’un média dans la même file que ses commandes.</summary>
    /// <param name="slot">Lecteur dont les changements conservés par le backend sont demandés.</param>
    /// <param name="cancellationToken">Annulation de la consultation en attente.</param>
    /// <returns>True si le backend signale une copie modifiable à publier explicitement.</returns>
    ValueTask<bool> HasUnsavedChangesAsync(EmulationMediaSlot slot, CancellationToken cancellationToken = default);
}
