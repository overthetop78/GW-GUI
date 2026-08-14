using System.IO.Pipes;
using System.Text.Json;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Cores;

internal enum AmigaHostCommand : byte
{
    Initialize = 1, RunFrame, HardReset, Stop, InsertFloppy, EjectFloppy,
    SaveState, LoadState, SetOption, SelectDisk, Dispose
}

internal static class AmigaCoreHostProtocol
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    internal const int MaximumBlobLength = 512 * 1024 * 1024;

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

    internal static byte[] ReadBytes(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        if (length is < 0 or > MaximumBlobLength) throw new InvalidDataException($"The Amiga host sent invalid binary payload length {length}.");
        var bytes = reader.ReadBytes(length);
        if (bytes.Length != length) throw new EndOfStreamException("The Amiga host binary payload ended early.");
        return bytes;
    }

    internal static void WriteInput(BinaryWriter writer, EmulationInputSnapshot input)
    {
        writer.Write(input.Keys.Count);
        foreach (var key in input.Keys) writer.Write((int)key);
        writer.Write(input.Pointer.DeltaX); writer.Write(input.Pointer.DeltaY); writer.Write(input.Pointer.Wheel);
        writer.Write(input.Pointer.Left); writer.Write(input.Pointer.Right); writer.Write(input.Pointer.Middle);
        writer.Write(input.Controllers.Count);
        foreach (var controller in input.Controllers)
        {
            writer.Write(controller.Buttons); writer.Write(controller.LeftX); writer.Write(controller.LeftY);
            writer.Write(controller.RightX); writer.Write(controller.RightY);
            writer.Write(controller.LeftTrigger); writer.Write(controller.RightTrigger);
        }
    }

    internal static EmulationInputSnapshot ReadInput(BinaryReader reader)
    {
        var keyCount = reader.ReadInt32();
        if (keyCount is < 0 or > 512) throw new InvalidDataException("The Amiga host input contains an invalid key count.");
        var keys = new HashSet<EmulationKey>();
        for (var index = 0; index < keyCount; index++) keys.Add((EmulationKey)reader.ReadInt32());
        var pointer = new EmulationPointerState(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(),
            reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean());
        var controllerCount = reader.ReadInt32();
        if (controllerCount is < 0 or > 8) throw new InvalidDataException("The Amiga host input contains an invalid controller count.");
        var controllers = new EmulationControllerState[controllerCount];
        for (var index = 0; index < controllerCount; index++)
            controllers[index] = new EmulationControllerState(reader.ReadUInt32(), reader.ReadInt16(), reader.ReadInt16(),
                reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
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

    internal static VideoFrame? ReadFrame(BinaryReader reader)
    {
        if (!reader.ReadBoolean()) return null;
        var width = reader.ReadInt32(); var height = reader.ReadInt32(); var pitch = reader.ReadInt32();
        var format = (EmulationPixelFormat)reader.ReadInt32(); var aspect = reader.ReadSingle();
        var sequence = reader.ReadInt64(); var timestamp = TimeSpan.FromTicks(reader.ReadInt64());
        var pixels = ReadBytes(reader);
        if (width <= 0 || height <= 0 || pitch <= 0 || pixels.Length != checked(pitch * height))
            throw new InvalidDataException("The Amiga host sent an invalid video frame.");
        return new VideoFrame(pixels, width, height, pitch, format, aspect, sequence, timestamp);
    }

    internal static void WriteAudio(BinaryWriter writer, IReadOnlyList<AudioChunk> chunks)
    {
        writer.Write(chunks.Count);
        foreach (var chunk in chunks)
        {
            writer.Write(chunk.SampleRate); writer.Write(chunk.FrameCount); writer.Write(chunk.Sequence); writer.Write(chunk.Timestamp.Ticks);
            var bytes = System.Runtime.InteropServices.MemoryMarshal.AsBytes(chunk.InterleavedStereo.Span);
            WriteBytes(writer, bytes);
        }
    }

    internal static IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        if (count is < 0 or > 1024) throw new InvalidDataException("The Amiga host sent an invalid audio chunk count.");
        var chunks = new AudioChunk[count];
        for (var index = 0; index < count; index++)
        {
            var sampleRate = reader.ReadInt32(); var frameCount = reader.ReadInt32();
            var sequence = reader.ReadInt64(); var timestamp = TimeSpan.FromTicks(reader.ReadInt64());
            var bytes = ReadBytes(reader);
            if ((bytes.Length & 1) != 0) throw new InvalidDataException("The Amiga host sent an invalid PCM payload.");
            var samples = new short[bytes.Length / 2];
            Buffer.BlockCopy(bytes, 0, samples, 0, bytes.Length);
            chunks[index] = new AudioChunk(samples, sampleRate, frameCount, sequence, timestamp);
        }
        return chunks;
    }
}

