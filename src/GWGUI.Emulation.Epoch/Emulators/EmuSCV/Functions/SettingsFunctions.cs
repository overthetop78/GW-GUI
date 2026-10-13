using GWGUI.Emulation.Epoch.Emulators.EmuSCV.Constants;
using GWGUI.Emulation.Epoch.Emulators.EmuSCV.Dictionaries;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Constants;
using CommonSettings = GWGUI.Emulation.Epoch.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Epoch.Emulators.EmuSCV.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<EmulationMediaDevice> Storage(MachineConfiguration configuration) =>
        [new(EmulationMediaSlot.Cartridge0, EmulationMediaType.Cartridge, MediaRuntimeConstants.ScvCartridgeExtensions, RequiresMachineRecreation: true, ImageDirectory: configuration.Folders.Cartridges, IsPermanent: true)];

    internal static IReadOnlyList<CoreOption> GetOptions() => EmuSCVOptionCatalog.All;

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock>();
        foreach (var group in GetOptions().Where(option => option.IsVisible).GroupBy(option => option.Key switch
        {
            OptionConstants.OptionCheckbios => EmulationMachineTab.Rom,
            OptionConstants.OptionDisplayinputs or OptionConstants.OptionKeyboardatstart or OptionConstants.OptionLanguage => EmulationMachineTab.Controllers,
            _ => EmulationMachineTab.Video
        }))
        {
            var resource = group.Key switch
            {
                EmulationMachineTab.Rom => SettingsConstants.ResourceFirmware,
                EmulationMachineTab.Controllers => SettingsConstants.ResourceNativeControls,
                _ => SettingsConstants.ResourceNativeVideo
            };
            blocks.Add(new(resource, group.Key, resource, group.Select(option => CommonSettings.Native(configuration, option, group.Key, resource)).ToArray()));
        }
        return blocks;
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.Options.TryGetValue(OptionConstants.OptionConsole, out var console) && console != MediaRuntimeConstants.ConsoleModel(configuration.Model))
            throw new ArgumentException(nameof(configuration.Options));
    }
}
