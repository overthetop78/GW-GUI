using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Snes9x2005.Constants;

internal static class OptionConstants
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new("snes9x_2005_region", "Emulation.Option.snes9x2005.snes9x_2005_region", "Emulation.Option.snes9x2005.snes9x_2005_region.Help", "", "auto", "auto", [new("auto", "Emulation.Option.snes9x2005.snes9x_2005_region.Value.auto"), new("NTSC", "Emulation.Option.snes9x2005.snes9x_2005_region.Value.NTSC"), new("PAL", "Emulation.Option.snes9x2005.snes9x_2005_region.Value.PAL")]),
        new("snes9x_2005_frameskip", "Emulation.Option.snes9x2005.snes9x_2005_frameskip", "Emulation.Option.snes9x2005.snes9x_2005_frameskip.Help", "", "disabled", "disabled", [new("disabled", "Emulation.Option.snes9x2005.snes9x_2005_frameskip.Value.disabled"), new("auto", "Emulation.Option.snes9x2005.snes9x_2005_frameskip.Value.auto"), new("manual", "Emulation.Option.snes9x2005.snes9x_2005_frameskip.Value.manual")]),
        new("snes9x_2005_frameskip_threshold", "Emulation.Option.snes9x2005.snes9x_2005_frameskip_threshold", "Emulation.Option.snes9x2005.snes9x_2005_frameskip_threshold.Help", "", "33", "33", [new("15", "15"), new("18", "18"), new("21", "21"), new("24", "24"), new("27", "27"), new("30", "30"), new("33", "33"), new("36", "36"), new("39", "39"), new("42", "42"), new("45", "45"), new("48", "48"), new("51", "51"), new("54", "54"), new("57", "57"), new("60", "60")]),
        new("snes9x_2005_low_pass_filter", "Emulation.Option.snes9x2005.snes9x_2005_low_pass_filter", "Emulation.Option.snes9x2005.snes9x_2005_low_pass_filter.Help", "", "disabled", "disabled", [new("disabled", "Emulation.Option.snes9x2005.snes9x_2005_low_pass_filter.Value.disabled"), new("enabled", "Emulation.Option.snes9x2005.snes9x_2005_low_pass_filter.Value.enabled")]),
        new("snes9x_2005_low_pass_range", "Emulation.Option.snes9x2005.snes9x_2005_low_pass_range", "Emulation.Option.snes9x2005.snes9x_2005_low_pass_range.Help", "", "60", "60", [new("5", "5%"), new("10", "10%"), new("15", "15%"), new("20", "20%"), new("25", "25%"), new("30", "30%"), new("35", "35%"), new("40", "40%"), new("45", "45%"), new("50", "50%"), new("55", "55%"), new("60", "60%"), new("65", "65%"), new("70", "70%"), new("75", "75%"), new("80", "80%"), new("85", "85%"), new("90", "90%"), new("95", "95%")]),
        new("snes9x_2005_overclock_cycles", "Emulation.Option.snes9x2005.snes9x_2005_overclock_cycles", "Emulation.Option.snes9x2005.snes9x_2005_overclock_cycles.Help", "hacks", "disabled", "disabled", [new("disabled", "Emulation.Option.snes9x2005.snes9x_2005_overclock_cycles.Value.disabled"), new("compatible", "Emulation.Option.snes9x2005.snes9x_2005_overclock_cycles.Value.compatible"), new("max", "Emulation.Option.snes9x2005.snes9x_2005_overclock_cycles.Value.max")]),
        new("snes9x_2005_reduce_sprite_flicker", "Emulation.Option.snes9x2005.snes9x_2005_reduce_sprite_flicker", "Emulation.Option.snes9x2005.snes9x_2005_reduce_sprite_flicker.Help", "hacks", "disabled", "disabled", [new("disabled", "Emulation.Option.snes9x2005.snes9x_2005_reduce_sprite_flicker.Value.disabled"), new("enabled", "Emulation.Option.snes9x2005.snes9x_2005_reduce_sprite_flicker.Value.enabled")])
    ];
}
