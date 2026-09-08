using GWGUI.Emulation;

namespace GWGUI.App.Functions.Emulation.Machine;

internal static class CassetteTransportPresentationFunctions
{
    internal static bool IsEnabled(bool powered, bool supported,
        EmulationCassetteState state, EmulationCassetteCommand? activeOperation,
        EmulationCassetteCommand command) =>
        powered && supported && state != EmulationCassetteState.Empty
        && !IsRunning(state, activeOperation, command);

    internal static bool IsActive(EmulationCassetteState state,
        EmulationCassetteCommand? activeOperation, EmulationCassetteCommand command) => command switch
    {
        EmulationCassetteCommand.Play => activeOperation == command
            && state is EmulationCassetteState.Playing or EmulationCassetteState.Paused,
        EmulationCassetteCommand.Record => activeOperation == command
            && state is EmulationCassetteState.Recording or EmulationCassetteState.Paused,
        EmulationCassetteCommand.Pause => state == EmulationCassetteState.Paused,
        _ => false
    };

    internal static bool IsBlinking(EmulationCassetteState state,
        EmulationCassetteCommand? activeOperation, EmulationCassetteCommand command) =>
        state == EmulationCassetteState.Paused
        && activeOperation is EmulationCassetteCommand.Play or EmulationCassetteCommand.Record
        && command == activeOperation;

    private static bool IsRunning(EmulationCassetteState state,
        EmulationCassetteCommand? activeOperation, EmulationCassetteCommand command) =>
        IsActive(state, activeOperation, command)
        && command is EmulationCassetteCommand.Play or EmulationCassetteCommand.Record;
}
