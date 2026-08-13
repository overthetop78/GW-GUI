using System.Buffers.Binary;
using GWGUI.MediaEngine.Geometries.Atari;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Atari.Msa;

/// <summary>Écrit une image sectorielle Atari ST dans un conteneur Magic Shadow Archiver.</summary>
public sealed class MsaWriter
{
    /// <summary>Écrit l'en-tête puis chaque piste sous forme brute ou compressée selon la représentation la plus courte.</summary>
    public async Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        if (!AtariStGeometry.TryFromFormatId(image.FormatId, out var geometry) || image.BlockSize != AtariStGeometry.SectorSize || image.Cylinders != geometry.Cylinders || image.Heads != geometry.Heads || image.SectorsPerTrack != geometry.SectorsPerTrack) throw MsaExceptions.UnsupportedSectorImage(image);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, MsaLayout.SectorSize, FileOptions.Asynchronous))
            {
                var header = new byte[MsaLayout.HeaderSize];
                WriteWord(header, MsaLayout.SignatureOffset, MsaFormat.Signature);
                WriteWord(header, MsaLayout.SectorsPerTrackOffset, geometry.SectorsPerTrack);
                WriteWord(header, MsaLayout.HeadsOffset, geometry.Heads - 1);
                WriteWord(header, MsaLayout.StartCylinderOffset, 0);
                WriteWord(header, MsaLayout.EndCylinderOffset, geometry.Cylinders - 1);
                await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
                for (var cylinder = 0; cylinder < geometry.Cylinders; cylinder++)
                {
                    for (var head = 0; head < geometry.Heads; head++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var track = BuildTrack(image, geometry, cylinder, head);
                        var packed = MsaRleEncoder.Pack(track);
                        var payload = packed.Length < track.Length ? packed : track;
                        var length = new byte[MsaLayout.TrackLengthFieldSize];
                        WriteWord(length, 0, payload.Length);
                        await output.WriteAsync(length, cancellationToken).ConfigureAwait(false);
                        await output.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    /// <summary>Assemble une piste complète dans l'ordre logique Atari ST.</summary>
    private static byte[] BuildTrack(SectorImage image, AtariStGeometry geometry, int cylinder, int head)
    {
        var track = new byte[geometry.SectorsPerTrack * AtariStGeometry.SectorSize];
        for (var sector = 0; sector < geometry.SectorsPerTrack; sector++)
        {
            var logical = (cylinder * geometry.Heads + head) * geometry.SectorsPerTrack + sector;
            if (!image.TryGetBlock(logical, out var block)) throw MsaExceptions.MissingSector(logical, cylinder, head, sector + 1);
            if (block.Data.Count != AtariStGeometry.SectorSize) throw MsaExceptions.InvalidSectorSize(logical, block.Data.Count, AtariStGeometry.SectorSize);
            block.Data.ToArray().CopyTo(track, sector * AtariStGeometry.SectorSize);
        }
        return track;
    }

    /// <summary>Écrit un entier non signé 16 bits en ordre big-endian.</summary>
    private static void WriteWord(Span<byte> destination, int offset, int value) => BinaryPrimitives.WriteUInt16BigEndian(destination[offset..], checked((ushort)value));
}
