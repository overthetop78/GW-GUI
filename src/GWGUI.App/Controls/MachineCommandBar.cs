using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.App.Constants;
using GWGUI.App.Contracts;
using GWGUI.App.Localization;
using GWGUI.App.Input;
using GWGUI.Domain.Settings;

namespace GWGUI.App.Controls;

internal sealed class MachineCommandBar
{
    private readonly List<Button> _requiresPower = [];
    private readonly Button _power;
    private readonly Button _pause;
    private readonly Button _audio;
    private readonly Button _quickSave;
    private readonly Button _quickLoad;
    private readonly Button? _controllerPointer;
    private bool _powered;
    private bool _supportsSavedStates;
    private bool _quickStateAvailable;

    internal MachineCommandBar(DockPanel host, MachineCommandActions actions,
        IReadOnlyList<GlobalShortcutBinding> shortcuts, Action<Exception> showError)
    {
        _power = Command(MachineCommandGlyphConstants.Power, EmulationResourceKeys.Power,
            actions.TogglePower, showError, false);
        _power.Foreground = Brushes.LimeGreen;
        _pause = Command(MachineCommandGlyphConstants.Pause, EmulationResourceKeys.PauseResume,
            actions.TogglePause, showError);
        _pause.Foreground = Brushes.DarkOrange;

        var softReset = Command(MachineCommandGlyphConstants.SoftReset,
            EmulationResourceKeys.SoftReset, actions.SoftReset, showError);
        softReset.Foreground = new SolidColorBrush(Color.FromRgb(120, 160, 48));
        var hardReset = Command(MachineCommandGlyphConstants.HardReset,
            EmulationResourceKeys.HardReset, actions.HardReset, showError);
        hardReset.Foreground = new SolidColorBrush(Color.FromRgb(220, 92, 48));

        var left = new StackPanel { Orientation = Orientation.Horizontal };
        left.Children.Add(MachineView.CreateToolbarGroup(_power, _pause, softReset, hardReset));
        _quickSave = Command(MachineCommandGlyphConstants.QuickSave, EmulationResourceKeys.QuickSave,
            actions.QuickSave, showError);
        _quickLoad = Command(MachineCommandGlyphConstants.QuickLoad, EmulationResourceKeys.QuickLoad,
            actions.QuickLoad, showError);
        left.Children.Add(MachineView.CreateToolbarGroup(_quickSave, _quickLoad));
        left.Children.Add(MachineView.CreateToolbarGroup(
            Command(MachineCommandGlyphConstants.CaptureScreen, EmulationResourceKeys.CaptureScreen,
                actions.CaptureScreen, showError),
            Command(MachineCommandGlyphConstants.Fullscreen, EmulationResourceKeys.Fullscreen,
                actions.ToggleFullscreen, showError, false)));
        var stateShortcuts = EmulationShortcutViewFunctions.CreateGroup(shortcuts,
            (EmulationShortcutDefaults.QuickSave, EmulationResourceKeys.QuickSave),
            (EmulationShortcutDefaults.QuickLoad, EmulationResourceKeys.QuickLoad));
        left.Children.Add(stateShortcuts);
        DockPanel.SetDock(left, Dock.Left);
        host.Children.Add(left);

        var right = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };
        var displayShortcuts = EmulationShortcutViewFunctions.CreateGroup(shortcuts,
            (EmulationShortcutDefaults.ToggleFullscreen, EmulationResourceKeys.Fullscreen),
            (EmulationShortcutDefaults.ReleaseMouse, EmulationResourceKeys.ReleaseMouse));
        right.Children.Add(displayShortcuts);
        _audio = Command(MachineCommandGlyphConstants.Audio, EmulationResourceKeys.Audio,
            actions.ToggleAudio, showError);
        var statusItems = new List<UIElement> { _audio };
        if (actions.SwitchControllerPointer is not null)
        {
            _controllerPointer = Command(MachineCommandGlyphConstants.Pointer,
                EmulationResourceKeys.SwitchControllerPointer, actions.SwitchControllerPointer, showError);
            _controllerPointer.Foreground = Brushes.LimeGreen;
            statusItems.Add(_controllerPointer);
        }
        ControllerStatus = StatusIcon(MachineCommandGlyphConstants.Controller);
        PointerStatus = StatusIcon(MachineCommandGlyphConstants.Pointer);
        statusItems.Add(ControllerStatus);
        statusItems.Add(PointerStatus);
        right.Children.Add(MachineView.CreateToolbarGroup(statusItems.ToArray()));
        Status = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
        right.Children.Add(MachineView.CreateToolbarGroup(Status));
        DockPanel.SetDock(right, Dock.Right);
        host.Children.Add(right);

