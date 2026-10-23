using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Enums;

namespace GWGUI.Tests.Emulation;

public sealed class CoreHostResponseTests
{
    [Theory]
    [InlineData("C:\\GW GUI\\gwgui.exe", "C:\\GW GUI", "--core-host", "pipe with spaces", "video map")]
    [InlineData("C:\\GW GUI\\gwgui.exe", "D:\\Sessions\\Amiga", "--amiga-core-host", "pipe", "video")]
    [InlineData("D:\\Émulation\\hôte.exe", "D:\\Émulation", "--custom-host", "pipe-1", "map-2")]
    public void HostStartInfoPreservesDirectoriesFlagsAndSeparateArguments(
        string executable, string workingDirectory, string command, string pipe, string videoMap)
    {
        var startInfo = EmulationSdk.HostProtocol.CreateHostStartInfo(
            executable, workingDirectory, command, pipe, videoMap);

        Assert.Equal(executable, startInfo.FileName);
        Assert.Equal(workingDirectory, startInfo.WorkingDirectory);
        Assert.False(startInfo.UseShellExecute);
        Assert.True(startInfo.CreateNoWindow);
        Assert.Equal(new[] { command, pipe, videoMap }, startInfo.ArgumentList);
        Assert.Empty(startInfo.Arguments);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InitializationPreservesTypedJsonStatusPositionAndRepeatedDiagnosticReads(bool withStatus)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        using var core = new Core();
        var count = 99;
        EmulationSdk.HostProtocol.WriteCoreInitialization<Core, Configuration, Option>(
            writer, core, new JsonSerializerOptions(JsonSerializerDefaults.Web), ref count, ';',
            withStatus ? static (output, source) =>
            {
                source.Trace.Add("status");
                output.Write(source.ProfileStatus);
            } : null);
        writer.Flush();
        Assert.False(core.Disposed);
        Assert.Equal(2, count);
        Assert.Equal(withStatus
            ? new[] { "hash", "fps", "rate", "status", "options", "diagnostics", "diagnostics", "name", "version", "extensions" }
            : new[] { "hash", "fps", "rate", "options", "diagnostics", "diagnostics", "name", "version", "extensions" }, core.Trace);

        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        Assert.Equal("hash", reader.ReadString());
        Assert.Equal(59.5, reader.ReadDouble());
        Assert.Equal(44100, reader.ReadInt32());
        if (withStatus) Assert.Equal(1234, reader.ReadInt32());
        using var options = JsonDocument.Parse(reader.ReadString());
        Assert.Equal(7, options.RootElement[0].GetProperty("code").GetInt32());
        Assert.Equal("module-specific", options.RootElement[0].GetProperty("profileDetail").GetString());
        using var diagnostics = JsonDocument.Parse(reader.ReadString());
        Assert.Single(diagnostics.RootElement.EnumerateArray());
        Assert.Equal("first", diagnostics.RootElement[0].GetString());
        Assert.Equal("core", reader.ReadString());
        Assert.Equal("version", reader.ReadString());
        Assert.Equal("a;mid;Z", reader.ReadString());
        Assert.Equal(stream.Length, stream.Position);
    }

