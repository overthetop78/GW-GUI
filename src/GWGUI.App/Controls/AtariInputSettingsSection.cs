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
    private readonly Border _keyboardUnavailable = EmulationSettingsLayout.InformationBanner(
        LocExtension.Get(AtariInputSettingsConstants.NoKeyboardResource));
    private readonly Border _mouseUnavailable = EmulationSettingsLayout.InformationBanner(
        LocExtension.Get(AtariInputSettingsConstants.NoMouseResource));
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
        var controllers = _portEditors.Select(editor => new AtariControllerBinding(editor.Port,
            editor.Peripheral.SelectedItem is AtariPeripheralKind selected ? selected : AtariPeripheralKind.Automatic,
            (editor.Device.SelectedItem as GameControllerDevice)?.Id ?? editor.Device.Tag as string,
            Mappings: editor.Bindings.Rows.ToDictionary(row => row.Id, row => row.Binding, StringComparer.Ordinal),
            DeadZonePercent: editor.DeadZonePercent)).ToArray();
        return AtariInputSettingsFunctions.Apply(configuration, _keyboard.Rows, _mouseBindings.Rows, controllers,
            configuration.Input.CaptureMouse, configuration.Input.ReleaseMouseKey,
            _mouseSpeed.SelectedItem is int speed ? speed : AtariInputSettingsConstants.DefaultMouseSpeedPercent);
    }

    private UIElement BuildKeyboardPage()
    {
        var page = new StackPanel { Margin = new Thickness(12) };
        _keyboardUnavailable.Visibility = Visibility.Collapsed;
        page.Children.Add(_keyboardUnavailable);
        var bindings = EmulationSettingsLayout.InputBindings(_keyboard,
            LocExtension.Get("Emulation.InputActions"));
        bindings.Margin = new Thickness(0, 10, 0, 0);
        page.Children.Add(bindings);
        return page;
    }

    private UIElement BuildMousePage()
    {
        var root = new StackPanel { Margin = new Thickness(12) };
        _mouseUnavailable.Visibility = Visibility.Collapsed;
        root.Children.Add(_mouseUnavailable);
        var settings = EmulationSettingsLayout.IconCard(EmulationSettingsLayout.CompactForm(1,
            (LocExtension.Get("Emulation.MouseSpeed"), _mouseSpeed)),
            LocExtension.Get("Emulation.MouseTab"), "\uE962");
        settings.Margin = new Thickness(0, 10, 0, 0);
        root.Children.Add(settings);
        var bindings = EmulationSettingsLayout.InputBindings(_mouseBindings,
            LocExtension.Get("Emulation.MouseActions"), LocExtension.Get("Emulation.InputCaptureHint"));
        bindings.Margin = new Thickness(0, 10, 0, 0);
        root.Children.Add(bindings);
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
            var devices = XInputControllerReader.GetConnectedDevices();
            detection.Text = devices.Count == AtariInputSettingsConstants.NoControllerCount
                ? LocExtension.Get(AtariInputSettingsConstants.NoControllerResource)
                : string.Join(AtariInputSettingsConstants.ControllerNameSeparator,
                    devices.Select(device => device.Name));
            foreach (var editor in _portEditors)
            {
                var selectedId = (editor.Device.SelectedItem as GameControllerDevice)?.Id
                    ?? editor.Device.Tag as string;
                editor.Device.ItemsSource = devices;
                editor.Device.SelectedItem = devices.FirstOrDefault(device => device.Id == selectedId)
                    ?? devices.ElementAtOrDefault(editor.Port);
                editor.Device.Tag = null;
            }
        };
        var detectionContent = new StackPanel { Orientation = Orientation.Horizontal };
        detectionContent.Children.Add(detect);
        detectionContent.Children.Add(detection);
        _controllers.Children.Add(EmulationSettingsLayout.IconCard(detectionContent,
            LocExtension.Get("Emulation.DetectedControllers"), ControlVisualConstants.GameControllerGlyph));
        var portCards = new Grid { Margin = new Thickness(0, 10, 0, 0) };
        portCards.ColumnDefinitions.Add(new ColumnDefinition());
        portCards.ColumnDefinitions.Add(new ColumnDefinition());
        var mappingTabs = new TabControl { MinHeight = AtariInputSettingsConstants.MappingEditorMinimumHeight };
        foreach (var port in ports)
        {
            var peripheral = new ComboBox { ItemsSource = port.Peripherals, SelectedItem = port.Selected };
            var device = new ComboBox { DisplayMemberPath = nameof(GameControllerDevice.Name) };
            if (port.DeviceId is not null)
            {
                device.Tag = port.DeviceId;
            }
            var bindings = new InputBindingEditor();
            bindings.ConfigureCaptureSources(InputCaptureSources.Keyboard | InputCaptureSources.Controller, true);
            bindings.SetRows(port.Definitions, port.Bindings);
            var group = new StackPanel();
            group.Children.Add(AtariAccessibilityFunctions.LabeledRow(
                LocExtension.Get(AtariInputSettingsConstants.ControllerTypeResource), peripheral));
            group.Children.Add(AtariAccessibilityFunctions.LabeledRow(
                LocExtension.Get(AtariInputSettingsConstants.ControllerDeviceResource), device));
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
            mappingTabs.Items.Add(new TabItem
            {
                Header = LocExtension.Get("Emulation.ControllerPort",
                    port.Port + AtariInputSettingsConstants.InclusiveEndpointCount),
                Content = bindings
            });
            _portEditors.Add(new ControllerEditor(port.Port, peripheral, device, port.DeadZonePercent, bindings));
        }
        _controllers.Children.Add(portCards);
        var mappings = EmulationSettingsLayout.ActionCard(mappingTabs,
            LocExtension.Get("Emulation.ControllerMappings"));
        mappings.Margin = new Thickness(0, 10, 0, 0);
        _controllers.Children.Add(mappings);
    }

    private sealed record ControllerEditor(int Port, ComboBox Peripheral, ComboBox Device, int DeadZonePercent,
        InputBindingEditor Bindings);
}
