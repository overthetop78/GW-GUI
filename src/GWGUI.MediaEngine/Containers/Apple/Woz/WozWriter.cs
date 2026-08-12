using System.Buffers.Binary;
using System.Text;
using GWGUI.MediaEngine.Encoding.BitPacking;

namespace GWGUI.MediaEngine.Containers.Apple.Woz;

/// <summary>Sérialise des pistes Apple dans un conteneur WOZ version 1.</summary>
internal static class WozWriter
{
    /// <summary>Nombre maximal de bits d'une piste WOZ1.</summary>
    public const int MaximumTrackBitCount = WozLayout.Woz1MaximumBitCount;

    /// <summary>Valide, empaquette puis écrit un conteneur WOZ1 complet.</summary>
    /// <param name="tracks">Pistes binaires dans l'ordre Apple II.</param>
    /// <param name="path">Chemin du fichier de destination.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    public static async Task WriteAsync(IReadOnlyList<IReadOnlyList<bool>> tracks, string path, CancellationToken cancellationToken = default)
    {
        if (tracks.Count == 0 || tracks.Count > WozLayout.AppleIITrackCount) throw WozExceptions.InvalidTrackCount(tracks.Count, WozLayout.AppleIITrackCount);
        for (var track = 0; track < tracks.Count; track++) if (tracks[track].Count > MaximumTrackBitCount) throw WozExceptions.TrackTooLong(track, tracks[track].Count, MaximumTrackBitCount);
        using var stream = new MemoryStream();
        stream.Write(WozFormat.Version1Signature);
        stream.Write(WozFormat.HeaderMarker);
        stream.Write(new byte[WozLayout.CrcLength]);
        WozChunkWriter.Write(stream, WozFormat.InfoChunkId, CreateInfo());
        WozChunkWriter.Write(stream, WozFormat.TrackMapChunkId, CreateTrackMap(tracks.Count));
        WozChunkWriter.Write(stream, WozFormat.TracksChunkId, CreateTracks(tracks));
        var output = stream.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(WozLayout.CrcOffset, WozLayout.CrcLength), WozCrc32.Compute(output.AsSpan(WozLayout.ChunksOffset)));
        await File.WriteAllBytesAsync(path, output, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Construit le chunk INFO WOZ1.</summary>
    private static byte[] CreateInfo()
    {
        var info = new byte[WozLayout.InfoLength];
        info[WozLayout.InfoVersionOffset] = WozFormat.InfoVersion1;
        info[WozLayout.InfoDiskTypeOffset] = WozFormat.AppleII525DiskType;
        info[WozLayout.InfoWriteProtectionOffset] = WozFormat.Writable;
        info[WozLayout.InfoSynchronizedOffset] = WozFormat.Synchronized;
        info[WozLayout.InfoCleanedOffset] = WozFormat.Cleaned;
        System.Text.Encoding.ASCII.GetBytes(WozFormat.Creator).CopyTo(info, WozLayout.InfoCreatorOffset);
        return info;
    }

    /// <summary>Construit le chunk TMAP depuis le nombre de pistes fourni.</summary>
    private static byte[] CreateTrackMap(int trackCount)
    {
        var map = new byte[WozLayout.TrackMapLength];
        Array.Fill(map, WozLayout.MissingTrackDescriptor);
        for (var track = 0; track < trackCount; track++) for (var quarter = 0; quarter < WozLayout.TrackMapEntriesPerTrack; quarter++) map[track * WozLayout.TrackMapEntriesPerTrack + quarter] = (byte)track;
        return map;
    }

    /// <summary>Construit les entrées de pistes du chunk TRKS.</summary>
    private static byte[] CreateTracks(IReadOnlyList<IReadOnlyList<bool>> tracks)
    {
        var output = new byte[checked(tracks.Count * WozLayout.Woz1TrackEntryLength)];
        for (var track = 0; track < tracks.Count; track++)
        {
            var entry = output.AsSpan(track * WozLayout.Woz1TrackEntryLength, WozLayout.Woz1TrackEntryLength);
            MsbFirstBitPacker.Pack(tracks[track], entry[..WozLayout.Woz1BitCountOffset], true);
            BinaryPrimitives.WriteUInt16LittleEndian(entry.Slice(WozLayout.Woz1BitCountOffset, WozLayout.Woz1BitCountLength), checked((ushort)tracks[track].Count));
        }
        return output;
    }
}