    [Fact]
    public void FailedStatusLeavesTheCounterAndFollowingFieldsUntouched()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        using var core = new Core();
        var count = 42;
        var error = Assert.Throws<InvalidOperationException>(() =>
            EmulationSdk.HostProtocol.WriteCoreInitialization<Core, Configuration, Option>(
                writer, core, new JsonSerializerOptions(), ref count, '|', static (_, source) =>
                {
                    source.Trace.Add("status");
                    throw new InvalidOperationException("status");
                }));
        Assert.Equal("status", error.Message);
        Assert.Equal(42, count);
        Assert.Equal(new[] { "hash", "fps", "rate", "status" }, core.Trace);
        writer.Flush();
        Assert.False(core.Disposed);
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        Assert.Equal("hash", reader.ReadString());
        Assert.Equal(59.5, reader.ReadDouble());
        Assert.Equal(44100, reader.ReadInt32());
        Assert.Equal(stream.Length, stream.Position);
    }

    [Fact]
    public void FailedFirstWriteDoesNotReadLaterCorePropertiesOrAdvanceTheCounter()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        using var core = new Core();
        writer.Dispose();
        stream.Dispose();
        var count = 42;
        Assert.Throws<ObjectDisposedException>(() =>
            EmulationSdk.HostProtocol.WriteCoreInitialization<Core, Configuration, Option>(
                writer, core, new JsonSerializerOptions(), ref count, '|'));
        Assert.Equal(42, count);
        Assert.Equal(new[] { "hash" }, core.Trace);
        Assert.False(core.Disposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AudioResponseDrainsTheQueueBeforeReadingTiming(bool withAudio)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        using var core = new Core();
        if (withAudio) core.Chunks.Enqueue(new AudioChunk(new short[] { 123, -456 }, 44100, 1, 17, TimeSpan.FromTicks(23)));
        EmulationHostProtocolFunctions.WriteCoreAudio(writer, core);
        writer.Flush();
        Assert.False(core.Disposed);
        Assert.Equal(withAudio ? new[] { "dequeue", "dequeue", "fps", "rate" } : new[] { "dequeue", "fps", "rate" }, core.Trace);
        Assert.Empty(core.Chunks);
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var chunks = EmulationSdk.HostProtocol.ReadAudio(reader, "response-test");
        if (withAudio)
        {
            var chunk = Assert.Single(chunks);
            Assert.Equal(new short[] { 123, -456 }, chunk.InterleavedSamples.ToArray());
            Assert.Equal(44100, chunk.SampleRate);
            Assert.Equal(17, chunk.Sequence);
            Assert.Equal(TimeSpan.FromTicks(23), chunk.Timestamp);
        }
        else Assert.Empty(chunks);
        Assert.Equal(72.5, reader.ReadDouble());
        Assert.Equal(22050, reader.ReadInt32());
        Assert.Equal(stream.Length, stream.Position);
    }

    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, true, true)]
    [InlineData(false, true, true, false)]
    [InlineData(true, true, false, true)]
    public void NativeFramePreservesWirePositionsAndPreDrainCounters(bool support, bool latency, bool changed, bool media)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        using var core = new Core { Latency = latency ? 33 : 0 };
        long sequence = 17;
        var count = changed ? 0 : 1;
        EmulationSdk.HostProtocol.WriteNativeCoreFrameResponse<Core, Configuration, Option>(
            writer, core, (output, frame) =>
            {
                core.Trace.Add("frame");
                Assert.Null(frame);
                output.Write(12345678);
            }, (output, states) =>
            {
                core.Trace.Add("led-write");
                Assert.Empty(states);
                output.Write(44);
            }, () =>
            {
                core.Trace.Add("json");
                return new JsonSerializerOptions(JsonSerializerDefaults.Web);
            }, ref sequence, ref count,
            support ? static (output, source) => output.Write(source.SupportsSaveStates) : null,
            media ? (output, _) => { core.Trace.Add("media"); output.Write(55); } : null);
        writer.Flush();

        var trace = new List<string> { "video", "frame", "buffered", "overrun", "underrun", "dequeue", "fps", "rate" };
        trace.Add("latency");
        trace.Add("region");
        if (support) trace.Add("support");
        if (media) trace.Add("media");
        trace.Add("diagnostics");
        if (changed) trace.Add("json");
        trace.AddRange(new[] { "led", "led-write" });
        Assert.Equal(trace, core.Trace);
        Assert.Equal(17, sequence);
        Assert.Equal(1, count);
        Assert.False(core.Disposed);

        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        Assert.Equal(12345678, reader.ReadInt32()); // No legacy success boolean precedes the frame.
        Assert.Empty(EmulationSdk.HostProtocol.ReadAudio(reader, "native-response-test"));
        Assert.Equal(72.5, reader.ReadDouble());
        Assert.Equal(22050, reader.ReadInt32());
        Assert.Equal(latency ? 33 : 0, reader.ReadInt32());
        Assert.Equal(1, reader.ReadInt32());
        Assert.Equal(9, reader.ReadInt32());
        Assert.Equal(10, reader.ReadInt64());
        Assert.Equal(11, reader.ReadInt64());
        if (support) Assert.True(reader.ReadBoolean());
        if (media) Assert.Equal(55, reader.ReadInt32());
        Assert.Equal(changed, reader.ReadBoolean());
        if (changed)
        {
            using var diagnostics = JsonDocument.Parse(reader.ReadString());
            Assert.Equal(new[] { "first" }, diagnostics.RootElement.EnumerateArray().Select(item => item.GetString()));
        }
        Assert.Equal(44, reader.ReadInt32());
        Assert.Equal(stream.Length, stream.Position);
    }

    [Fact]
    public void NativeFrameWriterFailureStopsBeforeAudioAndKeepsBorrowedCoreAndCounters()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        using var core = new Core();
        long sequence = 17;
        var count = 42;
        var failure = new InvalidOperationException("frame");
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() =>
            EmulationSdk.HostProtocol.WriteNativeCoreFrameResponse<Core, Configuration, Option>(
                writer, core, (_, _) => throw failure,
                (_, _) => throw new Xunit.Sdk.XunitException("LED writer must not run"),
                () => throw new Xunit.Sdk.XunitException("JSON getter must not run"), ref sequence, ref count)));
        Assert.Equal(new[] { "video" }, core.Trace);
        Assert.Equal(17, sequence);
        Assert.Equal(42, count);
        Assert.False(core.Disposed);
        Assert.True(stream.CanWrite);
    }

    private sealed record Configuration;
    private sealed record Option(int Code, [property: JsonPropertyName("profileDetail")] string Detail);

    private sealed class Core : IEmulationCoreRuntime<Configuration, Option>, IEmulationCoreRuntimeStatus, IEmulationAudioBufferFeedback
    {
        internal int Latency;
        public int MinimumAudioLatencyMilliseconds => Read("latency", Latency);
        public void SetAudioBufferStatus(AudioOutputBufferStatus? status) { }
        private int _diagnosticReads;
        private double _fps = 59.5;
        private int _sampleRate = 44100;
        private bool _drained;
        internal List<string> Trace { get; } = [];
        internal Queue<AudioChunk> Chunks { get; } = new();
        internal bool Disposed { get; private set; }
        internal int ProfileStatus => 1234;
        public VideoFrame? LatestVideoFrame => Read<VideoFrame?>("video", null);
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<Option> Options => Read("options", new[] { new Option(7, "module-specific") });
        public IReadOnlyList<string> Diagnostics => Read("diagnostics", ++_diagnosticReads == 1 ? new[] { "first" } : new[] { "first", "second" });
        public IReadOnlyDictionary<int, bool> LedStates => Read<IReadOnlyDictionary<int, bool>>("led", new Dictionary<int, bool>());
        public HostProcessState HostProcessState => HostProcessState.InProcess;
        public int? HostProcessId => null;
        public bool SupportsSaveStates => Read("support", true);
        public EmulationVideoStandard? Region => Read<EmulationVideoStandard?>("region", _drained ? EmulationVideoStandard.Pal : EmulationVideoStandard.Ntsc);
        public int BufferedAudioFrames => Read("buffered", _drained ? 99 : 9);
        public long AudioOverrunCount => Read("overrun", _drained ? 100L : 10L);
        public long AudioUnderrunCount => Read("underrun", _drained ? 101L : 11L);
        public string CoreName => Read("name", "core");
        public string CoreVersion => Read("version", "version");
        public IReadOnlySet<string> SupportedContentExtensions => Read("extensions", new HashSet<string> { "Z", "a", "mid" });
        public string CoreSha256 => Read("hash", "hash");
        public double FramesPerSecond => Read("fps", _fps);
        public int SampleRate => Read("rate", _sampleRate);
        public void Initialize(Configuration configuration, string sessionDirectory, string? saveDirectory = null) => throw new NotSupportedException();
        public void RunFrame() => throw new NotSupportedException();
        public void SetInput(EmulationInputSnapshot snapshot) => throw new NotSupportedException();
        public bool TryDequeueAudio(out AudioChunk? chunk)
        {
            Trace.Add("dequeue");
            if (Chunks.TryDequeue(out chunk)) return true;
            _drained = true;
            _fps = 72.5;
            _sampleRate = 22050;
            return false;
        }
        public void Dispose() { Disposed = true; Chunks.Clear(); }
        private T Read<T>(string name, T value) { Trace.Add(name); return value; }
    }
}
