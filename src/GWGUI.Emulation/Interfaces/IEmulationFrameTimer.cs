namespace GWGUI.Emulation.Interfaces;

/// <summary>Attente possédée des échéances de frames, avec la même horloge que la boucle de machine.</summary>
/// <remarks>Le créateur libère le timer dans son finally, après la fin de ses attentes.
/// Les ressources natives du timer restent sous la responsabilité de son implémentation interne.</remarks>
public interface IEmulationFrameTimer : IDisposable
{
    /// <summary>Attend l’échéance demandée ou l’annulation de la session.</summary>
    /// <param name="deadline">Échéance absolue exprimée en ticks de Stopwatch.</param>
    /// <param name="cancellationToken">Signal d’annulation de l’attente en cours.</param>
    void WaitUntil(long deadline, CancellationToken cancellationToken);
}
