using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Coleco.Emulators.BlueMSX.Constants;
using Settings = GWGUI.Emulation.Coleco.Emulators.BlueMSX.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.BlueMSX.Factories;

internal sealed class BlueMSXMachineFactory() : MachineFactory(EmulatorConstants.Entry), IEmulatorSettings, IEmulatorConfigurationValidator, IEmulatorContentFormats
{
    public IReadOnlySet<string> SupportedContentExtensions(IReadOnlySet<string> declaredExtensions, bool requiresFilePath)
    {
        var extensions = new HashSet<string>(declaredExtensions, StringComparer.OrdinalIgnoreCase);
        // In the forced ColecoVision mode, the native file loader treats unknown suffixes as cartridges.
        extensions.Add(EmulatorConstants.RawCartridgeExtension);
        return extensions;
    }
    public IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration) => Settings.Create(configuration);
    public IReadOnlyList<CoreOption> GetSettingsOptions() => Settings.GetOptions();
    public void ValidateConfiguration(MachineConfiguration configuration) => Settings.Validate(configuration);
}
