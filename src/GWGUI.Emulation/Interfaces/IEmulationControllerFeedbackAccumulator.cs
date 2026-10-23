using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Conserve les intensités des deux moteurs de chaque port émulé, sans commander de périphérique physique.</summary>
/// <remarks>Les opérations sont synchronisées. Le propriétaire ferme le stockage après le détachement des
/// callbacks natifs ; un callback tardif est refusé après Dispose. Aucune ressource native n’est possédée ici.</remarks>
public interface IEmulationControllerFeedbackAccumulator : IDisposable
{
    /// <summary>Remplace l’intensité du moteur fort (0) ou faible (1) du port ; zéro arrête cet effet.</summary>
    /// <returns>False pour un port hors capacité, un effet inconnu ou un stockage fermé.</returns>
    bool SetState(uint port, uint effect, ushort strength);
    /// <summary>Copie les effets non nuls en intensités normalisées ; les lectures ne consomment pas les effets.</summary>
    IReadOnlyList<ControllerFeedbackState> Snapshot();
    /// <summary>Arrête les deux effets du port ; un port hors capacité est ignoré.</summary>
    void ResetPort(int port);
}
