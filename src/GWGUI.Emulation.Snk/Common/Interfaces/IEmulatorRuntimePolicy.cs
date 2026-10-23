using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Snk.Common.Interfaces;

/// <summary>Contraintes de redémarrage annoncées par le backend pour ses opérations de runtime.</summary>
/// <remarks>Common conserve les contrôles généraux du modèle, des médias et des options. Ce contrat ajoute
/// les décisions propres au backend. Le propriétaire de la machine applique ces décisions et libère le cœur
/// remplacé dans finally ; la politique ne possède elle-même aucun thread, cœur ou fichier de contenu.</remarks>
internal interface IEmulatorRuntimePolicy
{
    /// <summary>Détermine si un changement de média impose de recréer le cœur.</summary>
    /// <param name="configuration">Configuration empruntée permettant de consulter le modèle, les options et les lecteurs.</param>
    /// <param name="slot">Emplacement de lecteur concerné.</param>
    /// <param name="path">Chemin du nouveau contenu ; null représente une éjection.</param>
    /// <returns>True si le backend impose un redémarrage pour cette modification.</returns>
    bool RequiresMediaRestart(MachineConfiguration configuration, EmulationMediaSlot slot, string? path);

    /// <summary>Détermine si une option doit être appliquée au prochain démarrage du cœur.</summary>
    /// <param name="configuration">Configuration empruntée contenant le contexte de cette décision.</param>
    /// <param name="key">Identifiant technique exact de l’option.</param>
    /// <returns>True si le backend impose un redémarrage ; les métadonnées générales des options restent appliquées séparément.</returns>
    bool RequiresOptionRestart(MachineConfiguration configuration, string key);
}
