using GWGUI.Emulation;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal sealed record AtariMachineMediaView(
    AtariMediaConfiguration Configuration,
    string Label,
    string Glyph,
    bool Removable);

internal sealed record AtariMachineStatusView(
    string Text,
    double AspectRatio,
    IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity,
    bool AudioActive,
    bool MouseAvailable,
    bool ControllerAvailable);

internal sealed record AtariMachineShortcutActions(
    Func<Task> TogglePower,
    Func<Task> TogglePause,
    Func<Task> SoftReset,
    Func<Task> HardReset,
    Func<Task> QuickSave,
    Func<Task> QuickLoad,
    Func<Task> Screenshot,
    Func<Task> ToggleFullscreen,
    Action ReleaseMouse,
    Func<Task> ToggleMute);
