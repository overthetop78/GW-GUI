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
    private readonly ComboBox _paddleSpeed = new();
    private readonly ComboBox _autofire = new();
    private readonly ComboBox _controllerCompatibility = new();
    private readonly ComboBox _digitalSensitivity = new();
    private readonly ComboBox _analogSensitivity = new();
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
        ConfigureEightBitControllerOption(_paddleSpeed, AtariEightBitSettingsCatalog.PaddleMovementSpeeds);
        ConfigureEightBitControllerOption(_autofire, AtariEightBitSettingsCatalog.AutofireModes);
        ConfigureEightBitControllerOption(_controllerCompatibility,
            AtariEightBitSettingsCatalog.ControllerCompatibilityModes);
        ConfigurePercentageOption(_digitalSensitivity);
        ConfigurePercentageOption(_analogSensitivity);
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
        _paddleSpeed.SelectedValue = view.PaddleMovementSpeed;
        _autofire.SelectedValue = view.AutofireMode;
        _controllerCompatibility.SelectedValue = view.ControllerCompatibilityMode;
        _digitalSensitivity.SelectedValue = view.DigitalSensitivity;
        _analogSensitivity.SelectedValue = view.AnalogSensitivity;
        BuildControllers(view.Ports);
        UpdateEightBitControllerOptions(view.HasEightBitControllerOptions);
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
            _mouseSpeed.SelectedItem is int speed ? speed : AtariMouseSettingsConstants.DefaultSpeedPercent,
            _paddleSpeed.SelectedValue as string ?? AtariEightBitSettingsConstants.DefaultPaddleMovementSpeed,
            _autofire.SelectedValue as string ?? AtariEightBitSettingsConstants.Disabled,
            _controllerCompatibility.SelectedValue as string ?? AtariEightBitSettingsConstants.None,
            _digitalSensitivity.SelectedValue as string ?? AtariEightBitSettingsConstants.DefaultSensitivity,
            _analogSensitivity.SelectedValue as string ?? AtariEightBitSettingsConstants.DefaultSensitivity);
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
                UpdatePaddleSpeedVisibility();
            };
            _portEditors.Add(editor);
        }
        // Detach the previous page before constructing the replacement. WPF otherwise still
        // considers its editor controls logical children while the new visual tree is built.
        _controllers.Content = null;
        var behaviors = AtariEightBitSettingsCatalog.SupportsComputerOptions(_configurationModel)
            ? new[]
            {
                new EmulationSettingsField(LocExtension.Get("Emulation.Atari.Controller.PaddleSpeed"), _paddleSpeed),
                new EmulationSettingsField(LocExtension.Get("Emulation.Atari.Controller.Autofire"), _autofire),
                new EmulationSettingsField(LocExtension.Get("Emulation.Atari.Controller.Compatibility"),
                    _controllerCompatibility),
                new EmulationSettingsField(LocExtension.Get("Emulation.Atari.Controller.DigitalSensitivity"),
                    _digitalSensitivity),
                new EmulationSettingsField(LocExtension.Get("Emulation.Atari.Controller.AnalogSensitivity"),
                    _analogSensitivity)
            }
            : [];
        _controllers.Content = behaviors.Length == 0
            ? _controllerSection.Build(_portEditors.Select(editor => editor.Settings).ToArray())
            : _controllerSection.Build(_portEditors.Select(editor => editor.Settings).ToArray(), behaviors,
                LocExtension.Get("Emulation.Input.Behavior"), ControlVisualConstants.GameControllerGlyph);
        _ = _controllerSection.DetectAsync();
    }

    private static void ConfigureEightBitControllerOption(ComboBox selector, IEnumerable<string> values)
    {
        selector.ItemsSource = values.Select(value => new AtariControllerOptionChoice(value,
            value switch
            {
                AtariEightBitSettingsConstants.Disabled => LocExtension.Get("Emulation.Value.Disabled"),
                AtariEightBitSettingsConstants.AutofireOnButton =>
                    LocExtension.Get("Emulation.Atari.Controller.AutofireButton"),
                AtariEightBitSettingsConstants.AutofireAlways =>
                    LocExtension.Get("Emulation.Atari.Controller.AutofireAlways"),
                AtariEightBitSettingsConstants.None => LocExtension.Get("Emulation.Controller.None"),
                AtariEightBitSettingsConstants.DualStick =>
                    LocExtension.Get("Emulation.Atari.Controller.DualStick"),
                AtariEightBitSettingsConstants.SwapPorts =>
                    LocExtension.Get("Emulation.Atari.Controller.SwapPorts"),
                _ => value
            })).ToArray();
        selector.DisplayMemberPath = nameof(AtariControllerOptionChoice.Label);
        selector.SelectedValuePath = nameof(AtariControllerOptionChoice.Value);
    }

    private static void ConfigurePercentageOption(ComboBox selector)
    {
        selector.ItemsSource = AtariEightBitSettingsCatalog.Sensitivities.Select(value =>
            new AtariControllerOptionChoice(value, $"{value} %")).ToArray();
        selector.DisplayMemberPath = nameof(AtariControllerOptionChoice.Label);
        selector.SelectedValuePath = nameof(AtariControllerOptionChoice.Value);
    }

    private void UpdateEightBitControllerOptions(bool visible)
    {
        _autofire.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        _controllerCompatibility.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        _digitalSensitivity.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        _analogSensitivity.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        UpdatePaddleSpeedVisibility();
    }

    private void UpdatePaddleSpeedVisibility()
    {
        var paddleSelected = AtariEightBitSettingsCatalog.SupportsComputerOptions(_configurationModel)
            && _portEditors.Any(editor => editor.Type.SelectedItem is AtariPeripheralChoice choice
                && choice.Value == AtariPeripheralKind.Paddle);
        _paddleSpeed.Visibility = paddleSelected ? Visibility.Visible : Visibility.Collapsed;
        // The native paddle mode disables Dual Stick and Swap Ports, so do not offer a conflicting hack.
        _controllerCompatibility.Visibility = AtariEightBitSettingsCatalog.SupportsComputerOptions(
            _configurationModel) && !paddleSelected ? Visibility.Visible : Visibility.Collapsed;
    }

    private sealed record AtariPeripheralChoice(AtariPeripheralKind Value, string Label);
    private sealed record AtariControllerOptionChoice(string Value, string Label);
}
