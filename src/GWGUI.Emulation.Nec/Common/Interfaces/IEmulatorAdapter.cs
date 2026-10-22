namespace GWGUI.Emulation.Nec.Common.Interfaces;
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>, IEmulatorSettings, IEmulatorInputMapper, IEmulatorStorageSettings
{
    /// <summary>Réutilise les associations Common ; la machine ajoute ensuite sa touche de bascule de session.</summary>
    EmulationInputSnapshot IEmulatorInputMapper.MapInput(EmulationInputSnapshot snapshot, MachineConfiguration configuration) => InputSnapshotFunctions.Apply(snapshot, configuration.Input, false);

}
