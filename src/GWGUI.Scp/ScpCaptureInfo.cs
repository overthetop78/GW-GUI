using System.Buffers.Binary;

namespace GWGUI.Scp;

public sealed record ScpCaptureInfo(
    ScpHeader Header,
    int CapturedTracks,
    int MissingTracks,
    int Cylinders,
    int Sides,
    bool ChecksumValid,
    long FileSize);

public static class ScpCaptureInfoReader
{
    public static async Task<ScpCaptureInfo> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var tableLength = ScpReader.TrackTableOffset + ScpReader.FloppyTrackSlots * 4;
        var table = new byte[tableLength];
        await stream.ReadExactlyAsync(table, cancellationToken).ConfigureAwait(false);
        var header = ScpHeaderReader.Read(table);
        var slots = new List<int>();
        for (var slot = header.StartTrack; slot <= header.EndTrack; slot++)
        {
            if (BinaryPrimitives.ReadUInt32LittleEndian(table.AsSpan(ScpReader.TrackTableOffset + slot * 4, 4)) != 0) slots.Add(slot);
        }

        stream.Position = ScpReader.TrackTableOffset;
        var buffer = new byte[81920];
        uint checksum = 0;
        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read == 0) break;
            for (var index = 0; index < read; index++) checksum = unchecked(checksum + buffer[index]);
        }
        var checksumValid = header.Checksum == 0 && (header.Flags & ScpFlags.Writable) != 0 || checksum == header.Checksum;
        return new(
            header,
            slots.Count,
            Math.Max(0, header.TrackCount - slots.Count),
            slots.Select(slot => slot / 2).Distinct().Count(),
            slots.Select(slot => slot % 2).Distinct().Count(),
            checksumValid,
            stream.Length);
    }
}
