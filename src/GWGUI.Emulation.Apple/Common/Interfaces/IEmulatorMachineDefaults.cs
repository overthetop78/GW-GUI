namespace GWGUI.Emulation.Apple.Common.Interfaces;

/// <summary>Recommandation du cœur à utiliser pour une nouvelle configuration de machine.</summary>
/// <remarks>Common consulte les seuls adaptateurs qui déclarent la machine compatible. Une recommandation
/// concerne les capacités du cœur, indépendamment de son téléchargement ou de son installation locale.
/// Le choix explicite enregistré dans une configuration reste distinct de cette préférence initiale.</remarks>
internal interface IEmulatorMachineDefaults
{
    /// <summary>Indique si cet adaptateur est recommandé pour la machine demandée.</summary>
    /// <param name="machineId">Identifiant technique du modèle, identique à celui déclaré dans MachineIds.</param>
    /// <returns>True pour recommander le cœur ; false conserve les autres recommandations ou le choix initial du module.</returns>
    /// <remarks>Cette consultation de métadonnées ne démarre aucun cœur, ne lit aucun contenu et ne modifie aucune configuration.</remarks>
    bool IsDefaultFor(string machineId);
}
