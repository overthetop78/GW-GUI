using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Reading;

namespace GWGUI.MediaEngine.Containers.Atari.Msa;

public sealed class MsaReader : ISectorImageReader
{
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Msa, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var source = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (source.Length < MsaLayout.HeaderSize || ReadWord(source, MsaLayout.SignatureOffset) != MsaFormat.Signature) throw new InvalidDataException("The MSA header is invalid.");
        var sectors = ReadWord(source, MsaLayout.SectorsPerTrackOffset);
        var heads = ReadWord(source, MsaLayout.HeadsOffset) + 1;
        var startCylinder = ReadWord(source, MsaLayout.StartCylinderOffset);
        var endCylinder = ReadWord(source, MsaLayout.EndCylinderOffset);
        if (sectors is < MsaLayout.MinimumSectorsPerTrack or > MsaLayout.MaximumSectorsPerTrack || heads is < MsaLayout.MinimumHeadCount or > MsaLayout.MaximumHeadCount || endCylinder < startCylinder || endCylinder > MsaLayout.MaximumCylinder) throw new InvalidDataException("The MSA geometry is invalid.");
        var trackBytes = checked(sectors * MsaLayout.SectorSize);
        var position = MsaLayout.HeaderSize;
        var blocks = new List<SectorBlock>();
        for (var cylinder = startCylinder; cylinder <= endCylinder; cylinder++)
        {
            for (var head = 0; head < heads; head++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (position + MsaLayout.TrackLengthFieldSize > source.Length) throw new InvalidDataException("The MSA track table is truncated.");
                var packedLength = ReadWord(source, position);
                position += MsaLayout.TrackLengthFieldSize;
                if (position + packedLength > source.Length) throw new InvalidDataException("An MSA track is truncated.");
                var track = packedLength == trackBytes ? source.AsSpan(position, packedLength).ToArray() : MsaRleDecoder.Unpack(source.AsSpan(position, packedLength), trackBytes);
                position += packedLength;
                for (var sector = 0; sector < sectors; sector++)
                {
                    var logical = (cylinder * heads + head) * sectors + sector;
                    blocks.Add(new(logical, new(cylinder, head, sector + 1), track.AsSpan(sector * MsaLayout.SectorSize, MsaLayout.SectorSize).ToArray()));
                }
            }
        }
        return new(MsaFormat.FormatId((endCylinder + 1) * heads * sectors * (long)MsaLayout.SectorSize), MsaLayout.SectorSize, endCylinder + 1, heads, sectors, blocks);
    }

    private static int ReadWord(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
}
