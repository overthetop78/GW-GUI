using GWGUI.App.Services.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Emulation.EmulationContracts;
internal static class SessionLifecycleScenarios
{
    public static async Task CancellationAndIsolation()
    {
        var cancelled=new Machine { StartError=new OperationCanceledException() }; var retry=new Machine(); var independent=new Machine();
        await using var first=new MachineSession(cancelled.Value,_=>retry.Value,[]);
        await using var second=new MachineSession(independent.Value,_=>throw new InvalidOperationException(),[]);
        await second.PowerOnAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(first.PowerOnAsync);
        Assert.False(first.IsPowered); Assert.True(second.IsPowered); Assert.Equal(new[]{"StartAsync"},independent.Calls);
        var changes=0; EventHandler<IEmulatedMachine> handler=(_,_)=>changes++;
        first.MachineChanged+=handler; first.MachineChanged-=handler;
        await first.PowerOnAsync(); Assert.Equal(0,changes); Assert.True(first.IsPowered);
        await first.DisposeAsync(); Assert.True(second.IsPowered); Assert.Equal(new[]{"StartAsync"},independent.Calls);
        await Assert.ThrowsAsync<ObjectDisposedException>(()=>first.InsertAsync(Disk,false));
        await Assert.ThrowsAsync<ObjectDisposedException>(()=>first.EjectAsync(Disk.Slot,false));
    }
    public static async Task MediaRecreation(bool powered,bool recreate,bool eject)
    {
        var first=new Machine(); var replacement=new Machine(); var calls=0; IReadOnlyList<EmulationMedia>? supplied=null;
        await using var session=new MachineSession(first.Value,media=>{calls++; supplied=media.ToArray(); return replacement.Value;},[Disk]);
        if(powered) await session.PowerOnAsync();
        var next=Disk with{Path="replacement"};
        if(eject) await session.EjectAsync(Disk.Slot,recreate); else await session.InsertAsync(next,recreate);
        Assert.Equal(powered,session.IsPowered);
        if(!powered || recreate)
        {
            Assert.Equal(1,calls); Assert.Same(replacement.Value,session.Machine);
            Assert.Equal(powered?new[]{"StartAsync"}:Array.Empty<string>(),replacement.Calls);
            Assert.Equal(powered?new[]{"StartAsync","StopAsync","DisposeAsync"}:new[]{"StopAsync","DisposeAsync"},first.Calls);
            Assert.Equal(session.MountedMedia,supplied);
        }
        else { Assert.Equal(0,calls); Assert.Same(first.Value,session.Machine); Assert.Equal(eject?"EjectAsync":"InsertAsync",first.Calls[^1]); }
        if(eject) Assert.Empty(session.MountedMedia); else Assert.Equal(next,Assert.Single(session.MountedMedia));
    }
    private static EmulationMedia Disk => new("virtual", EmulationMediaSlot.Floppy0, EmulationMediaType.Floppy, false, true);
    public static async Task Lifecycle()
    {
        var first = new Machine(); var second = new Machine(); var factories = 0;
        await using var session = new MachineSession(first.Value, media => { factories++; Assert.Equal(Disk, Assert.Single(media)); return second.Value; }, [Disk]);
        var changed = 0; session.MachineChanged += (sender, machine) => { Assert.Same(session, sender); Assert.Same(second.Value, machine); changed++; };
        await session.TogglePauseAsync(); Assert.Empty(first.Calls);
        await session.PowerOnAsync(); await session.PowerOnAsync();
        Assert.True(session.IsPowered); Assert.Equal(new[] { "StartAsync" }, first.Calls);
        await session.TogglePauseAsync(); Assert.Equal(EmulationMachineState.Paused, first.State);
        await session.TogglePauseAsync(); Assert.Equal(EmulationMachineState.Running, first.State);
        await session.PowerOffAsync(); await session.PowerOffAsync(); Assert.False(session.IsPowered);
        Assert.Equal(new[] { "StartAsync", "PauseAsync", "ResumeAsync", "StopAsync", "DisposeAsync" }, first.Calls);
        await session.PowerOnAsync(); Assert.Equal(1, factories); Assert.Equal(1, changed);
        Assert.Same(second.Value, session.Machine); Assert.True(session.IsPowered);
        await session.DisposeAsync(); await session.DisposeAsync();
        Assert.Equal(new[] { "StartAsync", "StopAsync", "DisposeAsync" }, second.Calls);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => session.PowerOnAsync());
    }
    public static async Task Failure()
    {
        var error = new IOException("synthetic start error");
        var first = new Machine { StartError = error }; var second = new Machine();
        await using var session = new MachineSession(first.Value, _ => second.Value, []);
        Assert.Same(error, await Assert.ThrowsAsync<IOException>(() => session.PowerOnAsync()));
        Assert.False(session.IsPowered);
        Assert.Equal(new[] { "StartAsync", "StopAsync", "DisposeAsync" }, first.Calls);
        await session.PowerOnAsync(); Assert.True(session.IsPowered); Assert.Same(second.Value, session.Machine);
    }
    public static async Task MediaFailure()
    {
        var machine = new Machine();
        await using var session = new MachineSession(machine.Value, _ => throw new InvalidOperationException("Unexpected recreation"), [Disk]);
        await session.PowerOnAsync();
        var error = new IOException("synthetic media error"); machine.MediaError = error;
        Assert.Same(error, await Assert.ThrowsAsync<IOException>(() => session.InsertAsync(Disk with { Path = "replacement" }, false)));
        Assert.Equal(Disk, Assert.Single(session.MountedMedia));
        machine.MediaError = null;
        await session.InsertAsync(Disk with { Path = "replacement" }, false);
        Assert.Equal("replacement", Assert.Single(session.MountedMedia).Path);
        await session.EjectAsync(Disk.Slot, false); Assert.Empty(session.MountedMedia);
    }
    internal sealed class Machine
    {
        public List<string> Calls { get; } = [];
        public EmulationMachineState State { get; private set; }
        public Exception? StartError { get; init; }
        public Exception? MediaError { get; set; }
        public IEmulatedMachine Value { get; }
        public Machine()
        {
            var lifecycle = ControlledDependencies.Simulate<IEmulationLifecycle>((method, _) => {
                Calls.Add(method.Name);
                if (method.Name == "StartAsync" && StartError is not null) throw StartError;
                State = method.Name switch { "StartAsync" or "ResumeAsync" => EmulationMachineState.Running, "PauseAsync" => EmulationMachineState.Paused, "StopAsync" => EmulationMachineState.Stopped, _ => throw new InvalidOperationException(method.Name) };
                return ValueTask.CompletedTask;
            });
            var media = ControlledDependencies.Simulate<IEmulationMedia>((method, _) => { Calls.Add(method.Name); if (MediaError is not null) throw MediaError; return ValueTask.CompletedTask; });
            Value = ControlledDependencies.Simulate<IEmulatedMachine>((method, _) => {
                switch (method.Name) {
                    case "get_Lifecycle": return lifecycle;
                    case "get_Media": return media;
                    case "get_State": return State;
                    case "DisposeAsync": Calls.Add(method.Name); return ValueTask.CompletedTask;
                    default: throw new InvalidOperationException(method.Name);
                }
            });
        }
    }
}
