using GWGUI.App.Contracts.Emulation.Controllers;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Enums.Input;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Views.Controls.Emulation.Input;
using System.Windows;


namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed class EmulationControllerSettingsSection
{
    internal static EmulationControllerPortEditor CreatePort(int number,
        InputCaptureSources captureSources, bool prefixKeyboardSource,
        string actionLabel, string searchLabel, string moduleId, string machineId) =>
        new(number, captureSources, prefixKeyboardSource, actionLabel, searchLabel,
            moduleId, machineId);

    internal UIElement Build(IReadOnlyList<EmulationControllerPortSettings> ports,
        EmulationSettingsControlField? behavior = null, string? behaviorTitle = null,
        string? behaviorGlyph = null)
    {
        return EmulationSettingsLayout.ControllerSettingsPage(
            ports, behavior, behaviorTitle, behaviorGlyph);
    }

    internal UIElement Build(IReadOnlyList<EmulationControllerPortSettings> ports,
        IReadOnlyList<EmulationSettingsControlField> behaviors,
        string? behaviorTitle = null, string? behaviorGlyph = null)
    {
        return EmulationSettingsLayout.ControllerSettingsPage(
            ports, behaviors, behaviorTitle, behaviorGlyph);
    }
}
