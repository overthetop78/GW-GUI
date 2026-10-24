using CommonCoreHostFunctions = GWGUI.Emulation.Nintendo.Common.Functions.CoreHostFunctions;

using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;
using GWGUI.Emulation;

using System.IO;

namespace GWGUI.Emulation.Nintendo.Common.Functions;


internal static partial class CoreHostFunctions
{
    /// <summary>Écrit la version commune puis la commande ; les arguments suivent cet en-tête.</summary>
    internal static void WriteRequestHeader(BinaryWriter writer, HostCommand command) => Protocol.WriteHeader(writer, CoreHostConstants.ProtocolVersion, (byte)command);
    /// <summary>Valide la version avant de consommer la commande.</summary>
    internal static HostCommand ReadRequestHeader(BinaryReader reader) => (HostCommand)Protocol.ReadHeader(reader, ValidateProtocolVersion);
    /// <summary>Écrit la version commune puis le statut précédant le résultat ou le diagnostic.</summary>
    internal static void WriteResponseHeader(BinaryWriter writer, HostResponseStatus status) => Protocol.WriteHeader(writer, CoreHostConstants.ProtocolVersion, (byte)status);
    /// <summary>Valide la version avant de consommer le statut de réponse.</summary>
    internal static HostResponseStatus ReadResponseHeader(BinaryReader reader) => (HostResponseStatus)Protocol.ReadHeader(reader, ValidateProtocolVersion);
    /// <summary>Refuse un flux incompatible avant de lire ses données ; le message standard provient de .NET.</summary>
    internal static void ValidateProtocolVersion(int version)
    {
        if (version != CoreHostConstants.ProtocolVersion) throw new InvalidDataException();
    }

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


