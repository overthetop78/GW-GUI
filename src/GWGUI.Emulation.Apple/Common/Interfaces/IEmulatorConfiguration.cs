namespace GWGUI.Emulation.Apple.Common.Interfaces;

internal interface IEmulatorConfiguration : IEmulatorConfigurationValidation
{
    void InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options);
}
