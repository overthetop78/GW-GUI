using GWGUI.App.Functions.Emulation.Machine;
using GWGUI.App.Contracts.Machine;
using GWGUI.App.Views.Controls.Emulation.Machine;
using GWGUI.Emulation;
using GWGUI.Emulation.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GWGUI.Tests.Interface.EmulationViews;

internal static class CassetteTransportPresentationScenarios
{
    internal static void CommandsReflectCapabilitiesAndTransportState()
    {
        Assert.Equal(6, Enum.GetValues<EmulationCassetteCommand>().Length);

        Assert.False(Enabled(powered: true, supported: false,
            EmulationCassetteState.Stopped, null, EmulationCassetteCommand.Play));
        Assert.False(Enabled(powered: true, supported: true,
            EmulationCassetteState.Empty, null, EmulationCassetteCommand.Play));
        Assert.False(Enabled(powered: false, supported: true,
            EmulationCassetteState.Stopped, null, EmulationCassetteCommand.Play));
        Assert.True(Enabled(powered: true, supported: true,
            EmulationCassetteState.Stopped, null, EmulationCassetteCommand.Play));

        Assert.True(CassetteTransportPresentationFunctions.IsActive(
            EmulationCassetteState.Playing, EmulationCassetteCommand.Play,
            EmulationCassetteCommand.Play));
        Assert.False(Enabled(powered: true, supported: true,
            EmulationCassetteState.Playing, EmulationCassetteCommand.Play,
            EmulationCassetteCommand.Play));

        Assert.True(CassetteTransportPresentationFunctions.IsActive(
            EmulationCassetteState.Recording, EmulationCassetteCommand.Record,
            EmulationCassetteCommand.Record));
        Assert.False(Enabled(powered: true, supported: true,
            EmulationCassetteState.Recording, EmulationCassetteCommand.Record,
            EmulationCassetteCommand.Record));

        Assert.True(CassetteTransportPresentationFunctions.IsBlinking(
            EmulationCassetteState.Paused, EmulationCassetteCommand.Play,
            EmulationCassetteCommand.Play));
        Assert.True(CassetteTransportPresentationFunctions.IsBlinking(
            EmulationCassetteState.Paused, EmulationCassetteCommand.Record,
            EmulationCassetteCommand.Record));
        Assert.False(CassetteTransportPresentationFunctions.IsBlinking(
            EmulationCassetteState.Paused, EmulationCassetteCommand.Play,
            EmulationCassetteCommand.Record));
    }

