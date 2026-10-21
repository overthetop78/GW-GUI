namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Déclaration facultative du choix d’émulateur recommandé pour un modèle.</summary>
/// <remarks>Un adaptateur peut implémenter ce contrat en complément de IEmulatorAdapter. Le catalogue consulte cette
/// déclaration parmi les seuls adaptateurs qui annoncent prendre en charge la machine. Elle décrit une préférence,
/// indépendamment de la présence sur disque d’une bibliothèque de cœur.</remarks>
internal interface IEmulatorMachineDefaults
{
    /// <summary>Indique si cet adaptateur est le choix recommandé pour le modèle demandé.</summary>
    /// <param name="machineId">Identifiant technique du modèle, identique à celui déclaré dans MachineIds.</param>
    /// <returns>Vrai pour recommander cet adaptateur ; faux laisse le catalogue examiner les autres candidats.</returns>
    /// <remarks>Cette consultation de métadonnées ne démarre aucun cœur et ne modifie aucune configuration.</remarks>
    bool IsDefaultFor(string machineId);
}
