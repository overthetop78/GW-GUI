using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation.Errors;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Contracts.Machine;
using GWGUI.App.Functions.Emulation.Shortcuts;
using GWGUI.App.Presenters.Common;
using GWGUI.App.Services.Audio;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation;


namespace GWGUI.App.Views.Controls.Emulation.Machine;

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
        MachineController? view = null;
        view = new MachineController(new MachineControllerOptions(
            runtime.CreateMachine(runtime.MountedMedia), runtime.CreateMachine,
            runtime.MediaDevices, runtime.MountedMedia, runtime.Configuration.VideoRenderer,
            runtime.Configuration.VideoProcessing,
            EmulationShortcutMap.GlobalShortcuts(_settings.EmulationShortcuts),
            Path.Combine(_settings.EmulationStateFolder,
                $"{selected.Module.Id}-{selected.Configuration.Id:N}.gwas"),
            _settings.EmulationCaptureFolder, RuntimeDisplayName(runtime),
            error => ControlErrorPresenter.ShowEmulation(this, error,
                ControlErrorContexts.EmulationConfigurationOpening, selected.DisplayName),
            runtime.SupportsPointerCapture,
            device => InitialMediaDirectory(selected, device),
            (device, directory) => RememberMediaDirectory(selected, device, directory),
            () => ReferenceEquals(_machines.SelectedContent, view),
            runtime.PrepareMediaAsync));
        await AddMachineAsync(selected, runtime, view, view.StopAsync);
    }

    private Task AddMachineAsync(EmulationConfigurationListItem selected, EmulationMachineRuntime runtime,
        FrameworkElement view, Func<Task> stop)
    {
        var key = (selected.Module.Id, selected.Configuration.Id);
        var tab = new TabItem { Content = view, Tag = runtime, Padding = new Thickness(18, 9, 14, 9) };
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
