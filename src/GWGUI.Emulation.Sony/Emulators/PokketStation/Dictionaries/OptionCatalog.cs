using GWGUI.Emulation.Sony.Emulators.PokketStation.Constants;
using GWGUI.Emulation.Sony.Emulators.PokketStation.Enums;
using GWGUI.Emulation.Sony.Emulators.PokketStation.Functions;

namespace GWGUI.Emulation.Sony.Emulators.PokketStation.Dictionaries;

internal static class OptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(LinkConstants.LayoutKey, LinkConstants.LayoutLabel, LinkConstants.Help, null, LinkConstants.SideBySide, LinkConstants.SideBySide,
        [
            new(LinkFunctions.ToNative(LinkLayout.SideBySide), LinkConstants.HorizontalLabel),
            new(LinkFunctions.ToNative(LinkLayout.Stacked), LinkConstants.VerticalLabel)
        ], Tab: EmulationMachineTab.Video),
        new(LinkConstants.AudioKey, LinkConstants.AudioLabel, LinkConstants.Help, null, LinkConstants.BothUnits, LinkConstants.BothUnits,
        [
            new(LinkFunctions.ToNative(LinkAudio.BothUnits), "A + B"),
            new(LinkFunctions.ToNative(LinkAudio.UnitA), "A"),
            new(LinkFunctions.ToNative(LinkAudio.UnitB), "B"),
            new(LinkFunctions.ToNative(LinkAudio.Split), "A → L / B → R")
        ], Tab: EmulationMachineTab.Audio),
        new(LinkConstants.IdentityKey, LinkConstants.IdentityLabel, LinkConstants.Help, null, LinkConstants.DifferentIdentity, LinkConstants.DifferentIdentity,
        [
            new(LinkFunctions.ToNative(LinkIdentity.Different), "A ≠ B"),
            new(LinkFunctions.ToNative(LinkIdentity.Same), "A = B")
        ], Tab: EmulationMachineTab.General, RequiresRestart: true)
    ];
}
