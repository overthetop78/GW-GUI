using GWGUI.App.Contracts.Input;
using GWGUI.Emulation;

namespace GWGUI.App.Contracts.Machine;

internal sealed record MachineControllerOptions(
    string ModuleId,
    Guid ConfigurationId,
    IEmulatedMachine Machine,
    Func<IReadOnlyList<EmulationMedia>, IEmulatedMachine> MachineFactory,
    IReadOnlyList<EmulationMediaDevice> MediaDevices,
    IReadOnlyList<EmulationMedia> MountedMedia,
    EmulationVideoRenderer VideoRenderer,
    EmulationVideoProcessingConfiguration VideoProcessing,
    IReadOnlyList<GlobalShortcutBinding> GlobalShortcuts,
    string QuickStatePath,
    string CaptureFolder,
    string WindowTitle,
    Action<Exception> ShowError,
    bool SupportsPointerCapture,
    Func<EmulationMediaDevice, string?> InitialMediaDirectory,
    Action<EmulationMediaDevice, string> RememberMediaDirectory,
    Func<bool> IsActive,
    Func<EmulationMedia, CancellationToken, ValueTask<EmulationMedia>>? PrepareMediaAsync = null,
    Func<Task>? SwitchControllerPointer = null);
