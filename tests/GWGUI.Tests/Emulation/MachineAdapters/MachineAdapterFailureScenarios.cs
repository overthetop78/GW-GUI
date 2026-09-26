using GWGUI.App.Services.Emulation;
using GWGUI.Emulation.Atari.Common.Machines.Common.Functions;
using GWGUI.Emulation.Atari.Emulators.Libretro.Exceptions;
using GWGUI.Emulation.Atari.Common.Machines.Common.Enums;
using GWGUI.Emulation.Atari.Emulators.Libretro.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Exceptions;
using GWGUI.Tests.Emulation.EmulationContracts;
using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Common.Contracts;
using GWGUI.Emulation.Amiga.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Amiga.Common.Interfaces;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Interfaces;
using GWGUI.Emulation.Amiga.Common.Services;
namespace GWGUI.Tests.Emulation.MachineAdapters;
internal static class MachineAdapterFailureScenarios
{
    public static async Task AmigaBoundary(int failure)
    {
        var error = failure==0 ? new FileNotFoundException("core unavailable","virtual-core")
            : failure==1 ? new FileNotFoundException("firmware absent","virtual-rom")
            : (Exception)new InvalidOperationException("synthetic adapter refusal");
        using var core = new Core { InitializeError=failure<2?error:null, FrameError=failure==2?error:null };
        var cleanups=0;
        await using var machine = new Machine(Guid.NewGuid(),MachineConfiguration.A500("virtual-rom"),core,[],"virtual-session",
            deleteSession:path=>{Assert.Equal("virtual-session",path);cleanups++;});
        if(failure<2)
        {
            var translated=await Assert.ThrowsAsync<EmulationMessageException>(()=>machine.StartAsync().AsTask());
            Assert.Same(error,translated.InnerException);
            Assert.Equal(EmulationMessageCode.MachineStartFailed,translated.MessageData.MessageCode);
            await machine.StopAsync(); Assert.Equal(EmulationMachineState.Faulted,machine.State);
            Assert.Equal("synthetic diagnostic",translated.Data["AmigaDiagnostics"]);
            Assert.Equal(0,core.Stops);
        }
        else if(failure==2)
        {
            await machine.StartAsync(); await core.Stopped.Task.WaitAsync(TimeSpan.FromSeconds(5)); await machine.StopAsync();
            Assert.Equal(EmulationMachineState.Faulted,machine.State); Assert.Equal(1,core.Stops);
        }
        else
        {
            core.BlockFrame=true;
            await machine.StartAsync(); await core.FrameEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await machine.PauseAsync(); core.MediaError=error;
            var media = new EmulationMedia("virtual.adf",EmulationMediaSlot.Floppy0,EmulationMediaType.Floppy,false,true);
            var inserting = machine.Media.InsertAsync(media,default).AsTask(); core.FrameRelease.Set();
            var translated=await Assert.ThrowsAsync<EmulationMessageException>(()=>inserting);
            Assert.Same(error,translated.InnerException);
            Assert.Equal(EmulationMessageCode.MediaOperationFailed,translated.MessageData.MessageCode);
            Assert.Empty(machine.Media.MountedMedia); Assert.Equal(EmulationMachineState.Paused,machine.State);
            core.MediaError=null; await machine.Media.InsertAsync(media,default);
            Assert.Equal(media,Assert.Single(machine.Media.MountedMedia));
            await machine.StopAsync(); Assert.Equal(EmulationMachineState.Stopped,machine.State);
        }
        await machine.DisposeAsync(); await machine.DisposeAsync(); Assert.Equal(1,cleanups); Assert.Equal(1,core.Disposals);
    }
    public static async Task AmigaDiskChangeIsObservedBetweenEjectAndInsert()
    {
        var core = new Core { ExpectedMediaPath = "disk-2.adf" };
        var cleanups = 0;
        var machine = new Machine(Guid.NewGuid(), MachineConfiguration.A500("virtual-rom"), core,
            [new MediaConfiguration("disk-1.adf",
                GWGUI.Emulation.Amiga.Common.Machines.Common.Enums.MediaCategory.Floppy)], "virtual-session",
            deleteSession: path => { Assert.Equal("virtual-session", path); cleanups++; });
        try
        {
            await machine.StartAsync();
            var media = new EmulationMedia("disk-2.adf", EmulationMediaSlot.Floppy0,
                EmulationMediaType.Floppy, false, true);

            await machine.Media.InsertAsync(media, default);

            Assert.Equal(["Eject", "Insert"], core.MediaCalls);
            Assert.True(core.FrameAtInsert > core.FrameAtEject);
            Assert.Equal("disk-2.adf", Assert.Single(machine.Media.MountedMedia).Path);
        }
        finally
        {
            await machine.DisposeAsync();
        }
        Assert.Equal(1, cleanups);
        Assert.Equal(1, core.Disposals);
    }
    private sealed class Core : IEmulatorCore
    {
        public Exception? InitializeError, FrameError, MediaError;
        public string ExpectedMediaPath = "virtual.adf";
        public bool BlockFrame;
        public int Stops,Disposals,Frames,FrameAtEject,FrameAtInsert;
        public List<string> MediaCalls { get; } = [];
        public TaskCompletionSource Stopped {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource FrameEntered {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public ManualResetEventSlim FrameRelease {get;}=new();
        public VideoFrame? LatestVideoFrame=>null; public AudioChunk? LatestAudioChunk=>null;
        public bool TryDequeueAudio(out AudioChunk? chunk){chunk=null;return false;}
        public IReadOnlyList<CoreOption> Options=>[]; public IReadOnlyList<string> Diagnostics=>["synthetic diagnostic"];
        public IReadOnlyDictionary<int,bool> LedStates=>new Dictionary<int,bool>();
        public string CoreName=>"synthetic"; public string CoreVersion=>"1"; public string CoreSha256=>"synthetic";
        public IReadOnlySet<string> SupportedContentExtensions=>new HashSet<string>{"adf"};
        public double FramesPerSecond=>1000; public int SampleRate=>44100; public int DiskCount=>0; public int CurrentDiskIndex=>0;
        public void Initialize(MachineConfiguration configuration,string sessionDirectory,string? saveDirectory=null)
        {Assert.Equal("virtual-rom",configuration.KickstartPath);Assert.Equal("virtual-session",sessionDirectory);if(InitializeError is {} error)throw error;}
        public void RunFrame(){Interlocked.Increment(ref Frames);FrameEntered.TrySetResult();if(BlockFrame){if(!FrameRelease.Wait(TimeSpan.FromSeconds(5)))throw new TimeoutException();BlockFrame=false;}if(FrameError is {} error)throw error;}
        public void Stop(){Stops++;Stopped.TrySetResult();}
        public void Dispose(){Disposals++;FrameRelease.Set();}
        public void InsertMedia(string path){MediaCalls.Add("Insert");FrameAtInsert=Volatile.Read(ref Frames);Assert.Equal(Path.GetFullPath(ExpectedMediaPath),path);if(MediaError is {} error)throw error;}
        public void HardReset()=>throw new InvalidOperationException("unexpected reset");
        public void SetInput(EmulationInputSnapshot snapshot)=>throw new InvalidOperationException("unexpected input");
        public void EjectMedia(){MediaCalls.Add("Eject");FrameAtEject=Volatile.Read(ref Frames);}
        public void SelectDisk(int index)=>throw new InvalidOperationException("unexpected disk selection");
        public byte[] SaveState()=>throw new InvalidOperationException("unexpected save");
        public void LoadState(ReadOnlySpan<byte> state)=>throw new InvalidOperationException("unexpected load");
        public void SetOption(string key,string value)=>throw new InvalidOperationException("unexpected option");
    }
    public static async Task Boundary(string category,string code,string expectedCategory,string expectedCode)
    {
        var configuration = new GWGUI.Emulation.Atari.Common.Machines.Common.Contracts.MachineConfiguration(MachineModel.Ste);
        var original = new EmulationException(Enum.Parse<ErrorCategory>(category),Enum.Parse<ErrorCode>(code),"synthetic core response");
        var translated = Assert.IsType<EmulationMessageException>(MessageFunctions.Translate(original,configuration));
        Assert.Same(original,translated.InnerException);
        Assert.Equal(expectedCategory,translated.MessageData.Category.ToString()); Assert.Equal(expectedCode,translated.MessageData.MessageCode.ToString());
        Assert.Equal(EmulationMessageTarget.Dialog,translated.MessageData.Target); Assert.Equal(EmulationMessageSeverity.Error,translated.MessageData.Severity);
        var machine = new SessionLifecycleScenarios.Machine { StartError = translated };
        await using var session = new MachineSession(machine.Value,_ => throw new InvalidOperationException("No external core"),[]);
        Assert.Same(translated,await Assert.ThrowsAsync<EmulationMessageException>(session.PowerOnAsync));
        Assert.False(session.IsPowered); Assert.Equal(new[] {"StartAsync","StopAsync","DisposeAsync"},machine.Calls);
        var unrelated = new IOException("host failure");
        var fallback = Assert.IsType<EmulationMessageException>(MessageFunctions.Translate(unrelated,configuration));
        Assert.Same(unrelated,fallback.InnerException);
        Assert.Equal(EmulationMessageCode.MachineStartFailed,fallback.MessageData.MessageCode);
    }
    public static async Task RecreationFailure()
    {
        var first = new SessionLifecycleScenarios.Machine(); var second = new SessionLifecycleScenarios.Machine();
        var missing = new FileNotFoundException("synthetic core unavailable","virtual-core"); var fail = true;
        await using var session = new MachineSession(first.Value,_ => fail ? throw missing : second.Value,[]);
        await session.PowerOnAsync();
        Assert.Same(missing,await Assert.ThrowsAsync<FileNotFoundException>(session.RecreateRunningMachineAsync));
        Assert.False(session.IsPowered); Assert.Equal(new[] {"StartAsync","StopAsync","DisposeAsync"},first.Calls);
        fail = false; await session.PowerOnAsync(); Assert.Same(second.Value,session.Machine); Assert.True(session.IsPowered);
        await session.DisposeAsync(); Assert.False(session.IsPowered);
    }
}
