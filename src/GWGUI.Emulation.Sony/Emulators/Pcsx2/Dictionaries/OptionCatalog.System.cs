using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Enums;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Dictionaries;

internal static partial class OptionCatalog
{
    private static IReadOnlyList<CoreOption> SystemOptions { get; } =
    [
        Define(OptionConstants.BiosKey, OptionConstants.BiosLabel, null,
            "", EmulationMachineTab.Rom,
            [], visible: false),
        Define(OptionConstants.FastbootKey, OptionConstants.FastbootLabel, OptionConstants.FastbootHelp,
            "enabled", EmulationMachineTab.Rom,
            ToggleChoices, restart: true),
        Define(OptionConstants.FastcdvdKey, OptionConstants.FastcdvdLabel, OptionConstants.FastcdvdHelp,
            "disabled", EmulationMachineTab.Storage,
            ToggleChoices, restart: true),
        Define(OptionConstants.SharedMemoryCardsKey, OptionConstants.SharedMemoryCardsLabel, OptionConstants.SharedMemoryCardsHelp,
            "enabled", EmulationMachineTab.Storage,
            ToggleChoices, restart: true, visible: false),
        Define(OptionConstants.EnableCheatsKey, OptionConstants.EnableCheatsLabel, OptionConstants.EnableCheatsHelp,
            "disabled", EmulationMachineTab.General,
            ToggleChoices),
        Define(OptionConstants.HintLanguageUnlockKey, OptionConstants.HintLanguageUnlockLabel, OptionConstants.HintLanguageUnlockHelp,
            "disabled", EmulationMachineTab.General,
            ToggleChoices, restart: true),
        Define(OptionConstants.Dev9HddKey, OptionConstants.Dev9HddLabel, OptionConstants.Dev9HddHelp,
            "disabled", EmulationMachineTab.Storage,
            ToggleChoices, visible: false),
        Define(OptionConstants.Dev9HddSizeKey, OptionConstants.Dev9HddSizeLabel, OptionConstants.Dev9HddSizeHelp,
            HardDiskRuntimeFunctions.NativeCapacity(HardDiskConstants.DefaultCapacityGiB), EmulationMachineTab.Storage,
            HardDiskConstants.CapacitiesGiB.Select(capacity =>
            {
                var value = HardDiskRuntimeFunctions.NativeCapacity(capacity);
                return new CoreOptionValue(value, value);
            }).ToArray(), visible: false),
        Define(OptionConstants.Dev9EthKey, OptionConstants.Dev9EthLabel, OptionConstants.Dev9EthHelp,
            "disabled", EmulationMachineTab.General,
            ToggleChoices),
        Define(OptionConstants.UseExternalGameindexKey, OptionConstants.UseExternalGameindexLabel, OptionConstants.UseExternalGameindexHelp,
            "disabled", EmulationMachineTab.General,
            ToggleChoices, restart: true),
    ];
}
