namespace GWGUI.Emulation.Atari.Common.Interfaces;
internal interface IEmulatorAdapter : GWGUI.Emulation.Interfaces.IEmulationEmulatorAdapter<MachineConfiguration, Machine>
{
    IReadOnlySet<string>? GetCartridgeExtensions(MachineConfiguration configuration);
    IReadOnlyDictionary<string, string> GetRuntimeOptions(MachineConfiguration configuration);
}
