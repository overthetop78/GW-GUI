namespace GWGUI.Emulation.Acorn.Common.Interfaces;
internal interface IEmulatorAdapter : global::GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>
{
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