    internal static void PanelContainsASeparateRowWithEveryCommand()
    {
        Assert.Equal(
        [
            EmulationCassetteCommand.Record,
            EmulationCassetteCommand.Play,
            EmulationCassetteCommand.Rewind,
            EmulationCassetteCommand.FastForward,
            EmulationCassetteCommand.Stop,
            EmulationCassetteCommand.Pause
        ], MachineController.CassetteCommandOrder);

        var commands = MachineController.CassetteCommandOrder
            .Select(command => new MachineViewDeviceCommand(command, command.ToString(), command.ToString(),
                command is EmulationCassetteCommand.Play or EmulationCassetteCommand.Record,
                command is EmulationCassetteCommand.Play or EmulationCassetteCommand.Record, false, false,
                () => Task.CompletedTask))
            .ToArray();
        var view = new MachineView();
        view.SetDevices(
            [new MachineViewDevice("Cassette0", "TAPE0:", "T", true, true,
                () => Task.CompletedTask, () => Task.CompletedTask, "Stopped", commands)],
            _ => { }, () => { });

        var deviceBorder = Assert.IsType<Border>(Assert.Single(view.DeviceStrip.Children));
        var deviceRows = Assert.IsType<StackPanel>(deviceBorder.Child);
        Assert.Equal(Orientation.Vertical, deviceRows.Orientation);
        Assert.Equal(2, deviceRows.Children.Count);
        var commandGroup = Assert.IsType<Border>(deviceRows.Children[1]);
        Assert.Equal(new CornerRadius(6), commandGroup.CornerRadius);
        var commandRow = Assert.IsType<StackPanel>(commandGroup.Child);
        Assert.Equal(Orientation.Horizontal, commandRow.Orientation);
        var buttons = commandRow.Children.Cast<Button>().ToArray();
        Assert.Equal(6, buttons.Length);
        Assert.Equal(MachineController.CassetteCommandOrder,
            buttons.Select(button => Enum.Parse<EmulationCassetteCommand>((string)button.ToolTip)).ToArray());
        Assert.True(buttons.Single(button => Equals(button.ToolTip,
            EmulationCassetteCommand.Play.ToString())).IsEnabled);
        Assert.True(buttons.Single(button => Equals(button.ToolTip,
            EmulationCassetteCommand.Record.ToString())).IsEnabled);
        Assert.All(buttons.Where(button => !Equals(button.ToolTip,
            EmulationCassetteCommand.Play.ToString()) && !Equals(button.ToolTip,
            EmulationCassetteCommand.Record.ToString())), button => Assert.False(button.IsEnabled));
        var play = buttons.Single(button => Equals(button.ToolTip,
            EmulationCassetteCommand.Play.ToString()));
        var record = buttons.Single(button => Equals(button.ToolTip,
            EmulationCassetteCommand.Record.ToString()));
        Assert.Equal(Color.FromRgb(22, 130, 59), ((SolidColorBrush)play.Foreground).Color);
        Assert.Equal(Color.FromRgb(180, 35, 24), ((SolidColorBrush)record.Foreground).Color);
        Assert.Equal(play.Foreground, Assert.IsType<TextBlock>(
            Assert.IsType<Border>(play.Content).Child).Foreground);
        Assert.Equal(record.Foreground, Assert.IsType<TextBlock>(
            Assert.IsType<Border>(record.Content).Child).Foreground);
        Assert.Equal(Brushes.Gray, buttons.Single(button => Equals(button.ToolTip,
            EmulationCassetteCommand.Stop.ToString())).Foreground);

        view.SetCassetteTransport("Cassette0", EmulationCassetteState.Playing,
            EmulationCassetteCommand.Play,
            new HashSet<EmulationCassetteCommand> { EmulationCassetteCommand.Play }, powered: true);
        Assert.Equal(Color.FromRgb(0, 200, 83), ((SolidColorBrush)play.Foreground).Color);
        Assert.False(play.IsEnabled);
        Assert.Equal(new Thickness(2), Assert.IsType<Border>(play.Content).BorderThickness);
    }

    internal static void DeviceHeaderKeepsEjectSpaceAndCentersLedWhenEmpty()
    {
        var view = new MachineView();
        view.SetDevices(
            [new MachineViewDevice("Cartridge0", "CART0:", "C", true, false,
                () => Task.CompletedTask, null)], _ => { }, () => { });

        var deviceBorder = Assert.IsType<Border>(Assert.Single(view.DeviceStrip.Children));
        var rows = Assert.IsType<StackPanel>(deviceBorder.Child);
        var header = Assert.IsType<StackPanel>(Assert.Single(rows.Children));
        var ledHost = Assert.IsType<Grid>(header.Children[0]);
        var led = Assert.IsType<System.Windows.Shapes.Ellipse>(Assert.Single(ledHost.Children));
        Assert.Equal(HorizontalAlignment.Center, led.HorizontalAlignment);
        Assert.Equal(VerticalAlignment.Center, led.VerticalAlignment);
        Assert.Equal(18, ledHost.Width);

        var eject = Assert.IsType<Button>(header.Children[header.Children.Count - 1]);
        Assert.Equal(22, eject.Width);
        Assert.Equal(Visibility.Hidden, eject.Visibility);
        Assert.False(eject.IsEnabled);
    }

    private static bool Enabled(bool powered, bool supported, EmulationCassetteState state,
        EmulationCassetteCommand? activeOperation, EmulationCassetteCommand command) =>
        CassetteTransportPresentationFunctions.IsEnabled(
            powered, supported, state, activeOperation, command);
}
