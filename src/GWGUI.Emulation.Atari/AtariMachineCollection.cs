using System.Collections.Concurrent;

namespace GWGUI.Emulation.Atari;

public sealed class AtariMachineCollection : IAsyncDisposable
{
    private readonly ConcurrentDictionary<Guid, IAtariMachine> _machines = new();

    public IReadOnlyCollection<IAtariMachine> Machines => _machines.Values.ToArray();

    public void Register(IAtariMachine machine)
    {
        ArgumentNullException.ThrowIfNull(machine);
        if (!_machines.TryAdd(machine.Id, machine))
            throw new InvalidOperationException(AtariMachineCollectionConstants.DuplicateMachineError);
    }

    public async ValueTask CloseAsync(Guid id)
    {
        if (!_machines.TryRemove(id, out var machine)) return;
        await AtariMachineCollectionFunctions.StopAndDisposeAsync(machine).ConfigureAwait(false);
    }

    public async ValueTask StopAllAsync()
    {
        var machines = _machines.ToArray();
        _machines.Clear();
        await Task.WhenAll(machines.Select(pair =>
            AtariMachineCollectionFunctions.StopAndDisposeAsync(pair.Value).AsTask())).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync() => StopAllAsync();
}
