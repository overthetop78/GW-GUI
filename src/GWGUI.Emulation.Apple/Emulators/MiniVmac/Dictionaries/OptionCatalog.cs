using GWGUI.Emulation.Apple.Common.Contracts;
using GWGUI.Emulation.Apple.Emulators.MiniVmac.Constants;

namespace GWGUI.Emulation.Apple.Emulators.MiniVmac.Dictionaries;

internal static class MiniVmacOptionCatalog
{
    internal static IReadOnlyList<CoreOption> All { get; } =
    [
        new(OptionConstants.Statusbar, "Status Bar", null, null,
            "disabled", "disabled",
            [
                new("disabled", "disabled", ResourceKey: "Emulation.Value.Disabled"),
                new("enabled", "enabled", ResourceKey: "Emulation.Value.Enabled"),
            ], ResourceKey: SettingsTextConstants.SettingStatusBar),
        new(OptionConstants.Kbdtype, "Keyboard Type", null, null,
            "Callback", "Callback",
            [
                new("Callback", "Callback"),
                new("Poll", "Poll"),
            ]),
    ];
}
