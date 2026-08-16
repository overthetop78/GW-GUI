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
    private readonly StackPanel _mouse = new();
    private readonly InputBindingEditor _mouseBindings = new();
    private readonly StackPanel _controllers = new();
    private readonly CheckBox _captureMouse = new();
    private readonly ComboBox _releaseMouse = new();
    private readonly ComboBox _mouseSpeed = new();
    private readonly List<ControllerEditor> _portEditors = [];
    internal UIElement Keyboard => _keyboard;
    internal UIElement Mouse => _mouse;
    internal UIElement Controllers => _controllers;

    internal AtariInputSettingsSection()
    {
        _keyboard.ConfigurePresentation(LocExtension.Get(AtariInputSettingsConstants.AtariKeyResource),
            LocExtension.Get(AtariInputSettingsConstants.SearchKeyResource));
        _releaseMouse.ItemsSource = Enum.GetValues<EmulationKey>().Where(value => value != EmulationKey.Unknown);
        _mouseSpeed.ItemsSource = AtariInputSettingsFunctions.MouseSpeeds();
        _mouseBindings.ConfigureCaptureSources(InputCaptureSources.Mouse | InputCaptureSources.Controller);
        BuildMouse();
    }

    internal void Load(AtariMachineConfiguration configuration)
    {
        var view = AtariInputSettingsFunctions.Create(configuration);
        _keyboard.Visibility = view.HasKeyboard ? Visibility.Visible : Visibility.Collapsed;
        _keyboard.SetRows(view.KeyboardDefinitions, view.KeyboardBindings);
        _mouse.Visibility = view.HasMouse ? Visibility.Visible : Visibility.Collapsed;
        _captureMouse.IsChecked = configuration.Input.CaptureMouse;
        _releaseMouse.SelectedItem = configuration.Input.ReleaseMouseKey;
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
            _captureMouse.IsChecked == true,
            _releaseMouse.SelectedItem is EmulationKey release ? release : EmulationKey.Escape,
            _mouseSpeed.SelectedItem is int speed ? speed : AtariInputSettingsConstants.DefaultMouseSpeedPercent);
    }

    private void BuildMouse()
    {
        _captureMouse.Content = LocExtension.Get(AtariInputSettingsConstants.CaptureMouseResource);
        _mouse.Children.Add(_captureMouse);
        _mouse.Children.Add(Row(AtariInputSettingsConstants.ReleaseMouseResource, _releaseMouse));
        _mouse.Children.Add(Row(AtariInputSettingsConstants.MouseSpeedResource, _mouseSpeed));
        _mouse.Children.Add(_mouseBindings);
    }

    private void BuildControllers(IReadOnlyList<AtariControllerPortView> ports)
    {
        _controllers.Children.Clear();
        _portEditors.Clear();
        var detection = new TextBlock();
        var detect = new Button { Content = LocExtension.Get(AtariInputSettingsConstants.DetectControllersResource) };
        detect.Click += (_, _) =>
        {
            var count = XInputControllerReader.ReadAll().Count;
            detection.Text = count == AtariInputSettingsConstants.NoControllerCount
                ? LocExtension.Get(AtariInputSettingsConstants.NoControllerResource)
                : count.ToString(CultureInfo.CurrentCulture);
            var devices = AtariInputSettingsFunctions.ControllerDeviceIds(count);
            foreach (var editor in _portEditors) editor.Device.ItemsSource = devices;
        };
        _controllers.Children.Add(detect);
        _controllers.Children.Add(detection);
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
            group.Children.Add(Row(AtariInputSettingsConstants.ControllerTypeResource, peripheral));
            group.Children.Add(Row(AtariInputSettingsConstants.ControllerDeviceResource, device));
            group.Children.Add(Row(AtariInputSettingsConstants.DeadZoneResource, deadZone));
            group.Children.Add(bindings);
            _controllers.Children.Add(new GroupBox { Header = port.Port.ToString(CultureInfo.CurrentCulture), Content = group });
            _portEditors.Add(new ControllerEditor(port.Port, peripheral, device, deadZone, bindings));
        }
    }

    private static UIElement Row(string resource, UIElement editor)
    {
        var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.Children.Add(new TextBlock { Text = LocExtension.Get(resource), VerticalAlignment = VerticalAlignment.Center });
        Grid.SetColumn(editor, 1);
        row.Children.Add(editor);
        return row;
    }

    private sealed record ControllerEditor(int Port, ComboBox Peripheral, ComboBox Device, ComboBox DeadZone,
        InputBindingEditor Bindings);
}
