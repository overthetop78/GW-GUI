using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;

namespace GWGUI.App.Controls;

internal sealed class EmulationControllerSettingsSection
{
    private TextBlock _detectedControllers = CreateDetectionStatus();
    private IReadOnlyList<EmulationControllerPortSettings> _ports = [];

    internal static EmulationControllerPortEditor CreatePort(int number,
        InputCaptureSources captureSources, bool prefixKeyboardSource,
        string actionLabel, string searchLabel) => new(number, captureSources,
        prefixKeyboardSource, actionLabel, searchLabel);

    internal UIElement Build(IReadOnlyList<EmulationControllerPortSettings> ports,
        EmulationSettingsControlField? behavior = null, string? behaviorTitle = null,
        string? behaviorGlyph = null)
    {
        _ports = ports;
        _detectedControllers = CreateDetectionStatus();
        return EmulationSettingsLayout.ControllerSettingsPage(ports, _detectedControllers,
            DetectAsync, behavior, behaviorTitle, behaviorGlyph);
    }

    internal UIElement Build(IReadOnlyList<EmulationControllerPortSettings> ports,
        IReadOnlyList<EmulationSettingsControlField> behaviors,
        string? behaviorTitle = null, string? behaviorGlyph = null)
    {
        _ports = ports;
        _detectedControllers = CreateDetectionStatus();
        return EmulationSettingsLayout.ControllerSettingsPage(ports, _detectedControllers,
            DetectAsync, behaviors, behaviorTitle, behaviorGlyph);
    }

    internal Task DetectAsync() => EmulationSettingsLayout.DetectControllersAsync(
        _ports.Select(port => port.Device).ToArray(), _detectedControllers);

    private static TextBlock CreateDetectionStatus() => new()
    {
        Text = LocExtension.Get("Emulation.Controller.NoneDetected"),
        TextWrapping = TextWrapping.Wrap,
        VerticalAlignment = VerticalAlignment.Center
    };
}
