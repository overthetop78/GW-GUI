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
