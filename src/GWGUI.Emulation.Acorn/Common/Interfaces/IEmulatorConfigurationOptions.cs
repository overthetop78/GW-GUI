namespace GWGUI.Emulation.Acorn.Common.Interfaces;

internal interface IEmulatorConfigurationOptions
{
    IReadOnlyDictionary<string, string> GetDefaultOptions(MachineModel model);
    void ValidateOptions(MachineConfiguration configuration);
}
