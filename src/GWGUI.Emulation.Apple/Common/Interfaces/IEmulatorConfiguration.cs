namespace GWGUI.Emulation.Apple.Common.Interfaces;

internal interface IEmulatorConfiguration
{
    void InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options);
    void ValidateConfiguration(MachineConfiguration configuration);
}
