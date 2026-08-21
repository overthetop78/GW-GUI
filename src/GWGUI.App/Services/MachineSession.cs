using GWGUI.Emulation;

namespace GWGUI.App.Services;

internal sealed class MachineSession : IAsyncDisposable
{
    private readonly Func<IReadOnlyList<EmulationMedia>, IEmulatedMachine> _machineFactory;
    private readonly List<EmulationMedia> _mountedMedia;
    private IEmulatedMachine _machine;
    private bool _disposed;
    private bool _hasLiveMachine = true;

    internal MachineSession(IEmulatedMachine machine,
        Func<IReadOnlyList<EmulationMedia>, IEmulatedMachine> machineFactory,
        IEnumerable<EmulationMedia> mountedMedia)
    {
        _machine = machine;
        _machineFactory = machineFactory;
        _mountedMedia = mountedMedia.ToList();
    }

    internal IEmulatedMachine Machine => _machine;
    internal bool IsPowered { get; private set; }
    internal IReadOnlyList<EmulationMedia> MountedMedia => _mountedMedia;
    internal event EventHandler<IEmulatedMachine>? MachineChanged;

    internal async Task PowerOnAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (IsPowered) return;
        if (!_hasLiveMachine) CreateMachine();
        try
        {
            await _machine.Lifecycle.StartAsync();
            IsPowered = true;
        }
        catch
        {
            await DisposeCurrentMachineAsync();
            throw;
        }
    }

    internal async Task PowerOffAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!IsPowered) return;
        await DisposeCurrentMachineAsync();
        IsPowered = false;
    }

    internal async Task TogglePowerAsync()
    {
        if (IsPowered) await PowerOffAsync();
        else await PowerOnAsync();
    }

    internal async Task TogglePauseAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!IsPowered) return;
        if (_machine.State == EmulationMachineState.Running)
            await _machine.Lifecycle.PauseAsync();
        else if (_machine.State == EmulationMachineState.Paused)
            await _machine.Lifecycle.ResumeAsync();
    }

    internal async Task RecreateRunningMachineAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var wasPowered = IsPowered;
        await DisposeCurrentMachineAsync();
        CreateMachine();
        if (!wasPowered) return;
        try
        {
            await _machine.Lifecycle.StartAsync();
            IsPowered = true;
        }
        catch
        {
            await DisposeCurrentMachineAsync();
            IsPowered = false;
            throw;
        }
    }

    internal async Task InsertAsync(EmulationMedia media, bool requiresMachineRecreation)
    {
        var next = _mountedMedia.Where(item => item.Slot != media.Slot).Append(media).ToArray();
        if (IsPowered && requiresMachineRecreation) await RecreateRunningMachineAsync(next);
        else if (IsPowered) await _machine.Media.InsertAsync(media);
        ReplaceMountedMedia(next);
    }

    internal async Task EjectAsync(EmulationMediaSlot slot, bool requiresMachineRecreation)
    {
        var next = _mountedMedia.Where(item => item.Slot != slot).ToArray();
        if (IsPowered && requiresMachineRecreation) await RecreateRunningMachineAsync(next);
        else if (IsPowered) await _machine.Media.EjectAsync(slot);
        ReplaceMountedMedia(next);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        await DisposeCurrentMachineAsync();
        _disposed = true;
    }

    private void CreateMachine()
    {
        _machine = _machineFactory(_mountedMedia);
        _hasLiveMachine = true;
        MachineChanged?.Invoke(this, _machine);
    }

    private async Task RecreateRunningMachineAsync(IReadOnlyList<EmulationMedia> media)
    {
        await DisposeCurrentMachineAsync();
        _machine = _machineFactory(media);
        _hasLiveMachine = true;
        MachineChanged?.Invoke(this, _machine);
        try
        {
            await _machine.Lifecycle.StartAsync();
            IsPowered = true;
        }
        catch
        {
            await DisposeCurrentMachineAsync();
            IsPowered = false;
            throw;
        }
    }

    private void ReplaceMountedMedia(IEnumerable<EmulationMedia> media)
    {
        _mountedMedia.Clear();
        _mountedMedia.AddRange(media);
    }

    private async Task DisposeCurrentMachineAsync()
    {
        if (!_hasLiveMachine) return;
        try { await _machine.Lifecycle.StopAsync(); }
        finally
        {
            await _machine.DisposeAsync();
            _hasLiveMachine = false;
        }
    }
}
