using GWGUI.App.Services.Emulation;
using GWGUI.Emulation.Atari.Functions;
using GWGUI.Emulation.Atari.Exceptions;
using GWGUI.Emulation.Atari.Enums;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Exceptions;
using GWGUI.Tests.Emulation.EmulationContracts;
using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Contracts;
using GWGUI.Emulation.Amiga.Interfaces;
using GWGUI.Emulation.Amiga.Services;
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
        await using var machine = new AmigaMachine(Guid.NewGuid(),AmigaMachineConfiguration.A500("virtual-rom"),core,"virtual-session",
            deleteSession:path=>{Assert.Equal("virtual-session",path);cleanups++;});
        if(failure<2)
        {
            Assert.Same(error,await Assert.ThrowsAsync<FileNotFoundException>(()=>machine.StartAsync().AsTask()));
            await machine.StopAsync(); Assert.Equal(EmulationMachineState.Faulted,machine.State);
            Assert.Equal("synthetic diagnostic",error.Data["AmigaDiagnostics"]);
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
            Assert.Same(error,await Assert.ThrowsAsync<InvalidOperationException>(()=>inserting));
            Assert.Empty(machine.Media.MountedMedia); Assert.Equal(EmulationMachineState.Paused,machine.State);
            core.MediaError=null; await machine.Media.InsertAsync(media,default);
            Assert.Equal(media,Assert.Single(machine.Media.MountedMedia));
            await machine.StopAsync(); Assert.Equal(EmulationMachineState.Stopped,machine.State);
        }
        await machine.DisposeAsync(); await machine.DisposeAsync(); Assert.Equal(1,cleanups); Assert.Equal(1,core.Disposals);
    }
    private sealed class Core : IAmigaCore
    {
        public Exception? InitializeError, FrameError, MediaError;
        public bool BlockFrame;
        public int Stops,Disposals;
        public TaskCompletionSource Stopped {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource FrameEntered {get;}=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public ManualResetEventSlim FrameRelease {get;}=new();
        public VideoFrame? LatestVideoFrame=>null; public AudioChunk? LatestAudioChunk=>null;
        public bool TryDequeueAudio(out AudioChunk? chunk){chunk=null;return false;}
        public IReadOnlyList<AmigaCoreOption> Options=>[]; public IReadOnlyList<string> Diagnostics=>["synthetic diagnostic"];
        public IReadOnlyDictionary<int,bool> LedStates=>new Dictionary<int,bool>();
        public string CoreName=>"synthetic"; public string CoreVersion=>"1"; public string CoreSha256=>"synthetic";
        public IReadOnlySet<string> SupportedContentExtensions=>new HashSet<string>{"adf"};
        public double FramesPerSecond=>1000; public int SampleRate=>44100; public int DiskCount=>0; public int CurrentDiskIndex=>0;
        public void Initialize(AmigaMachineConfiguration configuration,string sessionDirectory,string? saveDirectory=null)
        {Assert.Equal("virtual-rom",configuration.KickstartPath);Assert.Equal("virtual-session",sessionDirectory);if(InitializeError is {} error)throw error;}
        public void RunFrame(){FrameEntered.TrySetResult();if(BlockFrame){if(!FrameRelease.Wait(TimeSpan.FromSeconds(5)))throw new TimeoutException();BlockFrame=false;}if(FrameError is {} error)throw error;}
        public void Stop(){Stops++;Stopped.TrySetResult();}
        public void Dispose(){Disposals++;FrameRelease.Set();}
        public void InsertMedia(string path){Assert.Equal(Path.GetFullPath("virtual.adf"),path);if(MediaError is {} error)throw error;}
        public void HardReset()=>throw new InvalidOperationException("unexpected reset");
        public void SetInput(EmulationInputSnapshot snapshot)=>throw new InvalidOperationException("unexpected input");
        public void EjectMedia()=>throw new InvalidOperationException("unexpected eject");
        public void SelectDisk(int index)=>throw new InvalidOperationException("unexpected disk selection");
        public byte[] SaveState()=>throw new InvalidOperationException("unexpected save");
        public void LoadState(ReadOnlySpan<byte> state)=>throw new InvalidOperationException("unexpected load");
        public void SetOption(string key,string value)=>throw new InvalidOperationException("unexpected option");
    }
    public static async Task Boundary(string category,string code,string expectedCategory,string expectedCode)
    {
        var configuration = new GWGUI.Emulation.Atari.Contracts.AtariMachineConfiguration(AtariMachineModel.Ste);
        var original = new AtariEmulationException(Enum.Parse<AtariErrorCategory>(category),Enum.Parse<AtariErrorCode>(code),"synthetic core response");
        var translated = Assert.IsType<EmulationMessageException>(AtariMessageFunctions.Translate(original,configuration));
        Assert.Same(original,translated.InnerException);
        Assert.Equal(expectedCategory,translated.MessageData.Category.ToString()); Assert.Equal(expectedCode,translated.MessageData.MessageCode.ToString());
        Assert.Equal(EmulationMessageTarget.Dialog,translated.MessageData.Target); Assert.Equal(EmulationMessageSeverity.Error,translated.MessageData.Severity);
        var machine = new SessionLifecycleScenarios.Machine { StartError = translated };
        await using var session = new MachineSession(machine.Value,_ => throw new InvalidOperationException("No external core"),[]);
        Assert.Same(translated,await Assert.ThrowsAsync<EmulationMessageException>(session.PowerOnAsync));
        Assert.False(session.IsPowered); Assert.Equal(new[] {"StartAsync","StopAsync","DisposeAsync"},machine.Calls);
        var unrelated = new IOException("host failure"); Assert.Same(unrelated,AtariMessageFunctions.Translate(unrelated,configuration));
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
