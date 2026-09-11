using System.Buffers.Binary;
using GWGUI.Domain.Constants;
using GWGUI.Domain.Contracts;

namespace GWGUI.Domain.Functions;

/// <summary>Serializes and deserializes the versioned neutral flux-track payload.</summary>
public static class MediaFluxTrackDataFunctions
{
    public static byte[] Serialize(MediaFluxTrackData track)
    {
        ArgumentNullException.ThrowIfNull(track);
        if (track.Revolutions.Count > MediaPhysicalEncodingIds.MaximumFluxRevolutions)
            throw new InvalidDataException("The flux track contains too many revolutions.");
        if (track.Revolutions.Any(revolution =>
                revolution.FluxIntervalsNanoseconds.Count > MediaPhysicalEncodingIds.MaximumFluxIntervalsPerRevolution))
            throw new InvalidDataException("A flux revolution contains too many intervals.");

        var length = checked((long)MediaPhysicalEncodingIds.FluxTrackV1HeaderSize
            + track.Revolutions.Sum(revolution =>
                (long)MediaPhysicalEncodingIds.FluxTrackV1RevolutionHeaderSize
                + (long)revolution.FluxIntervalsNanoseconds.Count * MediaPhysicalEncodingIds.UInt32Size));
        if (length > int.MaxValue) throw new InvalidDataException("The serialized flux track is too large.");
        var result = new byte[(int)length];
        BinaryPrimitives.WriteUInt32LittleEndian(result, MediaPhysicalEncodingIds.FluxTrackV1Magic);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4), checked((uint)track.Revolutions.Count));
        var offset = MediaPhysicalEncodingIds.FluxTrackV1HeaderSize;
        foreach (var revolution in track.Revolutions)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(offset), revolution.IndexTimeNanoseconds);
            BinaryPrimitives.WriteUInt32LittleEndian(
                result.AsSpan(offset + MediaPhysicalEncodingIds.UInt32Size),
                checked((uint)revolution.FluxIntervalsNanoseconds.Count));
            offset += MediaPhysicalEncodingIds.FluxTrackV1RevolutionHeaderSize;
            foreach (var interval in revolution.FluxIntervalsNanoseconds)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(offset), interval);
                offset += MediaPhysicalEncodingIds.UInt32Size;
            }
        }
        return result;
    }

    public static MediaFluxTrackData Deserialize(ReadOnlySpan<byte> data)
    {
        if (data.Length < MediaPhysicalEncodingIds.FluxTrackV1HeaderSize)
            throw new InvalidDataException("The neutral flux-track header is incomplete.");
        if (BinaryPrimitives.ReadUInt32LittleEndian(data) != MediaPhysicalEncodingIds.FluxTrackV1Magic)
            throw new InvalidDataException("The neutral flux-track signature is invalid.");
        var revolutionCount = BinaryPrimitives.ReadUInt32LittleEndian(data[4..]);
        if (revolutionCount is 0 or > MediaPhysicalEncodingIds.MaximumFluxRevolutions)
            throw new InvalidDataException("The neutral flux-track revolution count is invalid.");

        var revolutions = new List<MediaFluxRevolutionData>(checked((int)revolutionCount));
        var offset = MediaPhysicalEncodingIds.FluxTrackV1HeaderSize;
        for (var revolutionIndex = 0; revolutionIndex < revolutionCount; revolutionIndex++)
        {
            if (offset > data.Length - MediaPhysicalEncodingIds.FluxTrackV1RevolutionHeaderSize)
                throw new InvalidDataException("A neutral flux-revolution header is incomplete.");
            var indexTime = BinaryPrimitives.ReadUInt32LittleEndian(data[offset..]);
            var intervalCount = BinaryPrimitives.ReadUInt32LittleEndian(data[(offset + MediaPhysicalEncodingIds.UInt32Size)..]);
            if (intervalCount > MediaPhysicalEncodingIds.MaximumFluxIntervalsPerRevolution)
                throw new InvalidDataException("A neutral flux revolution contains too many intervals.");
            offset += MediaPhysicalEncodingIds.FluxTrackV1RevolutionHeaderSize;
            var byteCount = checked((long)intervalCount * MediaPhysicalEncodingIds.UInt32Size);
            if (byteCount > data.Length - offset)
                throw new InvalidDataException("A neutral flux revolution exceeds its track payload.");
            var intervals = new uint[checked((int)intervalCount)];
            for (var intervalIndex = 0; intervalIndex < intervals.Length; intervalIndex++)
            {
                intervals[intervalIndex] = BinaryPrimitives.ReadUInt32LittleEndian(data[offset..]);
                offset += MediaPhysicalEncodingIds.UInt32Size;
            }
            revolutions.Add(new MediaFluxRevolutionData(indexTime, intervals));
        }
        if (offset != data.Length)
            throw new InvalidDataException("The neutral flux-track payload contains trailing data.");
        return new MediaFluxTrackData(revolutions);
    }
}
