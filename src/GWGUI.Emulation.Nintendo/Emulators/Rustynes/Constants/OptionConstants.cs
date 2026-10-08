using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Rustynes.Constants;

internal static class OptionConstants
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new("rustynes_four_score", "Emulation.Option.rustynes.rustynes_four_score", null, "", "disabled", "disabled", [new("disabled", "Emulation.Option.rustynes.rustynes_four_score.Value.disabled"), new("enabled", "Emulation.Option.rustynes.rustynes_four_score.Value.enabled")])
    ];
}
