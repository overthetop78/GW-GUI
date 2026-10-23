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
}
