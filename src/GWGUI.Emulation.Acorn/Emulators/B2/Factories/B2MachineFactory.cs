using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Acorn.Emulators.B2.Functions;
using GWGUI.Emulation.Acorn.Emulators.B2.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.B2.Factories;

internal sealed class B2MachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorConfigurationOptions, IEmulatorSettings, IEmulatorStorageSettings
{
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => MediaDeviceFunctions.Describe(configuration);

    public IReadOnlyDictionary<string, string> GetDefaultOptions(MachineModel model) => ConfigurationOptionFunctions.Defaults(model);
    public void ValidateOptions(MachineConfiguration configuration) => ConfigurationOptionFunctions.Validate(configuration);
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => EmulatorSettingsFunctions.Describe(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null) => EmulatorSettingsFunctions.Options(configuration);

}
