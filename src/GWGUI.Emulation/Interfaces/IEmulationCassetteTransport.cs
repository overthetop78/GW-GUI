namespace GWGUI.Emulation.Interfaces;

public interface IEmulationCassetteTransport
{
    EmulationCassetteState State { get; }
    EmulationCassetteCommand? ActiveOperation => State switch
    {
        EmulationCassetteState.Playing => EmulationCassetteCommand.Play,
        EmulationCassetteState.Recording => EmulationCassetteCommand.Record,
        _ => null
    };
    IReadOnlySet<EmulationCassetteCommand> AvailableCommands { get; }
    ValueTask ExecuteAsync(EmulationCassetteCommand command,
        CancellationToken cancellationToken = default);
}
