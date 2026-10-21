namespace GWGUI.Emulation.Coleco.Common.Interfaces;
internal interface IEmulatorAdapter : global::GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>
{
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
