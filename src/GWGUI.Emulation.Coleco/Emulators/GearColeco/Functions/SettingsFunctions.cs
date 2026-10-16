using GWGUI.Emulation.Coleco.Emulators.Common.Constants;
using GWGUI.Emulation.Coleco.Emulators.GearColeco.Constants;
using GWGUI.Emulation.Coleco.Emulators.GearColeco.Dictionaries;
using CommonSettings = GWGUI.Emulation.Coleco.Common.Machines.Common.Functions.SettingsDescriptionFunctions;

namespace GWGUI.Emulation.Coleco.Emulators.GearColeco.Functions;

internal static class SettingsDescriptionFunctions
{
    internal static IReadOnlyList<CoreOption> GetOptions() => GearColecoOptionCatalog.All.Select(option => option with { IsVisible = IsExposed(option.Key) }).ToArray();

    private static bool IsExposed(string key) => key is not (OptionConstants.OptionCartridgeHardware or OptionConstants.OptionAdamDiskDrive or OptionConstants.OptionAdamWritableMedia or OptionConstants.OptionSpinners or OptionConstants.OptionAdamComputerReset or OptionConstants.OptionMapper);

    internal static IReadOnlyList<EmulationSettingsBlock> Create(MachineConfiguration configuration)
    {
        var blocks = new List<EmulationSettingsBlock>();
        foreach (var group in GetOptions().Where(option => option.IsVisible).GroupBy(option => option.Category == "input" ? EmulationMachineTab.Controllers : EmulationMachineTab.Video))
        {
            var resource = group.Key == EmulationMachineTab.Controllers ? SettingsConstants.ResourceNativeControls : SettingsConstants.ResourceNativeVideo;
            blocks.Add(new(resource, group.Key, resource, group.Select(option => CommonSettings.Native(configuration, option, group.Key, resource)).ToArray()));
        }
        var cartridge = GearColecoOptionCatalog.All.Single(option => option.Key == OptionConstants.OptionMapper);
        var cartridgeField = CommonSettings.Native(configuration, cartridge, EmulationMachineTab.Storage, "Emulation.Storage.Cartridges")
            with { DetailedExplanationResourceKey = SettingsTextConstants.CartridgeTypeDetails };
        blocks.Add(new("Emulation.Storage.Cartridges", EmulationMachineTab.Storage, "Emulation.Storage.Cartridges", [cartridgeField]));
        return blocks;
    }

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.Options.TryGetValue(OptionConstants.OptionCartridgeHardware, out var hardware)
            && hardware != (configuration.Model == MachineModel.Adam ? CoreModelConstants.Adam : CoreModelConstants.ColecoVision))
            throw new ArgumentException(nameof(configuration.Options));
    }
}
