namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Functions;

internal static class InputSnapshotFunctions
{
    /// <summary>Projette les entrées de session par le contrat Common et applique la bascule du pointeur.</summary>
    /// <param name="snapshot">Instantané physique emprunté, dont les collections restent inchangées.</param>
    /// <param name="configuration">Configuration complète et courante : modèle, cœur, options et associations.</param>
    /// <param name="mapper">Adaptateur emprunté chargé de traduire les commandes ; null utilise la projection de repli du module.</param>
    /// <param name="controllerPointerSwitchPressed">Impulsion de session ajoutant Contrôle droit aux touches projetées.</param>
    /// <returns>Instantané adapté au cœur ; sans impulsion, le résultat du projecteur est conservé tel quel.</returns>
    /// <remarks>La bascule est appliquée après la projection, comme pour les cœurs qui la prennent déjà en charge.
    /// Elle copie l'ensemble des touches avant ajout, conserve les autres entrées et ne modifie ni la source
    /// ni la configuration. Cette opération ne crée aucun cœur et ne possède aucune ressource native.</remarks>
    internal static EmulationInputSnapshot Apply(EmulationInputSnapshot snapshot, MachineConfiguration configuration, IEmulatorInputMapper? mapper = null, bool controllerPointerSwitchPressed = false)
    {
        var mapped = mapper?.MapInput(snapshot, configuration) ?? EmulationInputSnapshot.Empty;
        return controllerPointerSwitchPressed ? mapped with { Keys = new HashSet<EmulationKey>(mapped.Keys) { EmulationKey.RightControl } } : mapped;
    }
}
