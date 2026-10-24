using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Sega.Common.Interfaces;

/// <summary>Contraintes de redémarrage annoncées par le backend pour ses opérations de runtime.</summary>
/// <remarks>Common conserve les contrôles généraux du modèle, des médias et des options. Ce contrat ajoute
/// les décisions propres au backend. Le propriétaire de la machine applique ces décisions et libère le cœur
/// remplacé dans finally ; la politique ne possède elle-même aucun thread, cœur ou fichier de contenu.</remarks>
internal interface IEmulatorRuntimePolicy
{
    /// <summary>Combinaison de touches émulées choisie par le profil pour sa réinitialisation logicielle.</summary>
    /// <value>Touches à maintenir pendant une frame ; un ensemble vide sélectionne la commande native SoftReset.</value>
    /// <remarks>Cette collection décrit le profil et reste empruntée. Common transmet la séquence au service
    /// partagé qui relâche les entrées dans finally, même si le calcul de la frame échoue.
    /// La politique conserve ses autres règles de redémarrage et ne possède aucun cœur ni périphérique.</remarks>
    IReadOnlySet<EmulationKey> SoftResetKeys { get; }

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
    /// <summary>Projette les descriptions des options annoncées par le cœur sans changer leurs valeurs natives.</summary>
    /// <param name="configuration">Configuration de session empruntée donnant le profil et les réglages de présentation.</param>
    /// <param name="options">Options annoncées par le cœur, empruntées pendant la projection.</param>
    /// <returns>Options à publier ; le comportement de base conserve la collection reçue.</returns>
    /// <remarks>Le backend reprend ses métadonnées existantes par ce contrat. Cette opération ne crée
    /// ni cœur, ni processus, ni ressource native et ne prend pas possession des données reçues.</remarks>
    IReadOnlyList<CoreOption> NormalizeRuntimeOptions(MachineConfiguration configuration, IReadOnlyList<CoreOption> options) => options;

    /// <summary>Résout le délai applicable à une commande du protocole commun.</summary>
    /// <param name="configuration">Configuration empruntée de la session concernée.</param>
    /// <param name="command">Identifiant de commande du protocole commun.</param>
    /// <param name="fallback">Délai déjà choisi par le transport, conservé par défaut.</param>
    /// <returns>Délai applicable à cette attente, sans lancer ni interrompre de commande.</returns>
    TimeSpan GetHostCommandTimeout(MachineConfiguration configuration, byte command, TimeSpan fallback) => fallback;
}
