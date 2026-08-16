using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal sealed class AtariMachineSessionView : UserControl
{
    private readonly IAtariMachine _machine;
    private readonly TextBlock _status = new()
    {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        TextWrapping = TextWrapping.Wrap,
        TextAlignment = TextAlignment.Center
    };

    internal AtariMachineSessionView(IAtariMachine machine)
    {
        _machine = machine;
        _status.Text = LocExtension.Get(AtariEmulationConstants.StartingResource);
        Content = _status;
    }

    internal async Task StartAsync()
    {
        await _machine.StartAsync();
        _status.Text = LocExtension.Get(AtariEmulationConstants.RunningResource);
    }

    internal async Task StopAsync()
    {
        await _machine.StopAsync();
        _status.Text = LocExtension.Get(AtariEmulationConstants.StoppedResource);
    }
}
