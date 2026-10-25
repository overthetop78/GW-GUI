using GWGUI.Emulation.Acorn.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Acorn.Emulators.B2.Functions;
using GWGUI.Emulation.Acorn.Emulators.B2.Constants;
using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;

namespace GWGUI.Emulation.Acorn.Emulators.B2.Factories;

internal sealed class B2MachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorConfigurationOptions, IEmulatorSettings, IEmulatorStorageSettings
{
    /// <summary>Réutilise les identités et compatibilités matérielles communes ; B2 charge ses propres ROM intégrées.</summary>
    /// <remarks>La destination reste absente : reconnaître une ROM externe n'ajoute pas au cœur B2 une fonction de remplacement.</remarks>
    public override ValueTask<IReadOnlyList<EmulationFirmwareCandidate>> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken) => BbcMicroFirmwareFunctions.ScanAsync(configuration.Model, path, _ => null, cancellationToken);

    public IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration) => MediaDeviceFunctions.Describe(configuration);
    public IReadOnlyList<EmulationStorageDeviceSettings> DescribeDeviceSettings(MachineConfiguration configuration) => MediaDeviceFunctions.Settings(configuration);
    public MachineConfiguration ApplyDeviceSettings(MachineConfiguration configuration, IReadOnlyList<EmulationStorageDeviceSettings> settings) => MediaDeviceFunctions.Apply(configuration, settings);

    public IReadOnlyDictionary<string, string> GetDefaultOptions(MachineConfiguration configuration) => ConfigurationOptionFunctions.Defaults(configuration.Model);
    public void ValidateOptions(MachineConfiguration configuration) => ConfigurationOptionFunctions.Validate(configuration);
    public IReadOnlyDictionary<string, string> NormalizeOptions(MachineConfiguration configuration) => ConfigurationOptionFunctions.Normalize(configuration.Model, configuration.Options);
    public MachineModel ResolveModel(MachineConfiguration configuration) =>
        configuration.Model == MachineModel.BbcMicroModelB && configuration.Options.TryGetValue(OptionConstants.Model, out var nativeModel) ? nativeModel switch
        {
            OptionConstants.ModelBPlus => MachineModel.BbcMicroModelBPlus,
            OptionConstants.ModelBPlus128 => MachineModel.BbcMicroModelBPlus128,
            _ => configuration.Model
        } : configuration.Model;
    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null) => EmulatorSettingsFunctions.Describe(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions(MachineConfiguration? configuration = null, EmulatorManagementContext? context = null) => EmulatorSettingsFunctions.Options(configuration);

}
