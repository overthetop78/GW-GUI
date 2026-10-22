namespace GWGUI.Emulation.Panasonic.Common.Interfaces;

/// <summary>Projection des entrées physiques vers le protocole du cœur par le contrat Common.</summary>
internal interface IEmulatorInputMapper
{
    /// <summary>Projette les entrées physiques selon le modèle et les associations de la session.</summary>
    /// <param name="snapshot">Instantané physique emprunté, conservé intact.</param>
    /// <param name="configuration">Configuration complète et courante de la session, consultée sans modification.</param>
    /// <returns>Instantané adapté avec les règles existantes de la projection Common.</returns>
    EmulationInputSnapshot MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration);
}
