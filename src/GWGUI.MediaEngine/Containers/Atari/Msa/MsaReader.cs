using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Reading;

namespace GWGUI.MediaEngine.Containers.Atari.Msa;

/// <summary>Lit les conteneurs Atari ST Magic Shadow Archiver.</summary>
public sealed class MsaReader : ISectorImageReader
{
    /// <summary>Indique si l'extension du chemin correspond au format MSA.</summary>
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Msa, StringComparison.OrdinalIgnoreCase);

    /// <summary>Lit les pistes MSA brutes ou compressées et construit leur image sectorielle Atari ST.</summary>
    /// <param name="path">Chemin du fichier MSA.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler le parcours des pistes.</param>
    /// <returns>L'image sectorielle reconstruite.</returns>
    /// <exception cref="IOException">Une erreur d'entrée-sortie survient pendant la lecture.</exception>
    /// <exception cref="InvalidDataException">L'en-tête, la géométrie, une piste ou une séquence RLE est invalide.</exception>
    /// <exception cref="OverflowException">Un calcul de taille dépasse la capacité d'un entier.</exception>
    /// <exception cref="OperationCanceledException">L'opération est annulée.</exception>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var source = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (source.Length < MsaLayout.HeaderSize || ReadWord(source, MsaLayout.SignatureOffset) != MsaFormat.Signature) throw MsaExceptions.InvalidHeader(source.Length);
        var sectors = ReadWord(source, MsaLayout.SectorsPerTrackOffset);
        var heads = ReadWord(source, MsaLayout.HeadsOffset) + 1;
        var startCylinder = ReadWord(source, MsaLayout.StartCylinderOffset);
        var endCylinder = ReadWord(source, MsaLayout.EndCylinderOffset);
        if (sectors is < MsaLayout.MinimumSectorsPerTrack or > MsaLayout.MaximumSectorsPerTrack || heads is < MsaLayout.MinimumHeadCount or > MsaLayout.MaximumHeadCount || endCylinder < startCylinder || endCylinder > MsaLayout.MaximumCylinder) throw MsaExceptions.InvalidGeometry(sectors, heads, startCylinder, endCylinder);
        var trackBytes = checked(sectors * MsaLayout.SectorSize);
        var position = MsaLayout.HeaderSize;
        var blocks = new List<SectorBlock>();
        for (var cylinder = startCylinder; cylinder <= endCylinder; cylinder++)
        {
            for (var head = 0; head < heads; head++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (position + MsaLayout.TrackLengthFieldSize > source.Length) throw MsaExceptions.TruncatedTrackTable(cylinder, head, position, Math.Max(0, source.Length - position));
                var packedLength = ReadWord(source, position);
                position += MsaLayout.TrackLengthFieldSize;
                if (position + packedLength > source.Length) throw MsaExceptions.TruncatedTrack(cylinder, head, position, packedLength, Math.Max(0, source.Length - position));
                var track = packedLength == trackBytes ? source.AsSpan(position, packedLength).ToArray() : MsaRleDecoder.Unpack(source.AsSpan(position, packedLength), trackBytes, cylinder, head);
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

    /// <summary>Lit un entier non signé 16 bits big-endian à la position demandée.</summary>
    /// <param name="data">Données contenant l'entier.</param>
    /// <param name="offset">Position de l'entier, en octets.</param>
    /// <returns>La valeur convertie en entier signé positif.</returns>
    private static int ReadWord(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
}
