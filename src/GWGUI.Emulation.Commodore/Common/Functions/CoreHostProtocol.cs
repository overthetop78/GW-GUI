using CommonCoreHostFunctions = GWGUI.Emulation.Commodore.Common.Functions.CoreHostFunctions;
using System.IO;

using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Commodore.Common.Functions;


internal static class CoreHostProtocol
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    internal static void WriteString(BinaryWriter writer, string? value) => CommonCoreHostFunctions.Protocol.WriteString(writer, value);
    internal static string? ReadString(BinaryReader reader) => CommonCoreHostFunctions.Protocol.ReadString(reader);
    internal static void WriteBytes(BinaryWriter writer, ReadOnlySpan<byte> bytes) => CommonCoreHostFunctions.Protocol.WriteBytes(writer, bytes);
    internal static byte[] ReadBytes(BinaryReader reader) => CommonCoreHostFunctions.Protocol.ReadBytes(reader, ProcessCoreConstants.TransportName);
    internal static void WriteInput(BinaryWriter writer, EmulationInputSnapshot input) => CommonCoreHostFunctions.Protocol.WriteInput(writer, input);
    internal static EmulationInputSnapshot ReadInput(BinaryReader reader) => CommonCoreHostFunctions.Protocol.ReadInput(reader, ProcessCoreConstants.TransportName);
    internal static void WriteSharedFrame(BinaryWriter writer, VideoFrame? frame, MemoryMappedViewAccessor videoMap) => CommonCoreHostFunctions.Protocol.WriteSharedFrame(writer, frame, videoMap, ProcessCoreConstants.TransportName);
    internal static VideoFrame? ReadSharedFrame(BinaryReader reader, MemoryMappedViewAccessor videoMap) => CommonCoreHostFunctions.Protocol.ReadSharedFrame(reader, videoMap, ProcessCoreConstants.TransportName);
    internal static IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader) => CommonCoreHostFunctions.Protocol.ReadAudio(reader, ProcessCoreConstants.TransportName);
    internal static void WriteLedStates(BinaryWriter writer, IReadOnlyDictionary<int, bool> states) => CommonCoreHostFunctions.Protocol.WriteLedStates(writer, states);
    internal static IReadOnlyDictionary<int, bool> ReadLedStates(BinaryReader reader) => CommonCoreHostFunctions.Protocol.ReadLedStates(reader, ProcessCoreConstants.TransportName);
}
