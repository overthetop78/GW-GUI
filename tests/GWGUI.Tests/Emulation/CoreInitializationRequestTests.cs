using System.IO;
using System.Text;
using System.Text.Json;
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
            EmulationHostProtocolFunctions.WriteInitializationRequest(writer, "core.dll", 17,
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
                EmulationHostProtocolFunctions.WriteInitializationRequest(writer, "core.dll", 8,
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
                EmulationHostProtocolFunctions.WriteInitializationRequest(writer, "core.dll", 4,
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
            EmulationHostProtocolFunctions.WriteInitializationRequest(writer, "core.dll", 17,
                "session", saves, new Configuration("Cœur", 3), new(JsonSerializerDefaults.Web));
            writer.Write("reader still open");
            writer.Flush();
            Inspect(stream, reader =>
            {
                var calls = new List<string>();
                var request = EmulationHostProtocolFunctions.ReadInitializationRequest(reader,
                    input => { calls.Add("emulator"); return input.ReadInt32(); },
                    input => { calls.Add("saves"); return EmulationHostProtocolFunctions.ReadString(input); },
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
            EmulationHostProtocolFunctions.WriteInitializationRequest(writer, "core.dll", 17,
                "session", null, new Configuration("unused", 0), new(JsonSerializerDefaults.Web));
            writer.Flush();
            Inspect(stream, reader =>
            {
                var error = new InvalidOperationException("codec failure");
                var savesCalled = false;
                var configurationCalled = false;
                Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
                    EmulationHostProtocolFunctions.ReadInitializationRequest<int, Configuration>(reader,
                        input => failOnConfiguration ? input.ReadInt32() : throw error,
                        input => { savesCalled = true; return EmulationHostProtocolFunctions.ReadString(input); },
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
            EmulationHostProtocolFunctions.WriteString(writer, saves);
            writer.Write(JsonSerializer.Serialize(new Configuration("legacy", 4)));
            writer.Write("remaining");
            writer.Flush();
            Inspect(stream, reader =>
            {
                var calls = new List<string>();
                var request = EmulationHostProtocolFunctions.ReadInitializationRequest(reader,
                    input => { calls.Add("saves"); return EmulationHostProtocolFunctions.ReadString(input); },
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
                    EmulationHostProtocolFunctions.ReadInitializationRequest(reader,
                        EmulationHostProtocolFunctions.ReadString,
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
            EmulationHostProtocolFunctions.WriteInitializationRequest(ref slot, "core.dll",
                () => { Assert.True(stream.Position > 0); calls.Add("session"); return "legacy-session"; },
                () => { calls.Add("saves"); return saves; },
                (output, value) => { calls.Add("save-codec"); EmulationHostProtocolFunctions.WriteString(output, value); },
                () => { calls.Add("configuration"); return JsonSerializer.Serialize(new Configuration("legacy", 4)); });
            Assert.Same(writer, slot);
            Assert.Equal(new[] { "session", "saves", "save-codec", "configuration" }, calls);
            writer.Write("remaining");
            writer.Flush();
            Inspect(stream, reader =>
            {
                Assert.Equal("core.dll", reader.ReadString());
                Assert.Equal("legacy-session", reader.ReadString());
                Assert.Equal(saves, EmulationHostProtocolFunctions.ReadString(reader));
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
        EmulationHostProtocolFunctions.WriteInitializationRequest(ref slot, "core.dll",
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
                EmulationHostProtocolFunctions.WriteInitializationRequest(ref slot, "core.dll",
                    () => "session", () => null, EmulationHostProtocolFunctions.WriteString, () => throw error)));
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
