using System.IO;
using System.Text;
using System.Text.Json;
using GWGUI.Emulation;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class LegacyHostFrameResponseTests
{
    [Fact]
    public void ResponseKeepsBinaryOrderAndUpdatesCountersAtOriginalPoints()
    {
        using var core = new Core();
        using var input = AudioRequest();
        using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);
        long sequence = 6;
        var diagnosticCount = 0;
        var mediaWritten = false;
        var optionsRead = 0;
        EmulationSdk.HostProtocol.RunCoreFrame<Core, object, string>(
            reader, writer, core,
            _ => { core.Events.Add("input"); return EmulationInputSnapshot.Empty; },
            (frameWriter, frame) =>
            {
                Assert.Equal(6, sequence);
                Assert.Same(core.Frame, frame);
                core.Events.Add("video");
                frameWriter.Write(frame!.Sequence);
            },
            (statusWriter, _) =>
            {
                Assert.Equal(7, sequence);
                Assert.Equal(0, diagnosticCount);
                core.Events.Add("media");
                statusWriter.Write(2);
                statusWriter.Write(1);
                mediaWritten = true;
            },
            (ledWriter, states) =>
            {
                Assert.Equal(1, diagnosticCount);
                core.Events.Add("led");
                EmulationSdk.HostProtocol.WriteLedStates(ledWriter, states);
            },
            () =>
            {
                Assert.True(mediaWritten);
                Assert.Equal(0, diagnosticCount);
                core.Events.Add("json");
                optionsRead++;
                return JsonOptions;
            }, ref sequence, ref diagnosticCount);

        Assert.Equal(["input", "set-input", "buffer", "run", "video", "media", "json", "led"], core.Events);
        Assert.Equal(7, sequence);
        Assert.Equal(1, diagnosticCount);
        Assert.Equal(1, optionsRead);
        Assert.False(core.Disposed);
        writer.Flush();
        output.Position = 0;
        using var response = new BinaryReader(output, Encoding.UTF8, leaveOpen: true);
        Assert.True(response.ReadBoolean());
        Assert.Equal(7, response.ReadInt64());
        var audio = Assert.Single(EmulationSdk.HostProtocol.ReadAudio(response, "test"));
        Assert.Equal(3, audio.Sequence);
        Assert.Equal(50d, response.ReadDouble());
        Assert.Equal(44100, response.ReadInt32());
        Assert.Equal(35, response.ReadInt32());
        Assert.Equal(1, response.ReadInt32());
        Assert.Equal(1, response.ReadInt32());
        Assert.Equal(11, response.ReadInt64());
        Assert.Equal(12, response.ReadInt64());
        Assert.Equal(2, response.ReadInt32());
        Assert.Equal(1, response.ReadInt32());
        Assert.True(response.ReadBoolean());
        Assert.Equal("[\"ready\"]", response.ReadString());
        Assert.True(EmulationSdk.HostProtocol.ReadLedStates(response, "test")[1]);
        Assert.Equal(output.Length, output.Position);
        Assert.True(reader.BaseStream.CanRead);
        Assert.True(writer.BaseStream.CanWrite);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnchangedDiagnosticsDoNotReadOptionsAndRepeatedOrMissingVideoUsesNull(bool repeated)
    {
        using var core = new Core();
        core.Frame = repeated ? Frame(6) : null;
        core.OnRun = () => core.DiagnosticItems[0] = "same count, changed text";
        using var input = AudioRequest();
        using var reader = new BinaryReader(input);
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output);
        long sequence = 6;
        var diagnosticCount = 1;
        var frameWrites = 0;
        EmulationSdk.HostProtocol.RunCoreFrame<Core, object, string>(
            reader, writer, core, _ => EmulationInputSnapshot.Empty,
            (_, frame) => { Assert.Null(frame); frameWrites++; },
            static (_, _) => { }, static (_, _) => { },
            () => throw new InvalidOperationException("Unchanged diagnostics must not access JSON options."),
            ref sequence, ref diagnosticCount);
        Assert.Equal(1, frameWrites);
        Assert.Equal(6, sequence);
        Assert.Equal(1, diagnosticCount);
        Assert.False(core.Disposed);
    }

    [Theory]
    [InlineData("run")]
    [InlineData("video")]
    [InlineData("media")]
    [InlineData("json")]
    [InlineData("led")]
    public void FailuresPreservePartialCountersAndBorrowedResources(string stage)
    {
        using var core = new Core();
        var expected = new InvalidOperationException(stage);
        core.OnRun = () => { if (stage == "run") throw expected; };
        using var input = AudioRequest();
        using var reader = new BinaryReader(input);
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output);
        long sequence = 6;
        var diagnosticCount = 0;
        var actual = Assert.Throws<InvalidOperationException>(() =>
            EmulationSdk.HostProtocol.RunCoreFrame<Core, object, string>(
                reader, writer, core, _ => EmulationInputSnapshot.Empty,
                (_, _) => { if (stage == "video") throw expected; },
                staticWriter(stage, expected), ledWriter(stage, expected),
                () => stage == "json" ? throw expected : JsonOptions,
                ref sequence, ref diagnosticCount));
        Assert.Same(expected, actual);
        Assert.Equal(stage is "run" or "video" ? 6 : 7, sequence);
        Assert.Equal(stage == "led" ? 1 : 0, diagnosticCount);
        Assert.Equal(stage is "run" or "video" ? 1 : 0, core.Audio.Count);
        Assert.False(core.Disposed);
        Assert.True(reader.BaseStream.CanRead);
        Assert.True(writer.BaseStream.CanWrite);
        if (stage == "run") Assert.Equal(0, output.Length);
        else Assert.True(output.Length > 0);
        writer.Write((byte)42);
    }

    private static Action<BinaryWriter, Core> staticWriter(string stage, Exception error)
        => (_, _) => { if (stage == "media") throw error; };

    private static Action<BinaryWriter, IReadOnlyDictionary<int, bool>> ledWriter(string stage, Exception error)
        => (_, _) => { if (stage == "led") throw error; };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static VideoFrame Frame(long sequence)
        => new(ReadOnlyMemory<byte>.Empty, 0, 0, 0, EmulationPixelFormat.Xrgb8888, 1, sequence, TimeSpan.Zero);

    private static MemoryStream AudioRequest()
    {
        var stream = new MemoryStream();
        try
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
                EmulationSdk.HostProtocol.WriteAudioBufferStatus(writer, new AudioOutputBufferStatus(44100, 8, 16, true));
            stream.Position = 0;
            return stream;
        }
        catch { stream.Dispose(); throw; }
    }
    private sealed class Core : IEmulationCoreRuntime<object, string>, IEmulationCoreRuntimeStatus, IEmulationAudioBufferFeedback
    {
        public int MinimumAudioLatencyMilliseconds => 35;
        public void SetAudioBufferStatus(AudioOutputBufferStatus? status)
        {
            Assert.Equal(new AudioOutputBufferStatus(44100, 8, 16, true), status);
            Events.Add("buffer");
        }
        public VideoFrame? Frame = LegacyHostFrameResponseTests.Frame(7);
        public readonly Queue<AudioChunk?> Audio = new(
            [new AudioChunk(ReadOnlyMemory<short>.Empty, 44100, 0, 3, TimeSpan.Zero)]);
        public readonly List<string> Events = new();
        public readonly List<string> DiagnosticItems = ["ready"];
        public Action? OnRun;
        public bool Disposed;
        public VideoFrame? LatestVideoFrame => Frame;
        public AudioChunk? LatestAudioChunk => null;
        public bool TryDequeueAudio(out AudioChunk? chunk) => Audio.TryDequeue(out chunk);
        public IReadOnlyList<string> Options => [];
        public IReadOnlyList<string> Diagnostics => DiagnosticItems;
        public IReadOnlyDictionary<int, bool> LedStates { get; } = new Dictionary<int, bool> { [1] = true };
        public string CoreName => "test";
        public string CoreVersion => "test";
        public IReadOnlySet<string> SupportedContentExtensions { get; } = new HashSet<string>();
        public string CoreSha256 => "test";
        public bool SupportsSaveStates => true;
        public EmulationVideoStandard? Region => EmulationVideoStandard.Pal;
        public int BufferedAudioFrames => Audio.Count;
        public long AudioOverrunCount => 11;
        public long AudioUnderrunCount => 12;
        public double FramesPerSecond => 50;
        public int SampleRate => 44100;
        public void Initialize(object configuration, string sessionDirectory, string? saveDirectory = null) { }
        public void RunFrame() { Events.Add("run"); OnRun?.Invoke(); }
        public void SetInput(EmulationInputSnapshot snapshot)
        {
            Assert.Same(EmulationInputSnapshot.Empty, snapshot);
            Events.Add("set-input");
        }
        public void Dispose() => Disposed = true;
    }
}