using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Reading;

namespace GWGUI.MediaEngine.Containers.ImageDisk;

/// <summary>Lit les images sectorielles Dave Dunfield ImageDisk.</summary>
public sealed class ImdReader : ISectorImageReader
{
    /// <summary>Indique si l'extension du chemin correspond à ImageDisk.</summary>
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Imd, StringComparison.OrdinalIgnoreCase);

    /// <summary>Lit un fichier ImageDisk et construit son image sectorielle.</summary>
    /// <param name="path">Chemin du fichier IMD.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>L'image sectorielle reconstruite.</returns>
    /// <exception cref="IOException">Une erreur d'entrée-sortie survient pendant la lecture.</exception>
    /// <exception cref="InvalidDataException">Une section ou une valeur ImageDisk est invalide.</exception>
    /// <exception cref="OverflowException">Un calcul de taille dépasse la capacité d'un entier.</exception>
    /// <exception cref="OperationCanceledException">L'opération est annulée.</exception>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return Read(data, cancellationToken);
    }

    /// <summary>Analyse les pistes et secteurs contenus dans une séquence ImageDisk.</summary>
    internal static SectorImage Read(ReadOnlySpan<byte> data, CancellationToken cancellationToken = default)
    {
        var offset = FindTrackDataOffset(data);
        var sectors = new List<ImdSector>();
        while (offset < data.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var header = ReadTrackHeader(data, ref offset);
            var numbers = ReadByteMap(data, ref offset, header.SectorCount, ImdSection.SectorNumberMap);
            var cylinders = header.HeadFlags.HasFlag(ImdHeadFlags.HasCylinderMap) ? ReadByteMap(data, ref offset, header.SectorCount, ImdSection.CylinderMap) : null;
            var heads = header.HeadFlags.HasFlag(ImdHeadFlags.HasHeadMap) ? ReadByteMap(data, ref offset, header.SectorCount, ImdSection.HeadMap) : null;
            var sizes = ReadSectorSizes(data, ref offset, header.SectorCount, header.SectorSizeCode);

            for (var index = 0; index < header.SectorCount; index++)
            {
                EnsureAvailable(data, offset, ImdLayout.MapEntrySize, ImdSection.SectorRecord);
                var rawRecordType = data[offset++];
                var recordType = (ImdSectorRecordType)rawRecordType;
                if (!Enum.IsDefined(recordType)) throw ImdExceptions.InvalidRecordType(rawRecordType);
                var bytes = ReadSectorRecord(data, ref offset, recordType, sizes[index]);
                sectors.Add(new(cylinders?[index] ?? header.Cylinder, (heads?[index] ?? header.Head) & (int)ImdHeadFlags.HeadMask, numbers[index], bytes, recordType.HasData(), recordType.IsIntegrityValid()));
            }
        }
        return BuildImage(sectors);
    }

    /// <summary>Valide la signature et retourne la position suivant le commentaire.</summary>
    private static int FindTrackDataOffset(ReadOnlySpan<byte> data)
    {
        var commentEnd = data.IndexOf(ImdFormat.CommentTerminator);
        if (commentEnd < ImdFormat.SignatureLength || !data[..ImdFormat.SignatureLength].SequenceEqual(ImdFormat.Signature)) throw ImdExceptions.MissingSignature(commentEnd, data.Length);
        return commentEnd + ImdLayout.MapEntrySize;
    }

    /// <summary>Lit et valide l'en-tête de piste situé à la position courante.</summary>
    private static ImdTrackHeader ReadTrackHeader(ReadOnlySpan<byte> data, ref int offset)
    {
        EnsureAvailable(data, offset, ImdLayout.TrackHeaderSize, ImdSection.TrackHeader);
        var header = data.Slice(offset, ImdLayout.TrackHeaderSize);
        offset += ImdLayout.TrackHeaderSize;
        var mode = (ImdMode)header[ImdLayout.ModeOffset];
        var sectorCount = header[ImdLayout.SectorCountOffset];
        if (!Enum.IsDefined(mode) || sectorCount == 0) throw ImdExceptions.InvalidTrackHeader(mode, sectorCount);
        var headFlags = (ImdHeadFlags)header[ImdLayout.HeadFlagsOffset];
        return new(mode, header[ImdLayout.CylinderOffset], headFlags, (int)(headFlags & ImdHeadFlags.HeadMask), sectorCount, header[ImdLayout.SectorSizeCodeOffset]);
    }

    /// <summary>Lit une carte d'octets et avance la position courante.</summary>
    private static byte[] ReadByteMap(ReadOnlySpan<byte> data, ref int offset, int count, ImdSection section)
    {
        var length = count * ImdLayout.MapEntrySize;
        EnsureAvailable(data, offset, length, section);
        var map = data.Slice(offset, length).ToArray();
        offset += length;
        return map;
    }

    /// <summary>Lit les tailles explicites ou développe le code exponentiel commun.</summary>
    private static int[] ReadSectorSizes(ReadOnlySpan<byte> data, ref int offset, int count, byte sizeCode)
    {
        if (sizeCode != ImdLayout.ExplicitSectorSizeCode)
        {
            if (sizeCode > ImdLayout.MaximumExponentialSizeCode) throw ImdExceptions.InvalidSizeCode(sizeCode);
            return Enumerable.Repeat(ImdLayout.BaseSectorSize << sizeCode, count).ToArray();
        }

        var length = count * ImdLayout.SectorSizeMapEntrySize;
        EnsureAvailable(data, offset, length, ImdSection.SectorSizeMap);
        var sizes = new int[count];
        for (var index = 0; index < count; index++) sizes[index] = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset + index * ImdLayout.SectorSizeMapEntrySize, ImdLayout.SectorSizeMapEntrySize));
        offset += length;
        return sizes;
    }

    /// <summary>Lit, décompresse ou matérialise la charge utile d'un enregistrement sectoriel.</summary>
    private static byte[] ReadSectorRecord(ReadOnlySpan<byte> data, ref int offset, ImdSectorRecordType recordType, int size)
    {
        if (!recordType.HasData()) return new byte[size];
        if (recordType.IsCompressed())
        {
            EnsureAvailable(data, offset, ImdLayout.MapEntrySize, ImdSection.CompressedValue);
            return Enumerable.Repeat(data[offset++], size).ToArray();
        }
        EnsureAvailable(data, offset, size, ImdSection.SectorData);
        var bytes = data.Slice(offset, size).ToArray();
        offset += size;
        return bytes;
    }

    /// <summary>Construit la géométrie, les blocs disponibles et les blocs absents de l'image sectorielle.</summary>
    private static SectorImage BuildImage(IReadOnlyList<ImdSector> sectors)
    {
        if (sectors.Count == 0) throw ImdExceptions.NoSectors();
        var blockSize = sectors.GroupBy(sector => sector.Data.Length).OrderByDescending(group => group.Count()).First().Key;
        var cylinders = sectors.Max(sector => sector.Cylinder) + 1;
        var heads = sectors.Max(sector => sector.Head) + 1;
        var sectorsPerTrack = sectors.GroupBy(sector => (sector.Cylinder, sector.Head)).Max(group => group.Count());
        var ordered = sectors.OrderBy(sector => sector.Cylinder).ThenBy(sector => sector.Head).ThenBy(sector => sector.Number).ToArray();
        var blocks = ordered.Select((sector, logical) => (sector, logical)).Where(item => item.sector.Available).Select(item => new SectorBlock(item.logical, new(item.sector.Cylinder, item.sector.Head, item.sector.Number), item.sector.Data, item.sector.IntegrityValid)).ToArray();
        var capacity = ordered.Sum(sector => (long)sector.Data.Length);
        var descriptors = sectors.Select(sector => new EpsonQx10SectorDescriptor(sector.Cylinder, sector.Head, sector.Number, sector.Data.Length)).ToArray();
        var formatId = EpsonQx10FormatDetector.TryDetect(descriptors, out var detectedFormat) ? detectedFormat : DiskImageFormatIds.Imd;
        return new(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks, sectors.Any(sector => sector.Data.Length != blockSize), capacity, ordered.Length);
    }

    /// <summary>Vérifie qu'une section complète est disponible à la position demandée.</summary>
    private static void EnsureAvailable(ReadOnlySpan<byte> data, int offset, int count, ImdSection section)
    {
        if (offset < 0 || count < 0 || offset > data.Length - count) throw ImdExceptions.TruncatedSection(section, offset, count, Math.Max(0, data.Length - offset));
    }

    /// <summary>Regroupe les champs validés d'un en-tête de piste ImageDisk.</summary>
    private readonly record struct ImdTrackHeader(ImdMode Mode, int Cylinder, ImdHeadFlags HeadFlags, int Head, int SectorCount, byte SectorSizeCode);

    /// <summary>Représente un secteur ImageDisk déclaré, disponible ou absent.</summary>
    private sealed record ImdSector(int Cylinder, int Head, int Number, byte[] Data, bool Available, bool IntegrityValid);
}
