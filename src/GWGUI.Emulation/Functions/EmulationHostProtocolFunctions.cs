using GWGUI.Emulation.Services;
using System.Text.Json;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace GWGUI.Emulation.Functions;

/// <summary>Codecs et sessions de transport communs à tous les modules indépendants.</summary>
/// <remarks>Les lectures et écritures empruntent les flux. Les opérations de destruction reçoivent uniquement les ressources dont l’appelant est propriétaire ; les sessions créées ici sont libérées dans leur portée.</remarks>
internal static partial class EmulationHostProtocolFunctions
{
    /// <summary>Écrit les mêmes six champs pour tous les diagnostics, sans posséder le flux emprunté.</summary>
    internal static void WriteError<TCategory, TCode>(BinaryWriter writer, HostError<TCategory, TCode> error, JsonSerializerOptions jsonOptions, IReadOnlyList<string> diagnostics) where TCategory : struct where TCode : struct
    {
        var message = EmulationErrorService.AppendNativeDiagnostics(error.Message, diagnostics);
        if (message != error.Message) error = error with { Message = message };
        writer.Write(JsonSerializer.Serialize(error, jsonOptions));
    }

    /// <summary>Lit le contrat complet et laisse Common choisir le diagnostic d’un document nul.</summary>
    internal static HostError<TCategory, TCode> ReadError<TCategory, TCode>(BinaryReader reader, JsonSerializerOptions jsonOptions, Func<Exception> invalidError) where TCategory : struct where TCode : struct
        => JsonSerializer.Deserialize<HostError<TCategory, TCode>>(reader.ReadString(), jsonOptions) ?? throw invalidError();

    /// <summary>Écrit le format commun : version Int32 suivie du code octet.</summary>
    internal static void WriteHeader(BinaryWriter writer, int version, byte code)
    {
        writer.Write(version);
        writer.Write(code);
    }

    /// <summary>Valide la version avant de consommer le code ; les flux restent empruntés.</summary>
    internal static byte ReadHeader(BinaryReader reader, Action<int> validateVersion)
    {
        validateVersion(reader.ReadInt32());
        return reader.ReadByte();
    }

    /// <summary>Construit une réponse isolée, remplace un résultat partiel par une erreur si nécessaire, puis écrit son enveloppe.</summary>
    internal static void WriteResponse(BinaryWriter transportWriter, Action<BinaryWriter> execute,
        Action<BinaryWriter, Exception> writeFailure)
    {
        using var responseStream = new MemoryStream();
        using var writer = new BinaryWriter(responseStream, Encoding.UTF8, leaveOpen: true);
        try
        {
            execute(writer);
        }
        catch (Exception error)
        {
            responseStream.SetLength(0);
            responseStream.Position = 0;
            writeFailure(writer, error);
        }
        writer.Flush();
        WriteBytes(transportWriter, responseStream.GetBuffer().AsSpan(0, checked((int)responseStream.Length)));
    }

    /// <summary>Sérialise une requête sous le verrou emprunté et signale les échecs de connexion selon la politique fournie.</summary>
    internal static void ExecuteRequest(SemaphoreSlim gate, Func<BinaryWriter> getWriter, Func<byte[]> getResponse,
        Action<BinaryWriter> write, Action<BinaryReader> read, Action<Exception> failConnection)
    {
        gate.Wait();
        try
        {
            var writer = getWriter();
            write(writer);
            writer.Flush();
            using var reader = new BinaryReader(new MemoryStream(getResponse(), writable: false), Encoding.UTF8, leaveOpen: false);
            read(reader);
        }
        catch (Exception error) when (error is IOException or EndOfStreamException or OperationCanceledException or InvalidDataException)
        {
            failConnection(error);
        }
        finally { gate.Release(); }
    }

