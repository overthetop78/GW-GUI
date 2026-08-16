using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

public static class AtariShortcutExecutionFunctions
{
    public static async ValueTask<bool> ExecuteAsync(string action, IAtariMachine machine,
        AtariShortcutExecutionContext context, CancellationToken cancellationToken = default)
    {
        var rules = AtariShortcutFunctions.Rules(machine.Configuration, machine.SupportsSaveStates,
            File.Exists(context.QuickStatePath));
        if (!AtariShortcutFunctions.IsAvailable(rules, action)) return false;

        switch (action)
        {
            case EmulationShortcutActions.Power:
                await context.TogglePowerAsync(cancellationToken).ConfigureAwait(false);
                break;
            case EmulationShortcutActions.PauseResume:
                await TogglePauseAsync(machine, cancellationToken).ConfigureAwait(false);
                break;
            case EmulationShortcutActions.SoftReset:
                await machine.SoftResetAsync(cancellationToken).ConfigureAwait(false);
                break;
            case EmulationShortcutActions.HardReset:
                await machine.HardResetAsync(cancellationToken).ConfigureAwait(false);
                break;
            case EmulationShortcutActions.ToggleFullscreen:
                await context.ToggleFullscreenAsync(cancellationToken).ConfigureAwait(false);
                break;
            case EmulationShortcutActions.ReleaseMouse:
                await context.ReleaseMouseAsync(cancellationToken).ConfigureAwait(false);
                break;
            case EmulationShortcutActions.QuickSave:
                await machine.SaveStateAsync(context.QuickStatePath, cancellationToken).ConfigureAwait(false);
                break;
            case EmulationShortcutActions.QuickLoad:
                await machine.LoadStateAsync(context.QuickStatePath, cancellationToken).ConfigureAwait(false);
                break;
            case EmulationShortcutActions.Screenshot:
                await context.CaptureScreenshotAsync(cancellationToken).ConfigureAwait(false);
                break;
            case EmulationShortcutActions.ToggleMute:
                machine.SetAudioMuted(!machine.IsAudioMuted);
                break;
            case EmulationShortcutActions.FastForward:
                await context.ToggleFastForwardAsync(cancellationToken).ConfigureAwait(false);
                break;
            case EmulationShortcutActions.InsertMedia:
                await context.InsertMediaAsync(cancellationToken).ConfigureAwait(false);
                break;
            case EmulationShortcutActions.EjectMedia:
                await context.EjectMediaAsync(cancellationToken).ConfigureAwait(false);
                break;
            case EmulationShortcutActions.NextMedia:
                await context.SelectNextMediaAsync(cancellationToken).ConfigureAwait(false);
                break;
            default:
                return false;
        }
        return true;
    }

    private static ValueTask TogglePauseAsync(IAtariMachine machine, CancellationToken cancellationToken) =>
        machine.State == EmulationMachineState.Paused
            ? machine.ResumeAsync(cancellationToken)
            : machine.PauseAsync(cancellationToken);
}
