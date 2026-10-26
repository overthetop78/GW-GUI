namespace GWGUI.Emulation.Acorn.Common.Services;

/// <summary>Projette la facette cassette du cœur vers l'API de transport existante de l'application.</summary>
internal sealed partial class Machine : IEmulationCassetteTransport
{
    /// <summary>Les autres cœurs restent dépourvus de transport lorsqu'ils ne fournissent pas cette facette.</summary>
    public IEmulationCassetteTransport? CassetteTransport => _core is IEmulatorCassetteTransport ? this : null;
    EmulationCassetteState IEmulationCassetteTransport.State => (_core as IEmulatorCassetteTransport)?.CassetteState ?? EmulationCassetteState.Empty;
    EmulationCassetteCommand? IEmulationCassetteTransport.ActiveOperation => (_core as IEmulatorCassetteTransport)?.CassetteOperation;
    IReadOnlySet<EmulationCassetteCommand> IEmulationCassetteTransport.AvailableCommands => (_core as IEmulatorCassetteTransport)?.CassetteCommands ?? new HashSet<EmulationCassetteCommand>();
    /// <summary>Le lecteur est modifié sur le thread propriétaire du cœur, comme les montages et les resets.</summary>
    ValueTask IEmulationCassetteTransport.ExecuteAsync(EmulationCassetteCommand command, CancellationToken cancellationToken) => QueueCommand(() => ((IEmulatorCassetteTransport)_core).ExecuteCassette(command), cancellationToken);
}
