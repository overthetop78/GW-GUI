using System.Globalization;
using GWGUI.Emulation.Watara.Emulators.Potator.Enums;

namespace GWGUI.Emulation.Watara.Emulators.Potator.Dictionaries;

internal static class SettingsDictionary
{
    internal static readonly IReadOnlyList<EmulationSettingsChoice> Palettes = [
        new("default", "Emulation.Video.DotMatrix.Palette.Gray"),
        new("potator_amber", string.Empty, "Potator Amber"),
        new("potator_green", string.Empty, "Potator Green"),
        new("potator_blue", string.Empty, "Potator Blue"),
        new("potator_bgb", string.Empty, "Potator BGB"),
        new("potator_wataroo", string.Empty, "Potator Wataroo"),
        new("gb_dmg", string.Empty, "Game Boy DMG"),
        new("gb_pocket", string.Empty, "Game Boy Pocket"),
        new("gb_light", string.Empty, "Game Boy Light"),
        new("blossom_pink", string.Empty, "Blossom Pink"),
        new("bubbles_blue", string.Empty, "Bubbles Blue"),
        new("buttercup_green", string.Empty, "Buttercup Green"),
        new("digivice", string.Empty, "Digivice"),
        new("game_com", string.Empty, "Game.com"),
        new("gameking", string.Empty, "GameKing"),
        new("game_master", string.Empty, "Game Master"),
        new("golden_wild", string.Empty, "Golden Wild"),
        new("greenscale", string.Empty, "Greenscale"),
        new("hokage_orange", string.Empty, "Hokage Orange"),
        new("labo_fawn", string.Empty, "Labo Fawn"),
        new("legendary_super_saiyan", string.Empty, "Legendary Super Saiyan"),
        new("microvision", string.Empty, "Microvision"),
        new("million_live_gold", string.Empty, "Million Live Gold"),
        new("odyssey_gold", string.Empty, "Odyssey Gold"),
        new("shiny_sky_blue", string.Empty, "Shiny Sky Blue"),
        new("slime_blue", string.Empty, "Slime Blue"),
        new("ti_83", string.Empty, "TI-83"),
        new("travel_wood", string.Empty, "Travel Wood"),
        new("virtual_boy", string.Empty, "Virtual Boy"),
        new("tvlink", string.Empty, "TV-Link"),
        new("tvlink_inverted", string.Empty, "TV-Link Inverted"),
    ];
    internal static readonly IReadOnlyDictionary<FrameSkipMode, string> FrameSkipValues = new Dictionary<FrameSkipMode, string> { [FrameSkipMode.Disabled] = "disabled", [FrameSkipMode.Automatic] = "auto", [FrameSkipMode.Manual] = "manual" };
    internal static IReadOnlyList<EmulationSettingsChoice> Ghosting => Enumerable.Range(0, 9).Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, value.ToString(CultureInfo.InvariantCulture))).ToArray();
    internal static IReadOnlyList<EmulationSettingsChoice> Thresholds => Enumerable.Range(0, 16).Select(index => (15 + index * 3).ToString(CultureInfo.InvariantCulture)).Select(value => new EmulationSettingsChoice(value, string.Empty, value + " %")).ToArray();
    internal static IReadOnlyList<EmulationSettingsChoice> FrameSkip => [new(FrameSkipValues[FrameSkipMode.Disabled], "Emulation.Value.Disabled"), new(FrameSkipValues[FrameSkipMode.Automatic], "Emulation.Controller.Automatic"), new(FrameSkipValues[FrameSkipMode.Manual], "Emulation.Watara.Video.Manual")];
}