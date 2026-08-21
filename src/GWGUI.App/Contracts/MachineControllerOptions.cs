using GWGUI.Domain.Settings;
using GWGUI.Emulation;
using GWGUI.App.Input;

namespace GWGUI.App.Contracts;

internal sealed record MachineControllerOptions(
    IEmulatedMachine Machine,
    Func<IReadOnlyList<EmulationMedia>, IEmulatedMachine> MachineFactory,
    IReadOnlyList<EmulationMediaDevice> MediaDevices,
    IReadOnlyList<EmulationMedia> MountedMedia,
    EmulationVideoRenderer VideoRenderer,
    IReadOnlyList<GlobalShortcutBinding> GlobalShortcuts,
    string QuickStatePath,
    string CaptureFolder,
    string WindowTitle,
    Action<Exception> ShowError,
    bool SupportsPointerCapture,
    Func<EmulationMediaDevice, string?> InitialMediaDirectory,
    Action<EmulationMediaDevice, string> RememberMediaDirectory,
    Func<EmulationMedia, CancellationToken, ValueTask<EmulationMedia>>? PrepareMediaAsync = null,
    Func<Task>? SwitchControllerPointer = null);
