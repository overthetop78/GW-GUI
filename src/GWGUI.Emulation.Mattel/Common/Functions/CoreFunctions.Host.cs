using System.IO;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using System.Runtime.Versioning;
using System.Text.Json;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Mattel.Common.Functions;

internal static class CoreHostFunctions
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    internal static string CreatePipeName() => CoreHostConstants.PipePrefix + Guid.NewGuid().ToString(CoreHostConstants.UniqueNameFormat);
    internal static string CreateVideoMapName() => CoreHostConstants.VideoMapPrefix + Guid.NewGuid().ToString(CoreHostConstants.UniqueNameFormat);

    internal static void WriteRequestHeader(BinaryWriter writer, HostCommand command)
    {
        writer.Write(CoreHostConstants.ProtocolVersion);
        writer.Write((byte)command);
    }

    internal static HostCommand ReadRequestHeader(BinaryReader reader)
    {
        ValidateProtocolVersion(reader.ReadInt32());
        return (HostCommand)reader.ReadByte();
    }

    internal static void WriteResponseHeader(BinaryWriter writer, HostResponseStatus status)
    {
        writer.Write(CoreHostConstants.ProtocolVersion);
        writer.Write((byte)status);
    }

    internal static HostResponseStatus ReadResponseHeader(BinaryReader reader)
    {
        ValidateProtocolVersion(reader.ReadInt32());
        return (HostResponseStatus)reader.ReadByte();
    }

    internal static void ValidateProtocolVersion(int version)
    {
        if (version == CoreHostConstants.ProtocolVersion) return;
        throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, CoreHostErrors.ProtocolVersionMismatchFormat, version, CoreHostConstants.ProtocolVersion));
    }

    internal static HostError CreateError(Exception error) => error is EmulationException structured
        ? new HostError(error.GetType().FullName ?? error.GetType().Name, error.Message, structured.Category, structured.Code, structured.Context, structured.IsLocalized)
        : new HostError(error.GetType().FullName ?? error.GetType().Name, error.Message, null, null, new Dictionary<string, string>(), false);

    internal static void WriteError(BinaryWriter writer, Exception error) => writer.Write(JsonSerializer.Serialize(CreateError(error), JsonOptions));

    internal static HostError ReadError(BinaryReader reader) => JsonSerializer.Deserialize<HostError>(reader.ReadString(), JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.CommunicationFailed);

    internal static void WriteString(BinaryWriter writer, string? value) => EmulationHostProtocolFunctions.WriteString(writer, value);
    internal static string? ReadString(BinaryReader reader) => EmulationHostProtocolFunctions.ReadString(reader);
    internal static void WriteBytes(BinaryWriter writer, ReadOnlySpan<byte> value) => EmulationHostProtocolFunctions.WriteBytes(writer, value);
    internal static byte[] ReadBytes(BinaryReader reader) => EmulationHostProtocolFunctions.ReadBytes(reader, CoreHostConstants.HostName);
    internal static void WriteInput(BinaryWriter writer, EmulationInputSnapshot input) => EmulationHostProtocolFunctions.WriteInput(writer, input);
    internal static EmulationInputSnapshot ReadInput(BinaryReader reader) => EmulationHostProtocolFunctions.ReadInput(reader, CoreHostConstants.HostName);
    internal static int CalculateVideoSlotCapacity(int frameLength) => EmulationHostProtocolFunctions.CalculateVideoSlotCapacity(
        frameLength, CoreHostConstants.MinimumVideoSlotCapacity, CoreHostConstants.HostName);

    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static void WriteResizableSharedFrame(BinaryWriter writer, VideoFrame? frame, SharedVideoWriter video) =>
        EmulationHostProtocolFunctions.WriteResizableSharedFrame(writer, frame, video);

    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static VideoFrame? ReadResizableSharedFrame(BinaryReader reader, ref MemoryMappedFile? memory, ref MemoryMappedViewAccessor? view, ref string? activeName) =>
        EmulationHostProtocolFunctions.ReadResizableSharedFrame(reader, ref memory, ref view, ref activeName, CoreHostConstants.MinimumVideoSlotCapacity);
    internal static void WriteAudio(BinaryWriter writer, IReadOnlyList<AudioChunk> chunks) => EmulationHostProtocolFunctions.WriteAudio(writer, chunks);
    internal static IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader) => EmulationHostProtocolFunctions.ReadAudio(reader, CoreHostConstants.HostName);
    internal static void WriteLedStates(BinaryWriter writer, IReadOnlyDictionary<int, bool> states) => EmulationHostProtocolFunctions.WriteLedStates(writer, states);
    internal static IReadOnlyDictionary<int, bool> ReadLedStates(BinaryReader reader) => EmulationHostProtocolFunctions.ReadLedStates(reader, CoreHostConstants.HostName);

    internal static void WriteDiskStatus(BinaryWriter writer, DiskStatus status) => writer.Write(JsonSerializer.Serialize(status, JsonOptions));

    internal static DiskStatus ReadDiskStatus(BinaryReader reader) => JsonSerializer.Deserialize<DiskStatus>(reader.ReadString(), JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.CommunicationFailed);

    internal static void DisposeTransport(IDisposable? resource)
    {
        if (resource is null) return;
        try
        {
            resource.Dispose();
        }
        catch (IOException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }
}
