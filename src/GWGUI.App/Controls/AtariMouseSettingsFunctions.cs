using GWGUI.App.Input;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

internal static class AtariMouseSettingsFunctions
{
    internal static IReadOnlyList<InputBindingDefinition> Definitions() => AtariMouseSettingsConstants.Actions
        .Select(action => new InputBindingDefinition(action, Label(action), InputBindingSyntax.Mouse(action))).ToArray();

    internal static IReadOnlyList<int> Speeds() => Enumerable.Range(
            AtariMouseSettingsConstants.MinimumSpeedPercent / AtariMouseSettingsConstants.SpeedStepPercent,
            (AtariMouseSettingsConstants.MaximumSpeedPercent - AtariMouseSettingsConstants.MinimumSpeedPercent)
            / AtariMouseSettingsConstants.SpeedStepPercent + AtariInputSettingsConstants.InclusiveEndpointCount)
        .Select(value => value * AtariMouseSettingsConstants.SpeedStepPercent).ToArray();

    private static string Label(string action) => action switch
    {
        "Left" => LocExtension.Get("Emulation.Mouse.Button.Left"),
        "Right" => LocExtension.Get("Emulation.Mouse.Button.Right"), _ => action
    };
}
