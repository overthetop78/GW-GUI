namespace GWGUI.Emulation.Acorn.Common.Interfaces;

internal interface IEmulatorConfigurationOptions
{
    IReadOnlyDictionary<string, string> GetDefaultOptions(MachineModel model);
    void ValidateOptions(MachineConfiguration configuration);
    MachineModel ResolveModel(MachineModel model, IReadOnlyDictionary<string, string> options) => model;
    IReadOnlyDictionary<string, string> NormalizeOptions(MachineModel model, IReadOnlyDictionary<string, string> options) => options;
}
