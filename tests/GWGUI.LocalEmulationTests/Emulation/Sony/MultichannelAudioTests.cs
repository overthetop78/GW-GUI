using GWGUI.Emulation;
using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Sony.Common.Interfaces;
using GWGUI.Emulation.Sony.Common.Contracts;
using GWGUI.Emulation.Sony.Common.Machines.Common.Enums;
using GWGUI.Emulation.Sony.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Services;
using Rpcs3 = GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants.CoreConstants;
using Rpcs3Options = GWGUI.Emulation.Sony.Emulators.Rpcs3.Constants.OptionConstants;

namespace GWGUI.Tests.Emulation.Sony;

public sealed class MultichannelAudioTests
{
    [Fact]
    public async Task MachineKeepsTheLayoutAndVolumeWhenTheOutputIsReplaced()
    {
        var layouts = new[] { EmulationAudioChannelLayout.Stereo, EmulationAudioChannelLayout.Surround51, EmulationAudioChannelLayout.Surround71 };
        var chunks = layouts.Select((layout, index) => new AudioChunk(
            Enumerable.Range(1, System.Numerics.BitOperations.PopCount((uint)layout)).Select(value => (short)(value * 200)).ToArray(),
            48000, 1, index + 1, TimeSpan.Zero, layout)).ToArray();
        var core = new AudioCore(chunks);
        var first = new Sink();
        var second = new Sink();
        var machine = new GWGUI.Emulation.Sony.Common.Services.Machine(Guid.NewGuid(), new("PlayStation3", Rpcs3.Id), core, [], "unused", deleteSession: _ => { }, audioOutputFactory: () => first);
        var received = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        machine.Audio.SetVolume(.5f);
        machine.Audio.ChunkReady += (_, chunk) =>
        {
            if (chunk.ChannelLayout == EmulationAudioChannelLayout.Surround51) machine.Audio.SetOutputFactory(() => second);
            if (chunk.ChannelLayout == EmulationAudioChannelLayout.Surround71) received.TrySetResult();
        };
        try
        {
            await machine.Lifecycle.StartAsync();
            await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await machine.Lifecycle.StopAsync();
            Assert.Equal(new[] { EmulationAudioChannelLayout.Stereo, EmulationAudioChannelLayout.Surround51 }, first.Layouts);
            Assert.Equal(new[] { EmulationAudioChannelLayout.Surround51, EmulationAudioChannelLayout.Surround71 }, second.Layouts);
            Assert.Equal(new short[] { 100, 200, 300, 400, 500, 600, 700, 800 }, Assert.Single(second.Writes));
        }
        finally { await machine.DisposeAsync(); }
        Assert.Equal(1, first.Disposals);
        Assert.Equal(1, second.Disposals);
        Assert.True(core.Disposed);
    }

