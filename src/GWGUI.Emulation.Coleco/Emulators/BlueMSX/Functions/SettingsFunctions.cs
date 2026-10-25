using GWGUI.Emulation.Coleco.Emulators.Common.Constants;
using GWGUI.Emulation.Coleco.Emulators.BlueMSX.Constants;
using GWGUI.Emulation.Coleco.Emulators.BlueMSX.Dictionaries;
using CommonSettings = GWGUI.Emulation.Coleco.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.BlueMSX.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<CoreOption> GetOptions() => BlueMSXOptionCatalog.All.Select(option => option with
    {
        IsVisible = option.Key is OptionConstants.OptionOverscan or OptionConstants.OptionVdpSynctype or OptionConstants.OptionNospritelimits or OptionConstants.OptionPatchColecoRom
    }).ToArray();

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration) =>
        GetOptions().Where(option => option.IsVisible).GroupBy(option => option.Key == OptionConstants.OptionPatchColecoRom ? EmulationMachineTab.General : EmulationMachineTab.Video)
            .Select(group =>
            {
                var resource = group.Key == EmulationMachineTab.General ? SettingsConstants.ResourceNativeSystem : SettingsConstants.ResourceNativeVideo;
                return new EmulationSettingsBlock(resource, group.Key, resource,
                    group.Select(option => CommonSettings.Native(configuration, option, group.Key, resource)).ToArray());
            }).ToArray();

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.Options.TryGetValue(OptionConstants.OptionMsxtype, out var machine)
            && machine is not CoreModelConstants.Automatic and not CoreModelConstants.ColecoVision)
            throw new ArgumentException(nameof(configuration.Options));
    }
}
