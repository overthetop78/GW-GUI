using GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Constants;
using GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Dictionaries;
using static GWGUI.Emulation.Bandai.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Functions;
internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock>();
        foreach (var group in BeetleWonderSwanOptionCatalog.All.Where(option => option.IsVisible).GroupBy(option => option.Key switch
        {
            OptionConstants.OptionRotateDisplay or OptionConstants.OptionMonoPalette or OptionConstants.OptionGfxColors or OptionConstants.OptionFrameskip or OptionConstants.OptionFrameskipThreshold or OptionConstants.Option60hzMode => EmulationMachineTab.Video,
            OptionConstants.OptionSoundSampleRate or OptionConstants.OptionSoundLowPass => EmulationMachineTab.Audio,
            OptionConstants.OptionRotateKeymap => EmulationMachineTab.Controllers,
            _ => throw new InvalidOperationException(option.Key)
        }))
        {
            var resource = group.Key switch
            {
                EmulationMachineTab.Video => SettingsConstants.ResourceNativeVideo,
                EmulationMachineTab.Audio => SettingsConstants.ResourceNativeAudio,
                EmulationMachineTab.Controllers => SettingsConstants.ResourceNativeControls,
                _ => throw new InvalidOperationException()
            };
            blocks.Add(Block(group.Key, resource, group.Select(option =>
            {
                var field = Native(configuration, option, group.Key, resource);
                return option.Key == OptionConstants.OptionMonoPalette
                    ? field with
                    {
                        VideoSection = EmulationVideoSettingsSection.Image,
                        Value = configuration.Options.GetValueOrDefault(option.Key, PaletteFunctions.Default(configuration.Model))
                    }
                    : field;
            }).ToArray())with { IsCompact = group.Key == EmulationMachineTab.Audio });
        }

        return blocks;
    }
}
