using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using System.Runtime.Versioning;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Functions;

internal static partial class EmulationHostProtocolFunctions
{
    [SupportedOSPlatform("windows")]
    internal static void WithSharedVideoMap(string videoMapName, long viewOffset, Action<MemoryMappedViewAccessor> run)
    {
        using var videoMemory = MemoryMappedFile.OpenExisting(videoMapName, MemoryMappedFileRights.ReadWrite);
        using var videoMap = videoMemory.CreateViewAccessor(viewOffset, EmulationHostProtocolConstants.VideoMapCapacity,
            MemoryMappedFileAccess.ReadWrite);
        run(videoMap);
    }

    internal static int CalculateVideoSlotCapacity(int frameLength, int minimumSlotCapacity, string hostName)
    {
        if (frameLength <= 0 || frameLength > EmulationHostProtocolConstants.VideoSlotCapacity)
            throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, EmulationHostProtocolConstants.InvalidVideoFrameLengthFormat, hostName, frameLength, EmulationHostProtocolConstants.VideoSlotCapacity));
        var minimum = Math.Max(frameLength, minimumSlotCapacity);
        return checked((int)BitOperations.RoundUpToPowerOf2((uint)minimum));
    }

    [SupportedOSPlatform("windows")]
    internal static void WriteResizableSharedFrame(BinaryWriter writer, VideoFrame? frame, EmulationSharedVideoWriter video)
    {
        writer.Write(frame is not null);
        if (frame is null) return;
        var length = frame.Pixels.Length;
        video.EnsureCapacity(length);
        var slot = (int)(frame.Sequence & (EmulationHostProtocolConstants.VideoSlotCount - 1));
        var pixels = frame.Pixels.ToArray();
        video.View!.WriteArray((long)slot * video.SlotCapacity, pixels, 0, pixels.Length);
        writer.Write(video.Name!);
        writer.Write(video.SlotCapacity);
        writer.Write(frame.Width);
        writer.Write(frame.Height);
        writer.Write(frame.Pitch);
        writer.Write((int)frame.PixelFormat);
        writer.Write(frame.AspectRatio);
        writer.Write(frame.Sequence);
        writer.Write(frame.Timestamp.Ticks);
        writer.Write(slot);
        writer.Write(length);
    }

    [SupportedOSPlatform("windows")]
    internal static VideoFrame? ReadResizableSharedFrame(BinaryReader reader, ref MemoryMappedFile? memory, ref MemoryMappedViewAccessor? view, ref string? activeName, int minimumSlotCapacity)
    {
        if (!reader.ReadBoolean()) return null;
        var mapName = reader.ReadString();
        var slotCapacity = reader.ReadInt32();
        if (slotCapacity < minimumSlotCapacity
            || slotCapacity > EmulationHostProtocolConstants.VideoSlotCapacity
            || !BitOperations.IsPow2(slotCapacity))
            throw new InvalidDataException(EmulationHostProtocolConstants.InvalidSharedVideoMetadata);
        if (!string.Equals(mapName, activeName, StringComparison.Ordinal))
        {
            view?.Dispose();
            memory?.Dispose();
            memory = MemoryMappedFile.OpenExisting(mapName, MemoryMappedFileRights.Read);
            view = memory.CreateViewAccessor(0, checked((long)slotCapacity * EmulationHostProtocolConstants.VideoSlotCount), MemoryMappedFileAccess.Read);
            activeName = mapName;
        }
        var width = reader.ReadInt32();
        var height = reader.ReadInt32();
        var pitch = reader.ReadInt32();
        var format = (EmulationPixelFormat)reader.ReadInt32();
        var aspect = reader.ReadSingle();
        var sequence = reader.ReadInt64();
        var timestamp = TimeSpan.FromTicks(reader.ReadInt64());
        var slot = reader.ReadInt32();
        var length = reader.ReadInt32();
        if (width <= 0 || height <= 0 || pitch <= 0 || length != checked(pitch * height)
            || slot is < 0 or >= EmulationHostProtocolConstants.VideoSlotCount
            || length > slotCapacity)
            throw new InvalidDataException(EmulationHostProtocolConstants.InvalidSharedVideoMetadata);
        var pixels = GC.AllocateUninitializedArray<byte>(length);
        var read = view!.ReadArray((long)slot * slotCapacity, pixels, 0, length);
        if (read != length) throw new EndOfStreamException(EmulationHostProtocolConstants.SharedVideoEndedEarly);
        return new VideoFrame(pixels, width, height, pitch, format, aspect, sequence, timestamp);
    }
}
