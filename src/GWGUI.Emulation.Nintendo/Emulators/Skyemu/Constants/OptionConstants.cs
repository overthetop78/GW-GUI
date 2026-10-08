using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Skyemu.Constants;

internal static class OptionConstants
{
    internal const string CoreOverrideOption = "system_core_override";
    internal const string GameBoyCore = "Game Boy";
    internal const string GameBoyAdvanceCore = "Game Boy Advance";
    internal const string NintendoDsCore = "Nintendo DS";
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new("system_core_override", "Emulation.Option.skyemu.system_core_override", "Emulation.Option.skyemu.system_core_override.Help", "system", "Automatic", "Automatic", [new("Automatic", "Emulation.Option.skyemu.system_core_override.Value.Automatic"), new("Game Boy", "Emulation.Option.skyemu.system_core_override.Value.Game Boy"), new("Game Boy Advance", "Emulation.Option.skyemu.system_core_override.Value.Game Boy Advance"), new("Nintendo DS", "Emulation.Option.skyemu.system_core_override.Value.Nintendo DS")], Tab: EmulationMachineTab.General),
        new("system_gb_bios_enable", "Emulation.Option.skyemu.system_gb_bios_enable", "Emulation.Option.skyemu.system_gb_bios_enable.Help", "system", "ON", "ON", [new("ON", "ON"), new("OFF", "Emulation.Option.skyemu.system_gb_bios_enable.Value.OFF")], Tab: EmulationMachineTab.Rom, MachineIds: OptionMachineConstants.GameBoy),
        new("system_gba_bios_enable", "Emulation.Option.skyemu.system_gba_bios_enable", "Emulation.Option.skyemu.system_gba_bios_enable.Help", "system", "ON", "ON", [new("ON", "ON"), new("OFF", "Emulation.Option.skyemu.system_gba_bios_enable.Value.OFF")], Tab: EmulationMachineTab.Rom, MachineIds: OptionMachineConstants.GameBoyAdvance),
        new("system_nds_bios_enable", "Emulation.Option.skyemu.system_nds_bios_enable", "Emulation.Option.skyemu.system_nds_bios_enable.Help", "system", "ON", "ON", [new("ON", "ON"), new("OFF", "Emulation.Option.skyemu.system_nds_bios_enable.Value.OFF")], Tab: EmulationMachineTab.Rom, MachineIds: OptionMachineConstants.NintendoDs)
    ];
}