public static class AmigaCoreHost
{
    public static void Run(string pipeName)
    {
        using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None);
        pipe.Connect(15_000);
        using var reader = new BinaryReader(pipe, System.Text.Encoding.UTF8, true);
        using var transportWriter = new BinaryWriter(pipe, System.Text.Encoding.UTF8, true);
        AmigaExternalCore? core = null;
        var lastVideoSequence = 0L;
        while (true)
        {
            AmigaHostCommand command;
            try { command = (AmigaHostCommand)reader.ReadByte(); }
            catch (EndOfStreamException) { break; }
            var exit = command == AmigaHostCommand.Dispose;
            using var responseStream = new MemoryStream();
            using var writer = new BinaryWriter(responseStream, System.Text.Encoding.UTF8, true);
            try
            {
                switch (command)
                {
                    case AmigaHostCommand.Initialize:
                        var corePath = reader.ReadString();
                        var session = reader.ReadString();
                        var saves = AmigaCoreHostProtocol.ReadString(reader);
                        var configuration = JsonSerializer.Deserialize<AmigaMachineConfiguration>(reader.ReadString(), AmigaCoreHostProtocol.JsonOptions)
                            ?? throw new InvalidDataException("The Amiga host configuration is invalid.");
                        core = new AmigaExternalCore(corePath);
                        core.Initialize(configuration, session, saves);
                        writer.Write(true);
                        writer.Write(core.CoreSha256); writer.Write(core.FramesPerSecond); writer.Write(core.SampleRate);
                        writer.Write(JsonSerializer.Serialize(core.Options, AmigaCoreHostProtocol.JsonOptions));
                        writer.Write(JsonSerializer.Serialize(core.Diagnostics, AmigaCoreHostProtocol.JsonOptions));
                        writer.Write(core.DiskCount); writer.Write(core.CurrentDiskIndex);
                        break;
                    case AmigaHostCommand.RunFrame:
                        var activeCore = EnsureCore(core);
                        activeCore.SetInput(AmigaCoreHostProtocol.ReadInput(reader));
                        activeCore.RunFrame();
                        writer.Write(true);
                        var frame = activeCore.LatestVideoFrame;
                        AmigaCoreHostProtocol.WriteFrame(writer, frame?.Sequence == lastVideoSequence ? null : frame);
                        if (frame is not null) lastVideoSequence = frame.Sequence;
                        var audio = new List<AudioChunk>();
                        while (activeCore.TryDequeueAudio(out var chunk) && chunk is not null) audio.Add(chunk);
                        AmigaCoreHostProtocol.WriteAudio(writer, audio);
                        writer.Write(activeCore.DiskCount); writer.Write(activeCore.CurrentDiskIndex);
                        break;
                    case AmigaHostCommand.HardReset: EnsureCore(core).HardReset(); WriteSuccess(writer); break;
                    case AmigaHostCommand.Stop: EnsureCore(core).Stop(); WriteSuccess(writer); break;
                    case AmigaHostCommand.InsertFloppy: EnsureCore(core).InsertFloppy(reader.ReadString()); WriteSuccess(writer); break;
                    case AmigaHostCommand.EjectFloppy: EnsureCore(core).EjectFloppy(); WriteSuccess(writer); break;
                    case AmigaHostCommand.SaveState:
                        var state = EnsureCore(core).SaveState(); writer.Write(true); AmigaCoreHostProtocol.WriteBytes(writer, state); break;
                    case AmigaHostCommand.LoadState: EnsureCore(core).LoadState(AmigaCoreHostProtocol.ReadBytes(reader)); WriteSuccess(writer); break;
                    case AmigaHostCommand.SetOption: EnsureCore(core).SetOption(reader.ReadString(), reader.ReadString()); WriteSuccess(writer); break;
                    case AmigaHostCommand.SelectDisk: EnsureCore(core).SelectDisk(reader.ReadInt32()); WriteSuccess(writer); break;
                    case AmigaHostCommand.Dispose: core?.Dispose(); core = null; WriteSuccess(writer); break;
                    default: throw new InvalidDataException($"Unknown Amiga host command {(byte)command}.");
                }
            }
            catch (Exception error)
            {
                responseStream.SetLength(0);
                responseStream.Position = 0;
                writer.Write(false);
                writer.Write(error.ToString());
            }
            writer.Flush();
            AmigaCoreHostProtocol.WriteBytes(transportWriter,
                responseStream.GetBuffer().AsSpan(0, checked((int)responseStream.Length)));
            if (exit) break;
        }
        core?.Dispose();
    }

    private static AmigaExternalCore EnsureCore(AmigaExternalCore? core) => core ?? throw new InvalidOperationException("The Amiga host is not initialized.");
    private static void WriteSuccess(BinaryWriter writer) => writer.Write(true);
}