    /// <summary>Lit la réponse bornée avec délai et annulation, sans prendre possession du flux de transport.</summary>
    internal static async Task<byte[]> ReadResponseAsync(Stream pipe, TimeSpan responseTimeout,
        CancellationToken cancellationToken, Func<int, string> invalidLength)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(responseTimeout);
        var header = new byte[sizeof(int)];
        await pipe.ReadExactlyAsync(header, timeout.Token).ConfigureAwait(false);
        var length = BinaryPrimitives.ReadInt32LittleEndian(header);
        if (length is < 0 or > EmulationHostProtocolConstants.MaximumBlobLength)
            throw new InvalidDataException(invalidLength(length));
        var response = GC.AllocateUninitializedArray<byte>(length);
        await pipe.ReadExactlyAsync(response, timeout.Token).ConfigureAwait(false);
        return response;
    }

    /// <summary>Écrit la présence puis la chaîne facultative dans le flux emprunté.</summary>
    internal static void WriteString(BinaryWriter writer, string? value)
    {
        writer.Write(value is not null);
        if (value is not null) writer.Write(value);
    }

    /// <summary>Lit une chaîne facultative selon son marqueur de présence.</summary>
    internal static string? ReadString(BinaryReader reader) => reader.ReadBoolean() ? reader.ReadString() : null;

    /// <summary>Écrit la longueur puis les octets empruntés.</summary>
    internal static void WriteBytes(BinaryWriter writer, ReadOnlySpan<byte> bytes)
    {
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    /// <summary>Lit un bloc binaire borné et refuse les données tronquées.</summary>
    internal static byte[] ReadBytes(BinaryReader reader, string hostName)
    {
        var length = reader.ReadInt32();
        if (length is < 0 or > EmulationHostProtocolConstants.MaximumBlobLength)
            throw new InvalidDataException(string.Format(EmulationHostProtocolConstants.InvalidBinaryPayloadLengthFormat, hostName, length));
        var bytes = reader.ReadBytes(length);
        if (bytes.Length != length) throw new EndOfStreamException(string.Format(EmulationHostProtocolConstants.BinaryPayloadEndedEarlyFormat, hostName));
        return bytes;
    }

    /// <summary>Écrit les touches, le pointeur et les manettes selon le protocole commun.</summary>
    internal static void WriteInput(BinaryWriter writer, EmulationInputSnapshot input)
    {
        writer.Write(input.Keys.Count);
        foreach (var key in input.Keys) writer.Write((int)key);
        writer.Write(input.Pointer.DeltaX); writer.Write(input.Pointer.DeltaY); writer.Write(input.Pointer.Wheel);
        writer.Write(input.Pointer.Left); writer.Write(input.Pointer.Right); writer.Write(input.Pointer.Middle);
        writer.Write(input.Pointer.ExtendedButton1); writer.Write(input.Pointer.ExtendedButton2);
        writer.Write(input.Pointer.HorizontalWheel);
        writer.Write(input.Controllers.Count);
        foreach (var controller in input.Controllers)
        {
            writer.Write(controller.Buttons); writer.Write(controller.LeftX); writer.Write(controller.LeftY);
            writer.Write(controller.RightX); writer.Write(controller.RightY);
            writer.Write(controller.LeftTrigger); writer.Write(controller.RightTrigger);
            writer.Write(controller.DeviceId);
            writer.Write(controller.Controls.Count);
            foreach (var control in controller.Controls.OrderBy(item => item.Key, StringComparer.Ordinal))
            {
                writer.Write(control.Key);
                writer.Write(control.Value);
            }
        }
    }

    /// <summary>Lit les entrées en contrôlant les nombres de touches, manettes et contrôles.</summary>
    internal static EmulationInputSnapshot ReadInput(BinaryReader reader, string hostName)
    {
        var keyCount = reader.ReadInt32();
        if (keyCount is < 0 or > EmulationHostProtocolConstants.MaximumInputKeyCount)
            throw new InvalidDataException(string.Format(EmulationHostProtocolConstants.InvalidKeyCountFormat, hostName));
        var keys = new HashSet<EmulationKey>();
        for (var index = 0; index < keyCount; index++) keys.Add((EmulationKey)reader.ReadInt32());
        var pointer = new EmulationPointerState(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadInt32());
        var controllerCount = reader.ReadInt32();
        if (controllerCount is < 0 or > EmulationHostProtocolConstants.MaximumInputControllerCount)
            throw new InvalidDataException(string.Format(EmulationHostProtocolConstants.InvalidControllerCountFormat, hostName));
        var controllers = new EmulationControllerState[controllerCount];
        for (var index = 0; index < controllerCount; index++)
        {
            var controller = new EmulationControllerState(reader.ReadUInt32(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
            var deviceId = reader.ReadString();
            var controlCount = reader.ReadInt32();
            if (controlCount is < 0 or > EmulationHostProtocolConstants.MaximumControllerControlCount)
                throw new InvalidDataException(string.Format(EmulationHostProtocolConstants.InvalidControllerControlCountFormat, hostName));
            var controls = new Dictionary<string, float>(controlCount, StringComparer.OrdinalIgnoreCase);
            for (var controlIndex = 0; controlIndex < controlCount; controlIndex++)
                controls[reader.ReadString()] = reader.ReadSingle();
            controllers[index] = controlCount == 0 ? controller with { DeviceId = deviceId } : controller with { DeviceId = deviceId, Controls = new EmulationControllerControls(controls) };
        }
        return new EmulationInputSnapshot(keys, pointer, controllers);
    }

    /// <summary>Écrit une image facultative et ses métadonnées.</summary>
    internal static void WriteFrame(BinaryWriter writer, VideoFrame? frame)
    {
        writer.Write(frame is not null);
        if (frame is null) return;
        writer.Write(frame.Width); writer.Write(frame.Height); writer.Write(frame.Pitch); writer.Write((int)frame.PixelFormat);
        writer.Write(frame.AspectRatio); writer.Write(frame.Sequence); writer.Write(frame.Timestamp.Ticks);
        WriteBytes(writer, frame.Pixels.Span);
    }

    /// <summary>Lit une image et contrôle la cohérence de ses dimensions, pitch et octets.</summary>
    internal static VideoFrame? ReadFrame(BinaryReader reader, string hostName)
    {
        if (!reader.ReadBoolean()) return null;
        var width = reader.ReadInt32(); var height = reader.ReadInt32(); var pitch = reader.ReadInt32();
        var format = (EmulationPixelFormat)reader.ReadInt32(); var aspect = reader.ReadSingle();
        var sequence = reader.ReadInt64(); var timestamp = TimeSpan.FromTicks(reader.ReadInt64());
        var pixels = ReadBytes(reader, hostName);
        if (width <= 0 || height <= 0 || pitch <= 0 || pixels.Length != checked(pitch * height))
            throw new InvalidDataException(string.Format(EmulationHostProtocolConstants.InvalidVideoFrameFormat, hostName));
        return new VideoFrame(pixels, width, height, pitch, format, aspect, sequence, timestamp);
    }

    /// <summary>Copie une image dans la vue vidéo empruntée puis écrit les métadonnées du slot.</summary>
    internal static void WriteSharedFrame(BinaryWriter writer, VideoFrame? frame, MemoryMappedViewAccessor videoMap, string hostName)
    {
        writer.Write(frame is not null);
        if (frame is null) return;
        var length = frame.Pixels.Length;
        if (length <= 0 || length > EmulationHostProtocolConstants.VideoSlotCapacity)
            throw new InvalidDataException(string.Format(EmulationHostProtocolConstants.InvalidVideoFrameLengthFormat, hostName, length, EmulationHostProtocolConstants.VideoSlotCapacity));
        var slot = (int)(frame.Sequence & (EmulationHostProtocolConstants.VideoSlotCount - 1));
        var pixels = frame.Pixels.ToArray();
        videoMap.WriteArray((long)slot * EmulationHostProtocolConstants.VideoSlotCapacity, pixels, 0, pixels.Length);
        writer.Write(frame.Width); writer.Write(frame.Height); writer.Write(frame.Pitch); writer.Write((int)frame.PixelFormat);
        writer.Write(frame.AspectRatio); writer.Write(frame.Sequence); writer.Write(frame.Timestamp.Ticks);
        writer.Write(slot); writer.Write(length);
    }

    /// <summary>Lit les métadonnées et copie le slot de mémoire vidéo emprunté dans le résultat.</summary>
    internal static VideoFrame? ReadSharedFrame(BinaryReader reader, MemoryMappedViewAccessor videoMap, string hostName)
    {
        if (!reader.ReadBoolean()) return null;
        var width = reader.ReadInt32(); var height = reader.ReadInt32(); var pitch = reader.ReadInt32();
        var format = (EmulationPixelFormat)reader.ReadInt32(); var aspect = reader.ReadSingle();
        var sequence = reader.ReadInt64(); var timestamp = TimeSpan.FromTicks(reader.ReadInt64());
        var slot = reader.ReadInt32(); var length = reader.ReadInt32();
        if (width <= 0 || height <= 0 || pitch <= 0 || length != checked(pitch * height)
            || slot is < 0 or >= EmulationHostProtocolConstants.VideoSlotCount
            || length > EmulationHostProtocolConstants.VideoSlotCapacity)
            throw new InvalidDataException(string.Format(EmulationHostProtocolConstants.InvalidSharedVideoMetadataFormat, hostName));
        var pixels = GC.AllocateUninitializedArray<byte>(length);
        var read = videoMap.ReadArray((long)slot * EmulationHostProtocolConstants.VideoSlotCapacity, pixels, 0, length);
        if (read != length) throw new EndOfStreamException(string.Format(EmulationHostProtocolConstants.SharedVideoEndedEarlyFormat, hostName));
        return new VideoFrame(pixels, width, height, pitch, format, aspect, sequence, timestamp);
    }

    /// <summary>Écrit les blocs audio après validation de leurs échantillons et de leur disposition de canaux.</summary>
    internal static void WriteAudio(BinaryWriter writer, IReadOnlyList<AudioChunk> chunks)
    {
        writer.Write(chunks.Count);
        foreach (var chunk in chunks)
        {
            if (!chunk.HasValidLength) throw new InvalidDataException(string.Format(EmulationHostProtocolConstants.InvalidPcmPayloadFormat, nameof(AudioChunk)));
            writer.Write(chunk.SampleRate); writer.Write(chunk.FrameCount); writer.Write((uint)chunk.ChannelLayout);
            writer.Write(chunk.Sequence); writer.Write(chunk.Timestamp.Ticks);
            WriteBytes(writer, MemoryMarshal.AsBytes(chunk.InterleavedSamples.Span));
        }
    }

    /// <summary>Lit et valide les blocs audio du protocole commun.</summary>
    internal static IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader, string hostName)
    {
        var count = reader.ReadInt32();
        if (count is < 0 or > EmulationHostProtocolConstants.MaximumAudioChunkCount)
            throw new InvalidDataException(string.Format(EmulationHostProtocolConstants.InvalidAudioChunkCountFormat, hostName));
        var chunks = new AudioChunk[count];
        for (var index = 0; index < count; index++)
        {
            var sampleRate = reader.ReadInt32(); var frameCount = reader.ReadInt32();
            var layout = (EmulationAudioChannelLayout)reader.ReadUInt32();
            var sequence = reader.ReadInt64(); var timestamp = TimeSpan.FromTicks(reader.ReadInt64());
            var bytes = ReadBytes(reader, hostName);
            if (bytes.Length % EmulationHostProtocolConstants.BytesPerPcmSample != 0)
                throw new InvalidDataException(string.Format(EmulationHostProtocolConstants.InvalidPcmPayloadFormat, hostName));
            var samples = new short[bytes.Length / EmulationHostProtocolConstants.BytesPerPcmSample];
            Buffer.BlockCopy(bytes, 0, samples, 0, bytes.Length);
            var chunk = new AudioChunk(samples, sampleRate, frameCount, sequence, timestamp, layout);
            if (!chunk.HasValidLength) throw new InvalidDataException(string.Format(EmulationHostProtocolConstants.InvalidPcmPayloadFormat, hostName));
            chunks[index] = chunk;
        }
        return chunks;
    }

    /// <summary>Écrit les activités des lecteurs dans l’ordre de leurs identifiants.</summary>
    internal static void WriteLedStates(BinaryWriter writer, IReadOnlyDictionary<int, bool> states)
    {
        writer.Write(states.Count);
        foreach (var state in states.OrderBy(pair => pair.Key))
        {
            writer.Write(state.Key);
            writer.Write(state.Value);
        }
    }

    /// <summary>Lit la liste bornée des activités de lecteurs.</summary>
    internal static IReadOnlyDictionary<int, bool> ReadLedStates(BinaryReader reader, string hostName)
    {
        var count = reader.ReadInt32();
        if (count is < 0 or > EmulationHostProtocolConstants.MaximumLedStateCount)
            throw new InvalidDataException(string.Format(EmulationHostProtocolConstants.InvalidLedStateCountFormat, hostName, count));
        var states = new Dictionary<int, bool>(count);
        for (var index = 0; index < count; index++) states[reader.ReadInt32()] = reader.ReadBoolean();
        return states;
    }
}
