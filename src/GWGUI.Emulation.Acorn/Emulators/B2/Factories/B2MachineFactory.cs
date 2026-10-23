using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Acorn.Emulators.B2.Functions;
using GWGUI.Emulation.Acorn.Emulators.B2.Constants;

namespace GWGUI.Emulation.Acorn.Emulators.B2.Factories;

internal sealed class B2MachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorConfigurationOptions, IEmulatorSettingsAdapter, IEmulatorStorageSettings
{
    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => MediaDeviceFunctions.Describe(configuration);
    public IReadOnlyList<EmulationStorageDeviceSettings> DescribeDeviceSettings(MachineConfiguration configuration) => MediaDeviceFunctions.Settings(configuration);
    public MachineConfiguration ApplyDeviceSettings(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings) => MediaDeviceFunctions.Apply(configuration, settings);

    public IReadOnlyDictionary<string, string> GetDefaultOptions(MachineModel model) => ConfigurationOptionFunctions.Defaults(model);
    public void ValidateOptions(MachineConfiguration configuration) => ConfigurationOptionFunctions.Validate(configuration);
    public IReadOnlyDictionary<string, string> NormalizeOptions(MachineModel model, IReadOnlyDictionary<string, string> options) => ConfigurationOptionFunctions.Normalize(model, options);
    public MachineModel ResolveModel(MachineModel model, IReadOnlyDictionary<string, string> options) =>
        model == MachineModel.BbcMicroModelB && options.TryGetValue(OptionConstants.Model, out var nativeModel) ? nativeModel switch
        {
            OptionConstants.ModelBPlus => MachineModel.BbcMicroModelBPlus,
            OptionConstants.ModelBPlus128 => MachineModel.BbcMicroModelBPlus128,
            _ => model
        } : model;
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => EmulatorSettingsFunctions.Describe(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => EmulatorSettingsFunctions.Options(configuration);

}