    [Theory]
    [InlineData(1u, 0x004u, EmulationAudioChannelLayout.Mono)]
    [InlineData(2u, 0x003u, EmulationAudioChannelLayout.Stereo)]
    [InlineData(6u, 0x60Fu, EmulationAudioChannelLayout.Surround51)]
    [InlineData(8u, 0x63Fu, EmulationAudioChannelLayout.Surround71)]
    public void NativeChannelsReachTheQueueAndProtocolWithoutReordering(uint channels, uint mask, EmulationAudioChannelLayout layout)
    {
        using var callbacks = Callbacks();
        var callback = Marshal.AllocHGlobal(Marshal.SizeOf<MultichannelAudioApi.Callback>());
        var data = Marshal.AllocHGlobal(checked((int)channels * 3 * sizeof(short)));
        try
        {
            Assert.True(callbacks.Environment(MultichannelAudioConstants.GetBatchCallback, callback));
            var pointers = Marshal.PtrToStructure<MultichannelAudioApi.Callback>(callback);
            Assert.NotEqual(nint.Zero, pointers.Int16);
            Assert.Equal(nint.Zero, pointers.Float);
            var batch = Marshal.GetDelegateForFunctionPointer<MultichannelAudioApi.BatchInt16>(pointers.Int16);
            var expected = Enumerable.Range(1, checked((int)channels * 3)).Select(value => (short)value).ToArray();
            Marshal.Copy(expected, 0, data, expected.Length);
            Assert.Equal((nuint)3, batch(data, 3, channels, mask));
            Assert.True(callbacks.TryDequeueAudio(out var chunk));
            Assert.True(chunk!.HasValidLength);
            Assert.Equal(layout, chunk.ChannelLayout);
            Assert.Equal(expected, chunk.InterleavedSamples.ToArray());
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true)) EmulationHostProtocolFunctions.WriteAudio(writer, [chunk]);
            stream.Position = 0;
            using var reader = new BinaryReader(stream);
            var restored = Assert.Single(EmulationSdk.HostProtocol.ReadAudio(reader, "test"));
            Assert.Equal(layout, restored.ChannelLayout);
            Assert.Equal(expected, restored.InterleavedSamples.ToArray());
            Assert.Equal(chunk.FrameCount, restored.FrameCount);
        }
        finally { Marshal.FreeHGlobal(data); Marshal.FreeHGlobal(callback); }
    }

    [Theory]
    [InlineData(0u, 0u)]
    [InlineData(6u, 0x003u)]
    [InlineData(2u, 0x60Fu)]
    [InlineData(1u, 0x10000000u)]
    [InlineData(9u, 0x63Fu)]
    public void InvalidNativeLayoutsAreRejectedBeforeReadingSamples(uint channels, uint mask)
    {
        using var callbacks = Callbacks();
        var data = Marshal.AllocHGlobal(sizeof(short));
        try
        {
            Assert.Equal((nuint)0, callbacks.MultichannelAudioBatch(data, 1, channels, mask));
            Assert.Equal((nuint)0, callbacks.MultichannelAudioBatch(data, (nuint)MultichannelAudioConstants.MaximumFramesPerBatch + 1, 2, 3));
            Assert.False(callbacks.TryDequeueAudio(out _));
        }
        finally { Marshal.FreeHGlobal(data); }
    }

    [Fact]
    public void QueueTrimmingPreservesEveryChannelAndItsLayout()
    {
        using var callbacks = Callbacks();
        callbacks.SampleRate = 100;
        var samples = Enumerable.Range(0, 100 * 6).Select(value => (short)value).ToArray();
        var data = Marshal.AllocHGlobal(samples.Length * sizeof(short));
        try
        {
            Marshal.Copy(samples, 0, data, samples.Length);
            Assert.Equal((nuint)100, callbacks.MultichannelAudioBatch(data, 100, 6, 0x60F));
            Assert.True(callbacks.TryDequeueAudio(out var chunk));
            Assert.Equal(20, chunk!.FrameCount);
            Assert.Equal(EmulationAudioChannelLayout.Surround51, chunk.ChannelLayout);
            Assert.Equal(samples[^120..], chunk.InterleavedSamples.ToArray());
        }
        finally { Marshal.FreeHGlobal(data); }
    }

    [Theory]
    [InlineData("stereo", "2.0")]
    [InlineData("surround_5_1", "5.1")]
    [InlineData("surround_7_1", "7.1")]
    public void SpeakerLayoutChoiceIsAppliedAndObsoleteSaveSlotIsAbsent(string value, string display)
    {
        using var http = new System.Net.Http.HttpClient();
        var module = new GWGUI.Emulation.Sony.Modules.SonyEmulationModule(".", ".", http, ".");
        var configuration = new MachineConfiguration("PlayStation3", Rpcs3.Id);
        var fields = module.Describe(configuration.Model, configuration).Blocks.SelectMany(block => block.Fields).ToArray();
        var field = Assert.Single(fields, field => field.Id == Rpcs3Options.AudioFormatKey);
        Assert.Equal(EmulationMachineTab.Audio, field.Tab);
        Assert.Equal(display, Assert.Single(field.Choices!, choice => choice.Id == value).InvariantDisplayValue);
        Assert.DoesNotContain(fields, field => field.Id == "rpcs3_savedata_slot");
        var updated = (MachineConfiguration)module.ApplySettings(configuration, new Dictionary<string, string?> { [field.Id] = value });
        var runtime = new GWGUI.Emulation.Sony.Emulators.Rpcs3.Factories.Rpcs3MachineFactory().GetRuntimeOptions(updated);
        Assert.Equal(value, runtime[field.Id]);
    }

    [Fact]
    public void InvalidPacketLengthIsRejectedOnBothSides()
    {
        var chunk = new AudioChunk(new short[6], 48000, 2, 1, TimeSpan.Zero, EmulationAudioChannelLayout.Surround51);
        Assert.False(chunk.HasValidLength);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        Assert.Throws<InvalidDataException>(() => EmulationHostProtocolFunctions.WriteAudio(writer, [chunk]));
        stream.SetLength(0);
        writer.Write(1); writer.Write(48000); writer.Write(2); writer.Write((uint)EmulationAudioChannelLayout.Surround51);
        writer.Write(1L); writer.Write(0L); writer.Write(12); writer.Write(new byte[12]); writer.Flush();
        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        Assert.Throws<InvalidDataException>(() => EmulationSdk.HostProtocol.ReadAudio(reader, "test"));
    }

    private static ExternalHostCallbacks Callbacks() => new(Path.GetTempPath(), Path.GetTempPath(), Path.GetTempPath(), null);

    private sealed class Sink : IMultichannelAudioOutput
    {
        internal List<EmulationAudioChannelLayout> Layouts { get; } = [];
        internal List<short[]> Writes { get; } = [];
        internal int Disposals;
        public void Start(int sampleRate) => Start(sampleRate, EmulationAudioChannelLayout.Stereo);
        public void Start(int sampleRate, EmulationAudioChannelLayout layout) => Layouts.Add(layout);
        public void Write(ReadOnlySpan<short> samples) => Writes.Add(samples.ToArray());
        public void Flush() { }
        public void Stop() { }
        public void Dispose() => Disposals++;
    }

    private sealed class AudioCore(IEnumerable<AudioChunk> chunks) : IEmulatorCore
    {
        private readonly Queue<AudioChunk> _chunks = new(chunks);
        private int _frames;
        internal bool Disposed;
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<CoreOption> Options => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => new Dictionary<int, bool>();
        public string CoreName => "Audio test";
        public string CoreVersion => "1";
        public string CoreSha256 => string.Empty;
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public double FramesPerSecond => 1000;
        public int SampleRate => 48000;
        public int DiskCount => 0;
        public int CurrentDiskIndex => -1;
        public bool ShutdownRequested => _frames > 1;
        public bool TryDequeueAudio(out AudioChunk? chunk) => _chunks.TryDequeue(out chunk);
        public void Initialize(MachineConfiguration configuration, string sessionDirectory, string? saveDirectory = null) { }
        public void RunFrame() => _frames++;
        public void Stop() { }
        public void Dispose() => Disposed = true;
        public void SetInput(EmulationInputSnapshot snapshot) { }
        public void SetControllerPortDevice(int port, PeripheralCategory type) { }
        public void SetOption(string key, string value) => throw new NotSupportedException();
        public void HardReset() => throw new NotSupportedException();
        public void SoftReset() => throw new NotSupportedException();
        public void InsertMedia(string path) => throw new NotSupportedException();
        public void EjectMedia() => throw new NotSupportedException();
        public void SelectDisk(int index) => throw new NotSupportedException();
        public byte[] SaveState() => throw new NotSupportedException();
        public void LoadState(ReadOnlySpan<byte> state) => throw new NotSupportedException();
    }
}
