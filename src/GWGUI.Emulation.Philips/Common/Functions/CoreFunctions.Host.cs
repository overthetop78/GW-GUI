using GWGUI.Emulation.Interfaces;
using System.IO;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using System.Runtime.Versioning;
using System.Text.Json;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Philips.Common.Functions;

internal static class CoreHostFunctions
{
    /// <summary>Fournit le contrat commun de transport, de statut et de durée de vie des hôtes.</summary>
    /// <value>Service emprunté du SDK, indépendant du nom du module et de son adaptateur.</value>
    /// <remarks>Les adaptateurs passent par cet accès Common pour appeler les codecs et les opérations de session.
    /// Le service expose uniquement son interface publique ; il conserve les implémentations et limites du protocole
    /// dans Emulation. Les flux, vues, processus et cœurs restent possédés par la session appelante, qui les ferme
    /// dans son finally et attend l’arrêt effectif du cœur et de son hôte. Cet accès ne crée aucune session,
    /// ne mémorise aucune ressource reçue et ne requiert aucun enregistrement du nom de module dans le SDK.
    /// Les codecs propres au module conservent leur configuration et leurs diagnostics.</remarks>
    internal static IEmulationHostProtocol Protocol => EmulationSdk.HostProtocol;

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
    internal static string? ReadString(BinaryReader reader) => CoreHostFunctions.Protocol.ReadString(reader);
    internal static void WriteBytes(BinaryWriter writer, ReadOnlySpan<byte> value) => CoreHostFunctions.Protocol.WriteBytes(writer, value);
    internal static byte[] ReadBytes(BinaryReader reader) => CoreHostFunctions.Protocol.ReadBytes(reader, CoreHostConstants.HostName);
    internal static void WriteInput(BinaryWriter writer, EmulationInputSnapshot input) => CoreHostFunctions.Protocol.WriteInput(writer, input);
    internal static EmulationInputSnapshot ReadInput(BinaryReader reader) => CoreHostFunctions.Protocol.ReadInput(reader, CoreHostConstants.HostName);
    internal static int CalculateVideoSlotCapacity(int frameLength) => CoreHostFunctions.Protocol.CalculateVideoSlotCapacity(
        frameLength, CoreHostConstants.MinimumVideoSlotCapacity, CoreHostConstants.HostName);

    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static void WriteResizableSharedFrame(BinaryWriter writer, VideoFrame? frame, SharedVideoWriter video) =>
        CoreHostFunctions.Protocol.WriteResizableSharedFrame(writer, frame, video);

    [SupportedOSPlatform(CoreHostConstants.Windows)]
    internal static VideoFrame? ReadResizableSharedFrame(BinaryReader reader, ref MemoryMappedFile? memory, ref MemoryMappedViewAccessor? view, ref string? activeName) =>
        CoreHostFunctions.Protocol.ReadResizableSharedFrame(reader, ref memory, ref view, ref activeName, CoreHostConstants.MinimumVideoSlotCapacity);
    internal static IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader) => CoreHostFunctions.Protocol.ReadAudio(reader, CoreHostConstants.HostName);
    internal static void WriteLedStates(BinaryWriter writer, IReadOnlyDictionary<int, bool> states) => CoreHostFunctions.Protocol.WriteLedStates(writer, states);
    internal static IReadOnlyDictionary<int, bool> ReadLedStates(BinaryReader reader) => CoreHostFunctions.Protocol.ReadLedStates(reader, CoreHostConstants.HostName);

    internal static void WriteDiskStatus(BinaryWriter writer, DiskStatus status) => writer.Write(JsonSerializer.Serialize(status, JsonOptions));

    internal static DiskStatus ReadDiskStatus(BinaryReader reader) => JsonSerializer.Deserialize<DiskStatus>(reader.ReadString(), JsonOptions) ?? throw new InvalidDataException(CoreHostErrors.CommunicationFailed);

}
