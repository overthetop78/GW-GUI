using System.IO;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Contracts;
using GWGUI.App.Input;
using GWGUI.App.Services;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

public sealed partial class EmulationSection
{
    private async Task OpenMachineAsync(EmulationConfigurationListItem selected)
    {
        var moduleRoot = Path.Combine(_settings.EmulationStorageFolder, selected.Module.Id);
        var runtime = await selected.Module.CreateRuntimeAsync(selected.Configuration,
            new EmulationRuntimeServices(
                Path.Combine(moduleRoot, "Sessions"),
                Path.Combine(moduleRoot, "States"),
                Path.Combine(moduleRoot, "Converted"),
                Environment.ProcessPath!,
                (device, latency) => new WasapiAudioOutput(device, latency)));
        var view = new MachineController(new MachineControllerOptions(
            runtime.CreateMachine(runtime.MountedMedia), runtime.CreateMachine,
            runtime.MediaDevices, runtime.MountedMedia, runtime.Configuration.VideoRenderer,
            EmulationShortcutMap.GlobalShortcuts(_settings.EmulationShortcuts),
            Path.Combine(_settings.EmulationStateFolder,
                $"{selected.Module.Id}-{selected.Configuration.Id:N}.gwas"),
            _settings.EmulationCaptureFolder, RuntimeDisplayName(runtime),
            error => ControlErrorPresenter.ShowEmulation(this, error,
                ControlErrorContexts.EmulationConfigurationOpening, selected.DisplayName),
            runtime.SupportsPointerCapture,
            device => InitialMediaDirectory(selected, device),
            (device, directory) => RememberMediaDirectory(selected, device, directory),
            runtime.PrepareMediaAsync));
        await AddMachineAsync(selected, view, view.StopAsync);
    }

    private Task AddMachineAsync(
        EmulationConfigurationListItem selected, FrameworkElement view, Func<Task> stop)
    {
        var key = (selected.Module.Id, selected.Configuration.Id);
        var tab = new TabItem { Content = view, Padding = new Thickness(18, 9, 14, 9) };
        tab.SetResourceReference(StyleProperty, ControlVisualConstants.MainTabItemStyleResource);
        tab.Header = CreateMachineTabHeader(MachineTitle(selected), selected.DisplayName,
            () => CloseMachineAsync(key, tab, stop));
        _openMachines.Add(key, tab);
        _machines.Items.Add(tab);
        _machines.SelectedItem = tab;
        return Task.CompletedTask;
    }

    private async Task CloseMachineAsync(
        (string ModuleId, Guid Id) key, TabItem tab, Func<Task> stop)
    {
        if (!_openMachines.ContainsKey(key)) return;
        await stop();
        _openMachines.Remove(key);
        _machines.Items.Remove(tab);
    }

    public async Task StopAllAsync()
    {
        foreach (var tab in _openMachines.Values.ToArray())
        {
            if (tab.Content is MachineController machine) await machine.StopAsync();
        }
        _openMachines.Clear();
    }
}
