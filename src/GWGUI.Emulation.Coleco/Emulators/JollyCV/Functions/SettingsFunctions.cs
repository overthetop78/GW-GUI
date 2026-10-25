using GWGUI.Emulation.Coleco.Emulators.JollyCV.Constants;
using GWGUI.Emulation.Coleco.Emulators.JollyCV.Dictionaries;
using CommonSettings = GWGUI.Emulation.Coleco.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.JollyCV.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<CoreOption> GetOptions() => JollyCVOptionCatalog.All.Select(option => option with { IsVisible = option.Key != OptionConstants.OptionAutoreset }).ToArray();

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration) =>
        [new(SettingsConstants.ResourceNativeVideo, EmulationMachineTab.Video, SettingsConstants.ResourceNativeVideo,
            GetOptions().Where(option => option.IsVisible).Select(option => CommonSettings.Native(configuration, option, EmulationMachineTab.Video, SettingsConstants.ResourceNativeVideo)).ToArray())];
}
