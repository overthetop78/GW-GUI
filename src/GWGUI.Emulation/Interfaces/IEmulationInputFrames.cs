namespace GWGUI.Emulation.Interfaces;

/// <summary>Copie des entrées puis publication d’un instantané stable pour une frame.</summary>
/// <remarks>La politique est choisie à la création : mouvements accumulés et consommés, ou remplacement complet sans consommation. Chaque stockage possède son état indépendant et ne possède aucun périphérique.</remarks>
public interface IEmulationInputFrames
{
    /// <summary>Copie les collections empruntées et reçoit le nouvel état selon la politique de création.</summary>
    /// <param name="snapshot">Instantané emprunté pendant cet appel ; null représente un état vide.</param>
    void Update(EmulationInputSnapshot? snapshot);
    /// <summary>Publie les entrées ; consomme les mouvements en mode accumulé et les conserve en mode de remplacement.</summary>
    void Poll();
    /// <summary>Instantané figé, valable jusqu’au prochain Poll.</summary>
    EmulationInputSnapshot Polled { get; }
}
