namespace GWGUI.Emulation.Sharp.Common.Interfaces;

internal interface IEmulatorInputMapper
{
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineModel model);
    /// <summary>Projette les entrées physiques selon le modèle et les associations de la session.</summary>
    /// <param name="snapshot">Instantané physique emprunté, conservé intact.</param>
    /// <param name="configuration">Configuration complète et courante de la session, consultée sans modification.</param>
    /// <returns>Instantané adapté au protocole du cœur avec les règles existantes du backend.</returns>
    EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration);
}
