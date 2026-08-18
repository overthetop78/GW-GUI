using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal sealed class AtariInputSettingsSection
{
    private readonly InputBindingEditor _keyboard = new();
    private readonly InputBindingEditor _mouseBindings = new();
    private readonly ContentControl _controllers = new();
    private readonly EmulationControllerSettingsSection _controllerSection = new();
    private readonly ComboBox _mouseSpeed = new();
    private readonly Border _keyboardUnavailable = EmulationSettingsLayout.InformationBanner(
        LocExtension.Get(AtariInputSettingsConstants.NoKeyboardResource));
    private readonly Border _mouseUnavailable = EmulationSettingsLayout.InformationBanner(
        LocExtension.Get(AtariInputSettingsConstants.NoMouseResource));
    private readonly List<EmulationControllerPortEditor> _portEditors = [];
    private AtariMachineModel _configurationModel;
    internal UIElement Keyboard => BuildKeyboardPage();
    internal UIElement Mouse => BuildMousePage();
    internal UIElement Controllers => _controllers;

    internal AtariInputSettingsSection()
    {
        _keyboard.ConfigurePresentation(LocExtension.Get("Emulation.Keyboard.SystemKey", AtariEmulationConstants.AtariTitle),
            LocExtension.Get(AtariInputSettingsConstants.SearchKeyResource));
        AtariAccessibilityFunctions.Configure(_keyboard,
            LocExtension.Get(AtariInputSettingsConstants.KeyboardTabResource));
        _mouseSpeed.ItemsSource = AtariMouseSettingsFunctions.Speeds();
        _mouseBindings.ConfigureCaptureSources(
            InputCaptureSources.Keyboard | InputCaptureSources.Mouse | InputCaptureSources.Controller, true);
        _mouseBindings.ConfigurePresentation(AtariLocalizedText("Emulation.Controller.EmulatedAction"),
            LocExtension.Get("Emulation.Controller.SearchBinding"));
    }

    internal void Load(AtariMachineConfiguration configuration)
    {
        _configurationModel = configuration.Model;
        var view = AtariInputSettingsFunctions.Create(configuration);
        _keyboard.IsEnabled = view.HasKeyboard;
        _keyboardUnavailable.Visibility = view.HasKeyboard ? Visibility.Collapsed : Visibility.Visible;
        _keyboard.SetRows(view.KeyboardDefinitions, view.KeyboardBindings);
        _mouseBindings.IsEnabled = view.HasMouse;
        _mouseSpeed.IsEnabled = view.HasMouse;
        _mouseUnavailable.Visibility = view.HasMouse ? Visibility.Collapsed : Visibility.Visible;
        _mouseSpeed.SelectedItem = view.MouseSpeedPercent;
        _mouseBindings.SetRows(view.MouseDefinitions, view.MouseBindings);
        BuildControllers(view.Ports);
    }

    internal AtariMachineConfiguration Apply(AtariMachineConfiguration configuration)
    {
        var controllers = _portEditors.Select(editor => new AtariControllerBinding(editor.Number - 1,
            editor.Type.SelectedItem is AtariPeripheralChoice selected
                ? selected.Value : AtariControllerSettingsFunctions.DefaultPeripheral(_configurationModel),
            (editor.Device.SelectedItem as GameControllerDevice)?.Id ?? editor.Device.Tag as string,
            Mappings: editor.Bindings.Rows.ToDictionary(row => row.Id, row => row.Binding, StringComparer.Ordinal),
            DeadZonePercent: editor.DeadZonePercent)).ToArray();
        return AtariInputSettingsFunctions.Apply(configuration, _keyboard.Rows, _mouseBindings.Rows, controllers,
            configuration.Input.CaptureMouse, configuration.Input.ReleaseMouseKey,
            _mouseSpeed.SelectedItem is int speed ? speed : AtariMouseSettingsConstants.DefaultSpeedPercent);
    }

    private UIElement BuildKeyboardPage()
    {
        _keyboardUnavailable.Visibility = Visibility.Collapsed;
        return EmulationSettingsLayout.KeyboardSettingsPage(_keyboard,
            AtariLocalizedText("Emulation.Keyboard.SpecialKeysOnlyHint"),
            _keyboardUnavailable);
    }

    private UIElement BuildMousePage()
    {
        _mouseUnavailable.Visibility = Visibility.Collapsed;
        return EmulationSettingsLayout.MouseSettingsPage(
            [new(LocExtension.Get("Emulation.Mouse.Speed"), _mouseSpeed)], null,
            _mouseBindings, _mouseUnavailable);
    }

    private static string AtariLocalizedText(string resource) => LocExtension.Get(resource).Replace(
        ControlVisualConstants.AmigaTitle, AtariEmulationConstants.AtariTitle, StringComparison.Ordinal);

    private void BuildControllers(IReadOnlyList<AtariControllerPortView> ports)
    {
        _portEditors.Clear();
        foreach (var port in ports)
        {
            var editor = EmulationControllerSettingsSection.CreatePort(
                port.Port + AtariInputSettingsConstants.InclusiveEndpointCount,
                InputCaptureSources.Keyboard | InputCaptureSources.Controller, true,
                AtariLocalizedText("Emulation.Controller.EmulatedAction"),
                LocExtension.Get("Emulation.Controller.SearchBinding"));
            editor.DeadZonePercent = port.DeadZonePercent;
            editor.Type.DisplayMemberPath = nameof(AtariPeripheralChoice.Label);
            var peripheralChoices = port.Peripherals.Select(value => new AtariPeripheralChoice(
                value, AtariControllerSettingsFunctions.PeripheralLabel(_configurationModel, value))).ToArray();
            editor.Type.ItemsSource = peripheralChoices;
            editor.Type.SelectedItem = peripheralChoices.FirstOrDefault(choice => choice.Value == port.Selected);
            if (port.DeviceId is not null)
            {
                editor.Device.Tag = port.DeviceId;
            }
            editor.Bindings.SetRows(port.Definitions, port.Bindings);
            editor.Type.SelectionChanged += (_, _) =>
            {
                if (editor.Type.SelectedItem is AtariPeripheralChoice selected)
                    editor.Bindings.SetRows(AtariControllerSettingsFunctions.Definitions(
                        _configurationModel, selected.Value, port.Port), port.Bindings);
            };
            _portEditors.Add(editor);
        }
        // Detach the previous page before constructing the replacement. WPF otherwise still
        // considers its editor controls logical children while the new visual tree is built.
        _controllers.Content = null;
        _controllers.Content = _controllerSection.Build(_portEditors.Select(editor => editor.Settings).ToArray());
        _ = _controllerSection.DetectAsync();
    }

    private sealed record AtariPeripheralChoice(AtariPeripheralKind Value, string Label);
}
