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
        [new(SettingsConstants.ResourceNativeVideo, EmulationMachineTab.Video, SettingsConstants.ResourceNativeVideo,
            GetOptions().Where(option => option.IsVisible).Select(option => CommonSettings.Native(configuration, option, EmulationMachineTab.Video, SettingsConstants.ResourceNativeVideo)).ToArray())];

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.Options.TryGetValue(OptionConstants.OptionMsxtype, out var machine)
            && machine is not CoreModelConstants.Automatic and not CoreModelConstants.ColecoVision)
            throw new ArgumentException(nameof(configuration.Options));
    }
}
