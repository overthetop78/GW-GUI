using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.BeetleGba.Constants;

internal static class OptionConstants
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new("gba_hle", "Emulation.Option.mednafen_gba.gba_hle", null, "", "enabled", "enabled", [new("enabled", "Emulation.Option.mednafen_gba.gba_hle.Value.enabled"), new("disabled", "Emulation.Option.mednafen_gba.gba_hle.Value.disabled")]),
        new("gba_use_mednafen_save_method", "Emulation.Option.mednafen_gba.gba_use_mednafen_save_method", null, "", "mednafen", "mednafen", [new("mednafen", "Emulation.Option.mednafen_gba.gba_use_mednafen_save_method.Value.mednafen"), new("libretro", "Emulation.Option.mednafen_gba.gba_use_mednafen_save_method.Value.libretro")])
    ];
}
