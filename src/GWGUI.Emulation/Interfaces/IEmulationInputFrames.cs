namespace GWGUI.Emulation.Interfaces;

/// <summary>Copie des entrées puis publication d’un instantané stable pour une frame.</summary>
/// <remarks>Chaque stockage possède son état indépendant et ne possède aucun périphérique. Les mouvements relatifs sont additionnés avec saturation puis consommés à chaque Poll ; les états absolus sont maintenus.</remarks>
public interface IEmulationInputFrames
{
    /// <summary>Copie les collections empruntées, conserve le dernier état absolu et additionne les quatre composantes relatives avec saturation.</summary>
    /// <param name="snapshot">Instantané emprunté pendant cet appel ; null représente un état vide.</param>
    void Update(EmulationInputSnapshot? snapshot);
    /// <summary>Publie les entrées et consomme les déplacements horizontal et vertical ainsi que les deux molettes, en conservant les états absolus.</summary>
    void Poll();
    /// <summary>Instantané figé, valable jusqu’au prochain Poll.</summary>
    EmulationInputSnapshot Polled { get; }
}
