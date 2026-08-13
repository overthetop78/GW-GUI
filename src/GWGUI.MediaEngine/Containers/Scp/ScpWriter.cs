using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Écrit atomiquement un conteneur SCP standard à partir de son modèle de pistes et de révolutions.</summary>
public sealed class ScpWriter
{
    /// <summary>Écrit l'image dans un fichier temporaire, finalise sa table et sa somme de contrôle, puis remplace la destination.</summary>
    public async Task WriteAsync(string path, ScpImage image, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(image);
        Validate(image);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await WriteTemporaryAsync(temporaryPath, image, cancellationToken).ConfigureAwait(false);
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    /// <summary>Écrit le corps, puis revient compléter les offsets et le checksum avant fermeture.</summary>
    private static async Task WriteTemporaryAsync(string path, ScpImage image, CancellationToken cancellationToken)
    {
        await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous);
        var prefixLength = ScpFormatConstants.TrackTableOffset + ScpFormatConstants.FloppyTrackSlots * ScpFormatConstants.TrackTableEntrySize;
        await output.WriteAsync(new byte[prefixLength], cancellationToken).ConfigureAwait(false);
        var offsets = new uint[ScpFormatConstants.FloppyTrackSlots];
        foreach (var track in image.Tracks.OrderBy(track => track.TrackNumber))
        {
            cancellationToken.ThrowIfCancellationRequested();
            offsets[track.TrackNumber] = checked((uint)output.Position);
            var bytes = BuildTrack(track);
            await output.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        }

        var header = BuildHeader(image.Header);
        output.Position = ScpFormatConstants.FileStartOffset;
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        var table = BuildTrackTable(offsets);
        await output.WriteAsync(table, cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
        var checksum = await ComputeChecksumAsync(output, cancellationToken).ConfigureAwait(false);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(ScpFormatConstants.ChecksumOffset, ScpFormatConstants.ChecksumLength), checksum);
        output.Position = ScpFormatConstants.FileStartOffset;
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Construit l'en-tête fixe sans reprendre un checksum devenu obsolète.</summary>
    private static byte[] BuildHeader(ScpHeader source)
    {
        var header = new byte[ScpFormatConstants.HeaderLength];
        ScpFormatConstants.FileSignature.CopyTo(header);
        header[ScpFormatConstants.VersionOffset] = source.Version;
        header[ScpFormatConstants.DiskTypeOffset] = source.DiskType;
        header[ScpFormatConstants.RevolutionCountOffset] = source.Revolutions;
        header[ScpFormatConstants.StartTrackOffset] = source.StartTrack;
        header[ScpFormatConstants.EndTrackOffset] = source.EndTrack;
        header[ScpFormatConstants.FlagsOffset] = (byte)source.Flags;
        header[ScpFormatConstants.BitCellWidthOffset] = (byte)source.BitCellEncoding;
        header[ScpFormatConstants.HeadsOffset] = (byte)source.Heads;
        header[ScpFormatConstants.ResolutionOffset] = source.Resolution;
        return header;
    }

    /// <summary>Encode les 168 offsets de pistes en little-endian.</summary>
    private static byte[] BuildTrackTable(IReadOnlyList<uint> offsets)
    {
        var table = new byte[ScpFormatConstants.FloppyTrackSlots * ScpFormatConstants.TrackTableEntrySize];
        for (var index = 0; index < offsets.Count; index++) BinaryPrimitives.WriteUInt32LittleEndian(table.AsSpan(index * ScpFormatConstants.TrackTableEntrySize, ScpFormatConstants.TrackTableEntrySize), offsets[index]);
        return table;
    }

    /// <summary>Encode les descripteurs et les mots de flux d'une piste.</summary>
    private static byte[] BuildTrack(ScpTrack track)
    {
        var words = track.Revolutions.Select(EncodeIntervals).ToArray();
        var descriptorLength = checked(ScpFormatConstants.TrackDescriptorHeaderSize + words.Length * ScpFormatConstants.RevolutionDescriptorSize);
        var length = checked(descriptorLength + words.Sum(values => values.Length * ScpFormatConstants.FluxIntervalSize));
        var data = new byte[length];
        ScpFormatConstants.TrackSignature.CopyTo(data);
        data[ScpFormatConstants.TrackNumberOffset] = track.TrackNumber;
        var dataOffset = descriptorLength;
        for (var index = 0; index < track.Revolutions.Count; index++)
        {
            var descriptorOffset = ScpFormatConstants.TrackDescriptorHeaderSize + index * ScpFormatConstants.RevolutionDescriptorSize;
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(descriptorOffset + ScpFormatConstants.RevolutionIndexTimeOffset, sizeof(uint)), track.Revolutions[index].IndexTimeTicks);
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(descriptorOffset + ScpFormatConstants.RevolutionFluxCountOffset, sizeof(uint)), checked((uint)words[index].Length));
            BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(descriptorOffset + ScpFormatConstants.RevolutionDataOffset, sizeof(uint)), checked((uint)dataOffset));
            foreach (var word in words[index])
            {
                BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(dataOffset, ScpFormatConstants.FluxIntervalSize), word);
                dataOffset += ScpFormatConstants.FluxIntervalSize;
            }
        }

        return data;
    }

    /// <summary>Découpe les grands intervalles avec les marqueurs de débordement définis par SCP.</summary>
    private static ushort[] EncodeIntervals(ScpRevolution revolution)
    {
        var words = new List<ushort>();
        uint carry = 0;
        for (var index = 0; index < revolution.FluxIntervals.Count; index++)
        {
            var remaining = checked(revolution.FluxIntervals[index] + carry);
            carry = 0;
            if (index + 1 < revolution.FluxIntervals.Count && remaining % ScpFormatConstants.ZeroFluxIntervalOverflow == 0)
            {
                remaining--;
                carry = 1;
            }
            while (remaining >= ScpFormatConstants.ZeroFluxIntervalOverflow)
            {
                words.Add(ScpFormatConstants.FluxOverflowMarker);
                remaining -= ScpFormatConstants.ZeroFluxIntervalOverflow;
            }
            if (remaining > 0) words.Add(checked((ushort)remaining));
        }

        return words.ToArray();
    }

    /// <summary>Calcule la somme des octets depuis la table jusqu'à la fin du fichier temporaire.</summary>
    private static async Task<uint> ComputeChecksumAsync(FileStream stream, CancellationToken cancellationToken)
    {
        stream.Position = ScpFormatConstants.TrackTableOffset;
        var buffer = new byte[81920];
        var checksum = ScpFormatConstants.InitialChecksum;
        int read;
        while ((read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0) checksum = ScpFormatAlgorithms.UpdateChecksum(checksum, buffer.AsSpan(0, read));
        return checksum;
    }

    /// <summary>Valide les invariants nécessaires à une table SCP non ambiguë.</summary>
    private static void Validate(ScpImage image)
    {
        var header = image.Header;
        if (header.Revolutions is < ScpFormatConstants.MinimumRevolutionCount or > ScpFormatConstants.MaximumRevolutionCount) throw ScpWriterExceptions.InvalidRevolutionCount(header.Revolutions);
        if (header.EndTrack < header.StartTrack || header.EndTrack >= ScpFormatConstants.FloppyTrackSlots) throw ScpWriterExceptions.InvalidTrackRange(header.StartTrack, header.EndTrack);
        _ = ScpReader.ReadHeader(BuildHeader(header));
        if ((header.Flags & ScpFlags.Extended) != ScpFlags.None) throw ScpWriterExceptions.ExtendedMedia();
        var seen = new HashSet<byte>();
        foreach (var track in image.Tracks)
        {
            if (!seen.Add(track.TrackNumber)) throw ScpWriterExceptions.DuplicateTrack(track.TrackNumber);
            if (track.TrackNumber < header.StartTrack || track.TrackNumber > header.EndTrack) throw ScpWriterExceptions.TrackOutsideRange(track.TrackNumber, header.StartTrack, header.EndTrack);
            var address = ScpFormatAlgorithms.ToTrackAddress(track.TrackNumber);
            if (track.Cylinder != address.Cylinder || track.Head != address.Head) throw ScpWriterExceptions.TrackAddressMismatch(track.TrackNumber, track.Cylinder, track.Head);
            if (track.Revolutions.Count != header.Revolutions) throw ScpWriterExceptions.RevolutionCountMismatch(track.TrackNumber, header.Revolutions, track.Revolutions.Count);
            for (var index = 0; index < track.Revolutions.Count; index++) if (track.Revolutions[index].FluxIntervals.Any(interval => interval == 0)) throw ScpWriterExceptions.EmptyFluxInterval(track.TrackNumber, index + ScpFormatConstants.RevolutionNumberOffset);
        }
    }
}