        RendererStatus = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.SemiBold
        };
        var rendererGroup = MachineView.CreateToolbarGroup(new TextBlock
        {
            Text = MachineCommandGlyphConstants.Renderer,
            FontFamily = ControlVisualConstants.IconFont,
            FontSize = 15,
            Margin = new Thickness(4, 0, 6, 0),
            VerticalAlignment = VerticalAlignment.Center
        }, RendererStatus);
        rendererGroup.Padding = new Thickness(16, 1, 16, 1);
        rendererGroup.Margin = new Thickness(8, 1, 8, 1);
        host.Children.Add(rendererGroup);
        host.SizeChanged += (_, args) =>
        {
            var visibility = args.NewSize.Width >= MachinePresentationConstants.WideToolbarMinimumWidth
                ? Visibility.Visible : Visibility.Collapsed;
            stateShortcuts.Visibility = visibility;
            displayShortcuts.Visibility = visibility;
        };
    }

    internal TextBlock Status { get; }
    internal TextBlock RendererStatus { get; }
    internal TextBlock ControllerStatus { get; }
    internal TextBlock PointerStatus { get; }

    internal void SetPowered(bool powered)
    {
        _powered = powered;
        foreach (var button in _requiresPower) button.IsEnabled = powered;
        _power.Foreground = powered ? Brushes.LimeGreen : Brushes.Gray;
        UpdateSavedStateButtons();
    }

    internal void SetSavedStateAvailability(bool supported, bool quickStateAvailable)
    {
        _supportsSavedStates = supported;
        _quickStateAvailable = quickStateAvailable;
        UpdateSavedStateButtons();
    }

    internal void SetPaused(bool paused)
    {
        SetGlyph(_pause, paused ? MachineCommandGlyphConstants.Continue : MachineCommandGlyphConstants.Pause);
        _pause.Foreground = paused ? Brushes.LimeGreen : Brushes.DarkOrange;
    }

    internal void SetMuted(bool muted)
    {
        SetGlyph(_audio, muted ? MachineCommandGlyphConstants.Muted : MachineCommandGlyphConstants.Audio);
        _audio.Foreground = muted ? Brushes.Gray : Brushes.Black;
    }

    internal void SetControllerPointerMode(bool pointerMode)
    {
        if (_controllerPointer is null) return;
        ((TextBlock)_controllerPointer.Content).Text = pointerMode
            ? MachineCommandGlyphConstants.Pointer
            : MachineCommandGlyphConstants.Controller;
        _controllerPointer.Foreground = Brushes.LimeGreen;
    }

    internal void SetInputStatus(bool pointerCaptured, bool controllerAvailable)
    {
        PointerStatus.Foreground = pointerCaptured ? Brushes.LimeGreen : Brushes.LightGray;
        ControllerStatus.Foreground = controllerAvailable ? Brushes.LimeGreen : Brushes.LightGray;
        AutomationProperties.SetItemStatus(PointerStatus, LocExtension.Get(pointerCaptured
            ? "Emulation.Value.Enabled" : "Emulation.Value.Disabled"));
        AutomationProperties.SetItemStatus(ControllerStatus, LocExtension.Get(controllerAvailable
            ? "Emulation.Value.Enabled" : "Emulation.Value.Disabled"));
    }

    private void UpdateSavedStateButtons()
    {
        _quickSave.IsEnabled = _powered && _supportsSavedStates;
        _quickLoad.IsEnabled = _powered && _supportsSavedStates && _quickStateAvailable;
    }

    private static void SetGlyph(Button button, string glyph)
    {
        if (button.Content is TextBlock text) text.Text = glyph;
    }

    private Button Command(string glyph, string tooltipResource, Func<Task> action,
        Action<Exception> showError, bool requiresPower = true)
    {
        var button = MachineView.CreateCommandButton(glyph, LocExtension.Get(tooltipResource));
        button.Click += async (_, _) => await RunAsync(action, showError);
        if (requiresPower) _requiresPower.Add(button);
        return button;
    }

    private static TextBlock StatusIcon(string glyph) => new()
    {
        Text = glyph,
        FontFamily = ControlVisualConstants.IconFont,
        FontSize = 16,
        Foreground = Brushes.LightGray,
        Margin = new Thickness(5, 0, 5, 0),
        VerticalAlignment = VerticalAlignment.Center
    };

    private static async Task RunAsync(Func<Task> action, Action<Exception> showError)
    {
        try { await action(); }
        catch (Exception error) { showError(error); }
    }
}
