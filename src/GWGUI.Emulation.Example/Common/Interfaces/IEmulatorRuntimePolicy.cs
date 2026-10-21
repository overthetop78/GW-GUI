using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Contraintes de redémarrage annoncées par le backend pour ses opérations de runtime.</summary>
/// <remarks>Le propriétaire de la machine applique ces décisions et libère le cœur remplacé dans finally.
/// L’interface décrit une politique et ne possède elle-même aucun thread ou cœur.</remarks>
internal interface IEmulatorRuntimePolicy
{
    /// <summary>Détermine si un changement de média impose de recréer le cœur.</summary>
    /// <param name="slot">Emplacement de lecteur concerné.</param>
    /// <param name="path">Chemin du nouveau contenu ; null représente une éjection.</param>
    /// <returns>True si le cœur ne permet pas cette modification à chaud.</returns>
    bool RequiresMediaRestart(EmulationMediaSlot slot, string? path);
    /// <summary>Détermine si une option doit être appliquée au prochain démarrage du cœur.</summary>
    /// <param name="key">Identifiant technique exact de l’option.</param>
    /// <returns>True si un redémarrage est nécessaire.</returns>
    bool RequiresOptionRestart(string key);
}
