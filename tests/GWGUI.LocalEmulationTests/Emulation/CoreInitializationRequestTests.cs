using GWGUI.Emulation.Enums;
using GWGUI.Emulation;
using System.IO;
using System.Text;
using System.Text.Json;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class CoreInitializationRequestTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("sauvegardes")]
    public void RequestKeepsFieldOrderNullablePathAndBorrowedWriter(string? saveDirectory)
    {
        WithWriter((stream, writer) =>
        {
            EmulationSdk.HostProtocol.WriteInitializationRequest(writer, "core.dll", 17,
                "session", saveDirectory, new Configuration("Cœur", 3), new(JsonSerializerDefaults.Web));
            writer.Write("writer still open");
            Inspect(stream, reader =>
            {
                Assert.Equal("core.dll", reader.ReadString());
                Assert.Equal(17, reader.ReadInt32());
                Assert.Equal(Path.GetFullPath("session"), reader.ReadString());
                Assert.Equal(saveDirectory is not null, reader.ReadBoolean());
                if (saveDirectory is not null)
                    Assert.Equal(Path.GetFullPath(saveDirectory), reader.ReadString());
                var document = JsonDocument.Parse(reader.ReadString());
                try
                {
                    Assert.Equal("Cœur", document.RootElement.GetProperty("name").GetString());
                    Assert.Equal(3, document.RootElement.GetProperty("count").GetInt32());
                }
                finally { document.Dispose(); }
                Assert.Equal("writer still open", reader.ReadString());
                Assert.Equal(stream.Length, stream.Position);
            });
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InvalidPathPreservesThePrefixWrittenBeforeIt(bool invalidSavePath)
    {
        WithWriter((stream, writer) =>
        {
            Assert.Throws<ArgumentException>(() =>
                EmulationSdk.HostProtocol.WriteInitializationRequest(writer, "core.dll", 8,
                    invalidSavePath ? "session" : "\0", invalidSavePath ? "\0" : null,
                    new Configuration("unused", 0), new(JsonSerializerDefaults.Web)));
            Inspect(stream, reader =>
            {
                Assert.Equal("core.dll", reader.ReadString());
                Assert.Equal(8, reader.ReadInt32());
                if (invalidSavePath)
                    Assert.Equal(Path.GetFullPath("session"), reader.ReadString());
                Assert.Equal(stream.Length, stream.Position);
            });
        });
    }

    [Fact]
    public void SerializationFailureOccursAfterThePathFields()
    {
        WithWriter((stream, writer) =>
        {
            Action unsupportedConfiguration = static () => { };
            Assert.Throws<NotSupportedException>(() =>
                EmulationSdk.HostProtocol.WriteInitializationRequest(writer, "core.dll", 4,
                    "session", null, unsupportedConfiguration, new(JsonSerializerDefaults.Web)));
            Inspect(stream, reader =>
            {
                Assert.Equal("core.dll", reader.ReadString());
                Assert.Equal(4, reader.ReadInt32());
                Assert.Equal(Path.GetFullPath("session"), reader.ReadString());
                Assert.False(reader.ReadBoolean());
                Assert.Equal(stream.Length, stream.Position);
            });
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("sauvegardes")]
    public void ReadingKeepsCodecOrderNullableSaveAndBorrowedReader(string? saves)
    {
        WithWriter((stream, writer) =>
        {
            EmulationSdk.HostProtocol.WriteInitializationRequest(writer, "core.dll", 17,
                "session", saves, new Configuration("Cœur", 3), new(JsonSerializerDefaults.Web));
            writer.Write("reader still open");
            writer.Flush();
            Inspect(stream, reader =>
            {
                var calls = new List<string>();
                var request = EmulationSdk.HostProtocol.ReadInitializationRequest(reader,
                    input => { calls.Add("emulator"); return input.ReadInt32(); },
                    input => { calls.Add("saves"); return EmulationSdk.HostProtocol.ReadString(input); },
                    input =>
                    {
                        calls.Add("configuration");
                        return JsonSerializer.Deserialize<Configuration>(input.ReadString(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                    });
                Assert.Equal(new[] { "emulator", "saves", "configuration" }, calls);
                Assert.Equal("core.dll", request.CorePath);
                Assert.Equal(17, request.Emulator);
                Assert.Equal(Path.GetFullPath("session"), request.Session);
                Assert.Equal(saves is null ? null : Path.GetFullPath(saves), request.Saves);
                Assert.Equal(new Configuration("Cœur", 3), request.Configuration);
                Assert.Equal("reader still open", reader.ReadString());
                Assert.Equal(stream.Length, stream.Position);
            });
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedCodecStopsAtItsOriginalFieldAndPreservesTheError(bool failOnConfiguration)
    {
        WithWriter((stream, writer) =>
        {
            EmulationSdk.HostProtocol.WriteInitializationRequest(writer, "core.dll", 17,
                "session", null, new Configuration("unused", 0), new(JsonSerializerDefaults.Web));
            writer.Flush();
            Inspect(stream, reader =>
            {
                var error = new InvalidOperationException("codec failure");
                var savesCalled = false;
                var configurationCalled = false;
                Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
                    EmulationSdk.HostProtocol.ReadInitializationRequest<int, Configuration>(reader,
                        input => failOnConfiguration ? input.ReadInt32() : throw error,
                        input => { savesCalled = true; return EmulationSdk.HostProtocol.ReadString(input); },
                        _ => { configurationCalled = true; throw error; })));
                Assert.Equal(failOnConfiguration, savesCalled);
                Assert.Equal(failOnConfiguration, configurationCalled);
                if (failOnConfiguration)
                    Assert.Contains("unused", reader.ReadString());
                else
                    Assert.Equal(17, reader.ReadInt32());
            });
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("saves")]
    public void HistoricalReaderKeepsFourFieldsAndLeavesTheReaderOpen(string? saves)
    {
        WithWriter((stream, writer) =>
        {
            writer.Write("core.dll");
            writer.Write("legacy-session");
            EmulationSdk.HostProtocol.WriteString(writer, saves);
            writer.Write(JsonSerializer.Serialize(new Configuration("legacy", 4)));
            writer.Write("remaining");
            writer.Flush();
            Inspect(stream, reader =>
            {
                var calls = new List<string>();
                var request = EmulationSdk.HostProtocol.ReadInitializationRequest(reader,
                    input => { calls.Add("saves"); return EmulationSdk.HostProtocol.ReadString(input); },
                    input => { calls.Add("configuration"); return JsonSerializer.Deserialize<Configuration>(input.ReadString())!; });
                Assert.Equal(new[] { "saves", "configuration" }, calls);
                Assert.Equal(("core.dll", "legacy-session", saves, new Configuration("legacy", 4)), request);
                Assert.Equal("remaining", reader.ReadString());
            });
        });
    }

    [Fact]
    public void HistoricalReaderPreservesTheConfigurationErrorAndItsPosition()
    {
        WithWriter((stream, writer) =>
        {
            writer.Write("core.dll");
            writer.Write("legacy-session");
            writer.Write(false);
            writer.Write("null");
            writer.Write("remaining");
            writer.Flush();
            Inspect(stream, reader =>
            {
                var error = new InvalidDataException("invalid configuration");
                Assert.Same(error, Assert.Throws<InvalidDataException>(() =>
                    EmulationSdk.HostProtocol.ReadInitializationRequest(reader,
                        EmulationSdk.HostProtocol.ReadString,
                        input => JsonSerializer.Deserialize<Configuration>(input.ReadString()) ?? throw error)));
                Assert.Equal("remaining", reader.ReadString());
            });
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("saves")]
    public void HistoricalWriterKeepsFourFieldsAndDefersItsCodecs(string? saves)
    {
        WithWriter((stream, writer) =>
        {
            BinaryWriter? slot = writer;
            var calls = new List<string>();
            EmulationSdk.HostProtocol.WriteInitializationRequest(ref slot, "core.dll",
                () => { Assert.True(stream.Position > 0); calls.Add("session"); return "legacy-session"; },
                () => { calls.Add("saves"); return saves; },
                (output, value) => { calls.Add("save-codec"); EmulationSdk.HostProtocol.WriteString(output, value); },
                () => { calls.Add("configuration"); return JsonSerializer.Serialize(new Configuration("legacy", 4)); });
            Assert.Same(writer, slot);
            Assert.Equal(new[] { "session", "saves", "save-codec", "configuration" }, calls);
            writer.Write("remaining");
            writer.Flush();
            Inspect(stream, reader =>
            {
                Assert.Equal("core.dll", reader.ReadString());
                Assert.Equal("legacy-session", reader.ReadString());
                Assert.Equal(saves, EmulationSdk.HostProtocol.ReadString(reader));
                Assert.Equal(new Configuration("legacy", 4), JsonSerializer.Deserialize<Configuration>(reader.ReadString()));
                Assert.Equal("remaining", reader.ReadString());
            });
        });
    }

    [Fact]
    public void HistoricalWriterReadsTheReentrantWriterSlotForEachOperation()
    {
        using var first = new RecordingWriter();
        using var second = new RecordingWriter();
        BinaryWriter? slot = first;
        first.AfterWrite = () => slot = second;
        EmulationSdk.HostProtocol.WriteInitializationRequest(ref slot, "core.dll",
            () => "session", () => "saves", (output, value) => output.Write(value!), () => "json");
        Assert.Equal(new[] { "core.dll" }, first.Values);
        Assert.Equal(new[] { "session", "saves", "json" }, second.Values);
        Assert.Same(second, slot);
    }

    [Fact]
    public void HistoricalWriterPreservesThePrefixAndBorrowedSlotWhenSerializationFails()
    {
        WithWriter((stream, writer) =>
        {
            BinaryWriter? slot = writer;
            var error = new InvalidOperationException("serialization failure");
            Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
                EmulationSdk.HostProtocol.WriteInitializationRequest(ref slot, "core.dll",
                    () => "session", () => null, EmulationSdk.HostProtocol.WriteString, () => throw error)));
            Assert.Same(writer, slot);
            writer.Write("remaining");
            writer.Flush();
            Inspect(stream, reader =>
            {
                Assert.Equal("core.dll", reader.ReadString());
                Assert.Equal("session", reader.ReadString());
                Assert.False(reader.ReadBoolean());
                Assert.Equal("remaining", reader.ReadString());
            });
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("saves")]
    public void HostInitializationPreservesTheResponseAndTransfersTheCoreToItsSession(string? saves)
        => VerifyHostInitialization(null, saves);

    [Theory]
    [InlineData("dispose")]
    [InlineData("emulator")]
    [InlineData("saves")]
    [InlineData("configuration")]
    [InlineData("factory")]
    [InlineData("initialize")]
    [InlineData("json")]
    [InlineData("status")]
    [InlineData("led")]
    public void HostInitializationFailureKeepsTheOriginalOrderErrorAndReachableOwner(string failure)
        => VerifyHostInitialization(failure, null);

    private static void VerifyHostInitialization(string? failure, string? saves)
    {
        var stages = new[] { "dispose", "emulator", "saves", "configuration", "factory", "initialize", "json", "status", "led" };
        var calls = new List<string>();
        var expected = new InvalidOperationException("initialization failure");
        void Step(string stage)
        {
            calls.Add(stage);
            if (stage == failure) throw expected;
        }

        var previous = new InitializationCore { OnDispose = () => Step("dispose") };
        var created = new InitializationCore();
        InitializationCore? slot = previous;
        var diagnosticCount = 9;
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        using var input = new MemoryStream();
        using var request = new BinaryWriter(input, Encoding.UTF8, leaveOpen: true);
        request.Write("core.dll");
        request.Write(17);
        request.Write("session");
        EmulationSdk.HostProtocol.WriteString(request, saves);
        request.Write(JsonSerializer.Serialize(new Configuration("configured", 3), options));
        request.Write("remaining");
        request.Flush();
        input.Position = 0;
        using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);
        using var output = new MemoryStream();
        using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);
        created.OnInitialize = (configuration, session, saveDirectory) =>
        {
            Assert.Same(created, slot);
            Assert.Equal(new Configuration("configured", 3), configuration);
            Assert.Equal("session", session);
            Assert.Equal(saves, saveDirectory);
            Step("initialize");
        };

        try
        {
            void Initialize() => EmulationSdk.HostProtocol.InitializeCore<InitializationCore, int, Configuration, string>(
                reader, writer,
                (path, emulator) =>
                {
                    Assert.Null(slot);
                    Assert.Equal("core.dll", path);
                    Assert.Equal(17, emulator);
                    Step("factory");
                    return created;
                }, ref slot, ref diagnosticCount,
                value => { Assert.Null(slot); Step("emulator"); return value.ReadInt32(); },
                value => { Step("saves"); return EmulationSdk.HostProtocol.ReadString(value); },
                value => { Step("configuration"); return JsonSerializer.Deserialize<Configuration>(value.ReadString(), options)!; },
                () => { Step("json"); return options; }, '|',
                (response, core) =>
                {
                    Assert.Same(created, core);
                    Assert.Equal(9, diagnosticCount);
                    Step("status");
                    response.Write(23);
                },
                (response, states) =>
                {
                    Assert.Equal(1, diagnosticCount);
                    Step("led");
                    EmulationSdk.HostProtocol.WriteLedStates(response, states);
                });

            if (failure is null) Initialize();
            else Assert.Same(expected, Assert.Throws<InvalidOperationException>(Initialize));
            var lastStage = failure is null ? stages.Length : Array.IndexOf(stages, failure) + 1;
            Assert.Equal(stages.Take(lastStage), calls);
            Assert.Equal(1, previous.DisposeCount);
            Assert.Equal(0, created.DisposeCount);
            var transferred = failure is null || Array.IndexOf(stages, failure) >= Array.IndexOf(stages, "initialize");
            if (transferred) Assert.Same(created, slot);
            else Assert.Null(slot);
            Assert.Equal(failure is null or "led" ? 1 : 9, diagnosticCount);
            if (failure is null)
            {
                Assert.Equal("remaining", reader.ReadString());
                writer.Flush();
                output.Position = 0;
                using var response = new BinaryReader(output, Encoding.UTF8, leaveOpen: true);
                Assert.Equal("hash", response.ReadString());
                Assert.Equal(50d, response.ReadDouble());
                Assert.Equal(44100, response.ReadInt32());
                Assert.Equal(23, response.ReadInt32());
                Assert.Equal("[\"option\"]", response.ReadString());
                Assert.Equal("[\"ready\"]", response.ReadString());
                Assert.Equal("core", response.ReadString());
                Assert.Equal("1", response.ReadString());
                Assert.Equal("a|b", response.ReadString());
                Assert.True(EmulationSdk.HostProtocol.ReadLedStates(response, "test")[1]);
                Assert.Equal(output.Length, output.Position);
            }
            Assert.True(input.CanRead);
            Assert.True(output.CanWrite);
        }
        finally
        {
            try { EmulationSdk.HostProtocol.DisposePreviousCore(ref slot); }
            finally
            {
                try { if (previous.DisposeCount == 0) previous.Dispose(); }
                finally { if (created.DisposeCount == 0) created.Dispose(); }
            }
        }
        Assert.Equal(1, created.DisposeCount);
    }

    private sealed class InitializationCore : IEmulationCoreRuntime<Configuration, string>
    {
        public int DiskCount => 0;
        public int CurrentDiskIndex => -1;
        public bool SupportsPointerCapture => true;

        public bool ShutdownRequested => false;
        public bool SupportsSaveStates => true;
        public EmulationVideoStandard? Region => null;
        public int BufferedAudioFrames => 0;
        public long AudioOverrunCount => 0;
        public long AudioUnderrunCount => 0;
        public HostProcessState HostProcessState => HostProcessState.InProcess;
        public int? HostProcessId => null;
        public int MinimumAudioLatencyMilliseconds => 0;
        public void SetAudioBufferStatus(AudioOutputBufferStatus? status) { }

        internal Action? OnDispose { get; init; }
        internal Action<Configuration, string, string?>? OnInitialize { get; set; }
        internal int DisposeCount { get; private set; }
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public bool TryDequeueAudio(out AudioChunk? chunk) { chunk = null; return false; }
        public IReadOnlyList<string> Options => ["option"];
        public IReadOnlyList<string> Diagnostics => ["ready"];
        public IReadOnlyDictionary<int, bool> LedStates { get; } = new Dictionary<int, bool> { [1] = true };
        public string CoreName => "core";
        public string CoreVersion => "1";
        public IReadOnlySet<string> SupportedContentExtensions { get; } = new HashSet<string> { "b", "a" };
        public string CoreSha256 => "hash";
        public double FramesPerSecond => 50;
        public int SampleRate => 44100;
        public void Initialize(Configuration configuration, string sessionDirectory, string? saveDirectory = null)
            => OnInitialize?.Invoke(configuration, sessionDirectory, saveDirectory);
        public void RunFrame() => throw new NotSupportedException();
        public void SetInput(EmulationInputSnapshot snapshot) => throw new NotSupportedException();
        public void Dispose() { DisposeCount++; OnDispose?.Invoke(); }
    }

    private sealed class RecordingWriter : BinaryWriter
    {
        internal RecordingWriter() : base(Stream.Null, Encoding.UTF8, leaveOpen: true) { }
        internal List<string> Values { get; } = [];
        internal Action? AfterWrite { get; set; }

        public override void Write(string value)
        {
            Values.Add(value);
            AfterWrite?.Invoke();
        }
    }

    private static void WithWriter(Action<MemoryStream, BinaryWriter> verify)
    {
        var stream = new MemoryStream();
        BinaryWriter? writer = null;
        try
        {
            writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
            verify(stream, writer);
        }
        finally
        {
            try { writer?.Dispose(); }
            finally { stream.Dispose(); }
        }
    }

    private static void Inspect(MemoryStream stream, Action<BinaryReader> verify)
    {
        stream.Position = 0;
        var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        try { verify(reader); }
        finally { reader.Dispose(); }
    }

    private sealed record Configuration(string Name, int Count);
}
