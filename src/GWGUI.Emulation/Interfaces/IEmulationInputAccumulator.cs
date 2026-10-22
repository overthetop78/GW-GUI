using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Conserve les entrées absolues les plus récentes et additionne les mouvements relatifs entre deux consommations.</summary>
/// <remarks>Chaque instance possède son propre état synchronisé. Les collections reçues sont empruntées : leur propriétaire doit les garder stables pendant leur utilisation. Pour copier les collections, utiliser le contrat IEmulationInputFrames. Ce contrat ne possède aucun périphérique ni aucune ressource native.</remarks>
public interface IEmulationInputAccumulator
{
    /// <summary>Remplace les touches, boutons et contrôleurs par les dernières valeurs et additionne les déplacements et molettes avec saturation entière.</summary>
    /// <param name="snapshot">Instantané emprunté ; null fournit un état vide tout en conservant les mouvements déjà accumulés.</param>
    void Update(EmulationInputSnapshot? snapshot);

    /// <summary>Renvoie les entrées courantes puis remet uniquement DeltaX, DeltaY, Wheel et HorizontalWheel à zéro.</summary>
    /// <returns>Instantané contenant les mouvements depuis la consommation précédente ; les états absolus restent disponibles aux consommations suivantes.</returns>
    EmulationInputSnapshot Consume();

    /// <summary>Efface toutes les entrées absolues et relatives, notamment lors de la réinitialisation du cœur.</summary>
    void Reset();
}
