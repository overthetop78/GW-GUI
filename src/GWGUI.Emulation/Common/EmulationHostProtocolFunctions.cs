using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Common;

internal static class EmulationHostProtocolFunctions
{
    internal static void WriteString(BinaryWriter writer, string? value)
    {
        writer.Write(value is not null);
        if (value is not null) writer.Write(value);
    }

    internal static string? ReadString(BinaryReader reader) => reader.ReadBoolean() ? reader.ReadString() : null;

    internal static void WriteBytes(BinaryWriter writer, ReadOnlySpan<byte> bytes)
    {
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    internal static byte[] ReadBytes(BinaryReader reader, string hostName)
    {
        var length = reader.ReadInt32();
        if (length is < 0 or > EmulationHostProtocolConstants.MaximumBlobLength)
            throw new InvalidDataException($"The {hostName} host sent invalid binary payload length {length}.");
        var bytes = reader.ReadBytes(length);
        if (bytes.Length != length) throw new EndOfStreamException($"The {hostName} host binary payload ended early.");
        return bytes;
    }

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

    internal static EmulationInputSnapshot ReadInput(BinaryReader reader, string hostName)
    {
        var keyCount = reader.ReadInt32();
        if (keyCount is < 0 or > EmulationHostProtocolConstants.MaximumInputKeyCount)
            throw new InvalidDataException($"The {hostName} host input contains an invalid key count.");
        var keys = new HashSet<EmulationKey>();
        for (var index = 0; index < keyCount; index++) keys.Add((EmulationKey)reader.ReadInt32());
        var pointer = new EmulationPointerState(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(),
            reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(),
            reader.ReadBoolean(), reader.ReadInt32());
        var controllerCount = reader.ReadInt32();
        if (controllerCount is < 0 or > EmulationHostProtocolConstants.MaximumInputControllerCount)
            throw new InvalidDataException($"The {hostName} host input contains an invalid controller count.");
        var controllers = new EmulationControllerState[controllerCount];
        for (var index = 0; index < controllerCount; index++)
        {
            var controller = new EmulationControllerState(reader.ReadUInt32(), reader.ReadInt16(), reader.ReadInt16(),
                reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
            var deviceId = reader.ReadString();
            var controlCount = reader.ReadInt32();
            if (controlCount is < 0 or > 4096)
                throw new InvalidDataException($"The {hostName} host input contains an invalid controller control count.");
            var controls = new Dictionary<string, float>(controlCount, StringComparer.OrdinalIgnoreCase);
            for (var controlIndex = 0; controlIndex < controlCount; controlIndex++)
                controls[reader.ReadString()] = reader.ReadSingle();
            controllers[index] = controlCount == 0
                ? controller with { DeviceId = deviceId }
                : controller with { DeviceId = deviceId, Controls = new EmulationControllerControls(controls) };
        }
        return new EmulationInputSnapshot(keys, pointer, controllers);
    }

    internal static void WriteFrame(BinaryWriter writer, VideoFrame? frame)
    {
        writer.Write(frame is not null);
        if (frame is null) return;
        writer.Write(frame.Width); writer.Write(frame.Height); writer.Write(frame.Pitch); writer.Write((int)frame.PixelFormat);
        writer.Write(frame.AspectRatio); writer.Write(frame.Sequence); writer.Write(frame.Timestamp.Ticks);
        WriteBytes(writer, frame.Pixels.Span);
    }

    internal static VideoFrame? ReadFrame(BinaryReader reader, string hostName)
    {
        if (!reader.ReadBoolean()) return null;
        var width = reader.ReadInt32(); var height = reader.ReadInt32(); var pitch = reader.ReadInt32();
        var format = (EmulationPixelFormat)reader.ReadInt32(); var aspect = reader.ReadSingle();
        var sequence = reader.ReadInt64(); var timestamp = TimeSpan.FromTicks(reader.ReadInt64());
        var pixels = ReadBytes(reader, hostName);
        if (width <= 0 || height <= 0 || pitch <= 0 || pixels.Length != checked(pitch * height))
            throw new InvalidDataException($"The {hostName} host sent an invalid video frame.");
        return new VideoFrame(pixels, width, height, pitch, format, aspect, sequence, timestamp);
    }

    internal static void WriteSharedFrame(BinaryWriter writer, VideoFrame? frame, MemoryMappedViewAccessor videoMap, string hostName)
    {
        writer.Write(frame is not null);
        if (frame is null) return;
        var length = frame.Pixels.Length;
        if (length <= 0 || length > EmulationHostProtocolConstants.VideoSlotCapacity)
            throw new InvalidDataException($"The {hostName} video frame requires {length} bytes; the shared slot supports {EmulationHostProtocolConstants.VideoSlotCapacity}.");
        var slot = (int)(frame.Sequence & (EmulationHostProtocolConstants.VideoSlotCount - 1));
        var pixels = frame.Pixels.ToArray();
        videoMap.WriteArray((long)slot * EmulationHostProtocolConstants.VideoSlotCapacity, pixels, 0, pixels.Length);
        writer.Write(frame.Width); writer.Write(frame.Height); writer.Write(frame.Pitch); writer.Write((int)frame.PixelFormat);
        writer.Write(frame.AspectRatio); writer.Write(frame.Sequence); writer.Write(frame.Timestamp.Ticks);
        writer.Write(slot); writer.Write(length);
    }

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
            throw new InvalidDataException($"The {hostName} host sent invalid shared video metadata.");
        var pixels = GC.AllocateUninitializedArray<byte>(length);
        var read = videoMap.ReadArray((long)slot * EmulationHostProtocolConstants.VideoSlotCapacity, pixels, 0, length);
        if (read != length) throw new EndOfStreamException($"The {hostName} shared video frame ended early.");
        return new VideoFrame(pixels, width, height, pitch, format, aspect, sequence, timestamp);
    }

    internal static void WriteAudio(BinaryWriter writer, IReadOnlyList<AudioChunk> chunks)
    {
        writer.Write(chunks.Count);
        foreach (var chunk in chunks)
        {
            writer.Write(chunk.SampleRate); writer.Write(chunk.FrameCount); writer.Write(chunk.Sequence); writer.Write(chunk.Timestamp.Ticks);
            WriteBytes(writer, MemoryMarshal.AsBytes(chunk.InterleavedStereo.Span));
        }
    }

    internal static IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader, string hostName)
    {
        var count = reader.ReadInt32();
        if (count is < 0 or > EmulationHostProtocolConstants.MaximumAudioChunkCount)
            throw new InvalidDataException($"The {hostName} host sent an invalid audio chunk count.");
        var chunks = new AudioChunk[count];
        for (var index = 0; index < count; index++)
        {
            var sampleRate = reader.ReadInt32(); var frameCount = reader.ReadInt32();
            var sequence = reader.ReadInt64(); var timestamp = TimeSpan.FromTicks(reader.ReadInt64());
            var bytes = ReadBytes(reader, hostName);
            if (bytes.Length % EmulationHostProtocolConstants.BytesPerPcmSample != 0)
                throw new InvalidDataException($"The {hostName} host sent an invalid PCM payload.");
            var samples = new short[bytes.Length / EmulationHostProtocolConstants.BytesPerPcmSample];
            Buffer.BlockCopy(bytes, 0, samples, 0, bytes.Length);
            chunks[index] = new AudioChunk(samples, sampleRate, frameCount, sequence, timestamp);
        }
        return chunks;
    }

    internal static void WriteLedStates(BinaryWriter writer, IReadOnlyDictionary<int, bool> states)
    {
        writer.Write(states.Count);
        foreach (var state in states.OrderBy(pair => pair.Key))
        {
            writer.Write(state.Key);
            writer.Write(state.Value);
        }
    }

    internal static IReadOnlyDictionary<int, bool> ReadLedStates(BinaryReader reader, string hostName)
    {
        var count = reader.ReadInt32();
        if (count is < 0 or > EmulationHostProtocolConstants.MaximumLedStateCount)
            throw new InvalidDataException($"Invalid {hostName} LED state count {count}.");
        var states = new Dictionary<int, bool>(count);
        for (var index = 0; index < count; index++) states[reader.ReadInt32()] = reader.ReadBoolean();
        return states;
    }
}
