using GWGUI.App.Services.Audio;
using NAudio.Wave;
namespace GWGUI.Tests.Emulation.Audio;
internal static class AudioBufferScenarios
{
    private sealed class Sink : GWGUI.Emulation.Interfaces.IAudioOutput
    {
        public List<short[]> Samples = []; public List<int> Rates = []; public int Flushes,Stops,Disposals;
        public bool FailWrite;
        public void Start(int rate) => Rates.Add(rate);
        public void Write(ReadOnlySpan<short> samples) { if(FailWrite) throw new IOException("synthetic audio write"); Samples.Add(samples.ToArray()); }
        public void Flush() => Flushes++;
        public void Stop() => Stops++;
        public void Dispose() => Disposals++;
    }
    public static void ControlledAudio()
    {
        var sink = new Sink(); using var audio = new GWGUI.Emulation.Atari.Services.AtariAudioOutputController(sink);
        short[] samples = [1000,-1000,32766,-32768];
        var chunk = new GWGUI.Emulation.Contracts.AudioChunk(samples,48000,2,1,TimeSpan.Zero);
        audio.Start(48000); audio.SetVolume(.5f); audio.Write(chunk);
        Assert.Equal(new short[]{500,-500,16383,-16384},Assert.Single(sink.Samples)); Assert.Equal(new[]{48000},sink.Rates);
        Assert.Equal(new short[]{1000,-1000,32766,-32768},samples);
        audio.SetVolume(-2); Assert.Equal(0,audio.Volume); audio.Write(chunk); Assert.All(sink.Samples[^1],value=>Assert.Equal(0,value));
        audio.SetVolume(2); Assert.Equal(1,audio.Volume);
        audio.SetMuted(true); Assert.True(audio.IsMuted); audio.Write(chunk); Assert.Equal(2,sink.Samples.Count); Assert.Equal(1,sink.Flushes);
        audio.SetMuted(false); audio.Write(chunk); Assert.Equal(samples,sink.Samples[^1]);
        audio.Pause(); audio.Write(chunk); Assert.Equal(3,sink.Samples.Count); Assert.Equal(1,sink.Stops); Assert.Equal(2,sink.Flushes);
        audio.Resume(); audio.Write(chunk with {SampleRate=44100}); Assert.Equal(new[]{48000,48000,44100},sink.Rates); Assert.Equal(4,sink.Samples.Count);
        audio.Reset(); Assert.Equal(3,sink.Flushes); audio.Stop(); audio.Write(chunk); Assert.Equal(4,sink.Samples.Count);
        audio.Dispose(); Assert.Equal(1,sink.Disposals);
    }
    public static void OutputRecovery(bool available)
    {
        var first = new Sink { FailWrite = true }; var second = new Sink(); var created=0;
        using var audio = new GWGUI.Emulation.Atari.Services.AtariAudioOutputController(first,()=>{created++;return available?second:throw new IOException("synthetic unavailable output");});
        audio.Start(44100); audio.Write(new(new short[]{12,-34},44100,1,0,TimeSpan.Zero));
        Assert.Equal(1,first.Disposals); Assert.Equal(1,created); Assert.Equal(available?1:0,second.Samples.Count);
        if(available) Assert.Equal(new short[]{12,-34},second.Samples[0]);
        audio.ReplaceFactory(()=>second); audio.Write(new(new short[]{56,-78},48000,1,1,TimeSpan.Zero));
        Assert.Equal(new short[]{56,-78},second.Samples[^1]); Assert.Equal(48000,second.Rates[^1]);
    }
    private sealed class Output:IWavePlayer
    {
        public IWaveProvider? Provider;
        public int Plays,Stops,Disposals;
        public float Volume{get;set;}
        public WaveFormat OutputWaveFormat=>Provider!.WaveFormat;
        public PlaybackState PlaybackState{get;private set;}
        public event EventHandler<StoppedEventArgs>? PlaybackStopped {add{} remove{}}
        public void Init(IWaveProvider provider)=>Provider=provider;
        public void Play(){Plays++;PlaybackState=PlaybackState.Playing;}
        public void Pause()=>PlaybackState=PlaybackState.Paused;
        public void Stop(){Stops++;PlaybackState=PlaybackState.Stopped;}
        public void Dispose()=>Disposals++;
    }
    public static void Samples()
    {
        var device=new Output();
        using var audio=new WasapiAudioOutput("virtual",1,(id,latency)=>{
            Assert.Equal("virtual",id);Assert.Equal(10,latency);return device;
        });
        audio.Start(48000);
        Assert.Equal(1,device.Plays);
        Assert.Equal(48000,device.Provider!.WaveFormat.SampleRate);
        Assert.Equal(2,device.Provider.WaveFormat.Channels);
        audio.Write(new short[]{1,-2,32767,short.MinValue});
        var bytes=new byte[8];
        Assert.Equal(8,device.Provider.Read(bytes,0,8));
        Assert.Equal(new byte[]{1,0,254,255,255,127,0,128},bytes);
        audio.Write(new short[]{12,34});audio.Flush();
        device.Provider.Read(bytes,0,8);
        Assert.All(bytes,value=>Assert.Equal(0,value));
    }
    public static void Lifecycle()
    {
        var device=new Output();
        var audio=new WasapiAudioOutput(null,1000,(_,latency)=>{Assert.Equal(500,latency);return device;});
        Assert.Throws<InvalidOperationException>(()=>audio.Write(new short[]{1,2}));
        audio.Start(44100);
        Assert.Throws<InvalidOperationException>(()=>audio.Start(44100));
        audio.Stop();audio.Stop();
        Assert.Equal(1,device.Stops);Assert.Equal(1,device.Disposals);
        Assert.Throws<InvalidOperationException>(()=>audio.Write(new short[]{1,2}));
        audio.Dispose();audio.Dispose();
        Assert.Throws<ObjectDisposedException>(()=>audio.Start(44100));
        Assert.Throws<ObjectDisposedException>(()=>audio.Write(new short[]{1,2}));
    }
}
