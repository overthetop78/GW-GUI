using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Skyemu.Constants;

internal static class OptionConstants
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new("system_core_override", "Emulation.Option.skyemu.system_core_override", "Emulation.Option.skyemu.system_core_override.Help", "system", "Automatic", "Automatic", [new("Automatic", "Emulation.Option.skyemu.system_core_override.Value.Automatic"), new("Game Boy", "Emulation.Option.skyemu.system_core_override.Value.Game Boy"), new("Game Boy Advance", "Emulation.Option.skyemu.system_core_override.Value.Game Boy Advance"), new("Nintendo DS", "Emulation.Option.skyemu.system_core_override.Value.Nintendo DS")]),
        new("system_gb_bios_enable", "Emulation.Option.skyemu.system_gb_bios_enable", "Emulation.Option.skyemu.system_gb_bios_enable.Help", "system", "ON", "ON", [new("ON", "ON"), new("OFF", "Emulation.Option.skyemu.system_gb_bios_enable.Value.OFF")]),
        new("system_gba_bios_enable", "Emulation.Option.skyemu.system_gba_bios_enable", "Emulation.Option.skyemu.system_gba_bios_enable.Help", "system", "ON", "ON", [new("ON", "ON"), new("OFF", "Emulation.Option.skyemu.system_gba_bios_enable.Value.OFF")]),
        new("system_nds_bios_enable", "Emulation.Option.skyemu.system_nds_bios_enable", "Emulation.Option.skyemu.system_nds_bios_enable.Help", "system", "ON", "ON", [new("ON", "ON"), new("OFF", "Emulation.Option.skyemu.system_nds_bios_enable.Value.OFF")])
    ];
}
