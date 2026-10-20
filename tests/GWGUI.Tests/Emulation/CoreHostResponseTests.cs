using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;

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
        var startInfo = EmulationHostProtocolFunctions.CreateHostStartInfo(
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
        EmulationHostProtocolFunctions.WriteCoreInitialization<Core, Configuration, Option>(
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
            EmulationHostProtocolFunctions.WriteCoreInitialization<Core, Configuration, Option>(
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
            EmulationHostProtocolFunctions.WriteCoreInitialization<Core, Configuration, Option>(
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
        var chunks = EmulationHostProtocolFunctions.ReadAudio(reader, "response-test");
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

    private sealed record Configuration;
    private sealed record Option(int Code, [property: JsonPropertyName("profileDetail")] string Detail);

    private sealed class Core : IEmulationCoreRuntime<Configuration, Option>
    {
        private int _diagnosticReads;
        private double _fps = 59.5;
        private int _sampleRate = 44100;
        internal List<string> Trace { get; } = [];
        internal Queue<AudioChunk> Chunks { get; } = new();
        internal bool Disposed { get; private set; }
        internal int ProfileStatus => 1234;
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<Option> Options => Read("options", new[] { new Option(7, "module-specific") });
        public IReadOnlyList<string> Diagnostics => Read("diagnostics", ++_diagnosticReads == 1 ? new[] { "first" } : new[] { "first", "second" });
        public IReadOnlyDictionary<int, bool> LedStates { get; } = new Dictionary<int, bool>();
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
            _fps = 72.5;
            _sampleRate = 22050;
            return false;
        }
        public void Dispose() { Disposed = true; Chunks.Clear(); }
        private T Read<T>(string name, T value) { Trace.Add(name); return value; }
    }
}
