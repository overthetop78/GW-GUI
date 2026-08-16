using System.Globalization;
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
    private readonly StackPanel _controllers = new();
    private readonly ComboBox _mouseSpeed = new();
    private readonly List<ControllerEditor> _portEditors = [];
    internal UIElement Keyboard => BuildKeyboardPage();
    internal UIElement Mouse => BuildMousePage();
    internal UIElement Controllers => EmulationSettingsLayout.ScrollPage(_controllers);

    internal AtariInputSettingsSection()
    {
        _keyboard.ConfigurePresentation(LocExtension.Get(AtariInputSettingsConstants.AtariKeyResource),
            LocExtension.Get(AtariInputSettingsConstants.SearchKeyResource));
        AtariAccessibilityFunctions.Configure(_keyboard,
            LocExtension.Get(AtariInputSettingsConstants.KeyboardTabResource));
        _mouseSpeed.ItemsSource = AtariInputSettingsFunctions.MouseSpeeds();
        _mouseBindings.ConfigureCaptureSources(
            InputCaptureSources.Keyboard | InputCaptureSources.Mouse | InputCaptureSources.Controller, true);
    }

    internal void Load(AtariMachineConfiguration configuration)
    {
        var view = AtariInputSettingsFunctions.Create(configuration);
        _keyboard.IsEnabled = view.HasKeyboard;
        _keyboard.SetRows(view.KeyboardDefinitions, view.KeyboardBindings);
        _mouseBindings.IsEnabled = view.HasMouse;
        _mouseSpeed.IsEnabled = view.HasMouse;
        _mouseSpeed.SelectedItem = view.MouseSpeedPercent;
        _mouseBindings.SetRows(view.MouseDefinitions, view.MouseBindings);
        BuildControllers(view.Ports);
    }

    internal AtariMachineConfiguration Apply(AtariMachineConfiguration configuration)
    {
        var controllers = _portEditors.Select(editor => new AtariControllerBinding(editor.Port,
            editor.Peripheral.SelectedItem is AtariPeripheralKind selected ? selected : AtariPeripheralKind.Automatic,
            editor.Device.SelectedItem as string,
            Mappings: editor.Bindings.Rows.ToDictionary(row => row.Id, row => row.Binding, StringComparer.Ordinal),
            DeadZonePercent: editor.DeadZone.SelectedItem is int deadZone
                ? deadZone : AtariControllerConstants.DefaultDeadZonePercent)).ToArray();
        return AtariInputSettingsFunctions.Apply(configuration, _keyboard.Rows, _mouseBindings.Rows, controllers,
            configuration.Input.CaptureMouse, configuration.Input.ReleaseMouseKey,
            _mouseSpeed.SelectedItem is int speed ? speed : AtariInputSettingsConstants.DefaultMouseSpeedPercent);
    }

    private UIElement BuildKeyboardPage()
    {
        var page = new Grid { Margin = new Thickness(12) };
        page.Children.Add(EmulationSettingsLayout.InputBindings(_keyboard,
            LocExtension.Get("Emulation.InputActions"), LocExtension.Get("Emulation.SpecialKeysOnlyHint")));
        return page;
    }

    private UIElement BuildMousePage()
    {
        var root = new Grid { Margin = new Thickness(12) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.Children.Add(EmulationSettingsLayout.IconCard(EmulationSettingsLayout.CompactForm(1,
            (LocExtension.Get("Emulation.MouseSpeed"), _mouseSpeed)),
            LocExtension.Get("Emulation.MouseTab"), "\uE962"));
        var bindings = EmulationSettingsLayout.InputBindings(_mouseBindings,
            LocExtension.Get("Emulation.MouseActions"), LocExtension.Get("Emulation.InputCaptureHint"));
        bindings.Margin = new Thickness(0, 10, 0, 0);
        Grid.SetRow(bindings, 1); root.Children.Add(bindings);
        return EmulationSettingsLayout.ScrollPage(root);
    }

    private void BuildControllers(IReadOnlyList<AtariControllerPortView> ports)
    {
        _controllers.Children.Clear();
        _controllers.Margin = new Thickness(12);
        _portEditors.Clear();
        var detection = new TextBlock
        {
            Text = LocExtension.Get(AtariInputSettingsConstants.NoControllerResource),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0, 0, 0)
        };
        var detect = new Button { Content = LocExtension.Get(AtariInputSettingsConstants.DetectControllersResource) };
        AtariAccessibilityFunctions.Configure(detect,
            LocExtension.Get(AtariInputSettingsConstants.DetectControllersResource));
        AtariAccessibilityFunctions.Configure(detection,
            LocExtension.Get(AtariAccessibilityConstants.ControllerStatusResource));
        detect.Click += (_, _) =>
        {
            var count = XInputControllerReader.ReadAll().Count;
            detection.Text = count == AtariInputSettingsConstants.NoControllerCount
                ? LocExtension.Get(AtariInputSettingsConstants.NoControllerResource)
                : count.ToString(CultureInfo.CurrentCulture);
            var devices = AtariInputSettingsFunctions.ControllerDeviceIds(count);
            foreach (var editor in _portEditors) editor.Device.ItemsSource = devices;
        };
        var detectionContent = new StackPanel { Orientation = Orientation.Horizontal };
        detectionContent.Children.Add(detect);
        detectionContent.Children.Add(detection);
        _controllers.Children.Add(EmulationSettingsLayout.IconCard(detectionContent,
            LocExtension.Get("Emulation.DetectedControllers"), ControlVisualConstants.GameControllerGlyph));
        var portCards = new Grid { Margin = new Thickness(0, 10, 0, 0) };
        portCards.ColumnDefinitions.Add(new ColumnDefinition());
        portCards.ColumnDefinitions.Add(new ColumnDefinition());
        foreach (var port in ports)
        {
            var peripheral = new ComboBox { ItemsSource = port.Peripherals, SelectedItem = port.Selected };
            var device = new ComboBox();
            if (port.DeviceId is not null)
            {
                device.ItemsSource = new[] { port.DeviceId };
                device.SelectedItem = port.DeviceId;
            }
            var deadZone = new ComboBox
            {
                ItemsSource = Enumerable.Range(AtariControllerConstants.MinimumDeadZonePercent,
                    AtariControllerConstants.MaximumDeadZonePercent - AtariControllerConstants.MinimumDeadZonePercent
                    + AtariInputSettingsConstants.InclusiveEndpointCount),
                SelectedItem = port.DeadZonePercent
            };
            var bindings = new InputBindingEditor();
            bindings.ConfigureCaptureSources(InputCaptureSources.Keyboard | InputCaptureSources.Controller, true);
            bindings.SetRows(port.Definitions, port.Bindings);
            var group = new StackPanel();
            group.Children.Add(AtariAccessibilityFunctions.LabeledRow(
                LocExtension.Get(AtariInputSettingsConstants.ControllerTypeResource), peripheral));
            group.Children.Add(AtariAccessibilityFunctions.LabeledRow(
                LocExtension.Get(AtariInputSettingsConstants.ControllerDeviceResource), device));
            group.Children.Add(AtariAccessibilityFunctions.LabeledRow(
                LocExtension.Get(AtariInputSettingsConstants.DeadZoneResource), deadZone));
            group.Children.Add(EmulationSettingsLayout.InputBindings(bindings,
                LocExtension.Get("Emulation.ControllerMappings")));
            var card = EmulationSettingsLayout.IconCard(group,
                LocExtension.Get("Emulation.ControllerPort", port.Port + AtariInputSettingsConstants.InclusiveEndpointCount),
                ControlVisualConstants.GameControllerGlyph);
            card.Margin = new Thickness(port.Port % 2 == 0 ? 0 : 5, port.Port < 2 ? 0 : 10,
                port.Port % 2 == 0 ? 5 : 0, 0);
            var row = port.Port / 2;
            while (portCards.RowDefinitions.Count <= row)
                portCards.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(card, row);
            Grid.SetColumn(card, port.Port % 2);
            portCards.Children.Add(card);
            _portEditors.Add(new ControllerEditor(port.Port, peripheral, device, deadZone, bindings));
        }
        _controllers.Children.Add(portCards);
    }

    private sealed record ControllerEditor(int Port, ComboBox Peripheral, ComboBox Device, ComboBox DeadZone,
        InputBindingEditor Bindings);
}
