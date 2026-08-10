using System.Buffers.Binary;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Apple.DiskCopy;

/// <summary>
/// Lit un conteneur Apple DiskCopy 4.2, valide ses plages et checksums, puis reconstruit son image sectorielle.
/// </summary>
internal static class DiskCopyReader
{
    /// <summary>
    /// Extrait les données et tags d’un conteneur DiskCopy, vérifie leur intégrité et détermine leur géométrie Apple.
    /// </summary>
    /// <param name="container">Octets complets du conteneur DiskCopy, en-tête inclus.</param>
    /// <returns>L’image sectorielle reconstruite, avec les tags associés lorsqu’ils sont présents.</returns>
    /// <exception cref="InvalidDataException">
    /// L’en-tête ou la charge utile est invalide, un checksum présent ne correspond pas aux données,
    /// ou la combinaison des données et tags n’est pas reconnue.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Une longueur 32 bits déclarée par l’en-tête ne peut pas être représentée par un entier signé.
    /// </exception>
    public static SectorImage Read(byte[] container)
    {
        if (container.Length < DiskCopyLayout.HeaderSize)
            throw DiskCopyExceptions.TruncatedHeader();
        var privateWord = BinaryPrimitives.ReadUInt16BigEndian(container.AsSpan(DiskCopyLayout.PrivateWordOffset));
        if (privateWord != DiskCopyFormat.PrivateWord)
            throw DiskCopyExceptions.InvalidPrivateWord(privateWord, DiskCopyFormat.PrivateWord);
        var dataLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(container.AsSpan(DiskCopyLayout.DataLengthOffset)));
        var tagLength = checked((int)BinaryPrimitives.ReadUInt32BigEndian(container.AsSpan(DiskCopyLayout.TagLengthOffset)));
        if (dataLength <= 0 || DiskCopyLayout.HeaderSize + (long)dataLength + tagLength > container.Length)
            throw DiskCopyExceptions.InvalidPayload();

        var payload = container.AsSpan(DiskCopyLayout.HeaderSize, dataLength).ToArray();
        var tags = container.AsSpan(DiskCopyLayout.HeaderSize + dataLength, tagLength);
        ValidateChecksums(container, payload, tags);

        if (AppleDiskImageSignatures.LooksLikeMac(payload) ||
            AppleDiskImageSignatures.LooksLikeProDos(payload))
            return AppleRawImageReader.Read(payload, DiskImageFileExtensions.Image);

        if (tagLength != dataLength / DiskCopyLayout.DataBlockSize * DiskCopyLayout.TagSizePerBlock)
            throw DiskCopyExceptions.UnrecognizedDataAndTags();

        var blocks = new SectorBlock[dataLength / DiskCopyLayout.DataBlockSize];
        for (var logical = 0; logical < blocks.Length; logical++)
        {
            var address = blocks.Length == AppleDiskGeometry.LisaFileWareBlockCount
                ? AppleDiskGeometry.LisaFileWareAddress(logical)
                : blocks.Length == AppleDiskGeometry.Macintosh400KBlockCount
                    ? AppleDiskGeometry.AppleMacZonedAddress(logical, AppleDiskGeometry.Macintosh400KHeadCount)
                    : new SectorAddress(logical / AppleDiskGeometry.GenericTaggedImageSectorsPerTrack, 0, logical % AppleDiskGeometry.GenericTaggedImageSectorsPerTrack);
            blocks[logical] = new(logical, address,
                payload.AsSpan(logical * DiskCopyLayout.DataBlockSize, DiskCopyLayout.DataBlockSize).ToArray(),
                Tag: tags.Slice(logical * DiskCopyLayout.TagSizePerBlock, DiskCopyLayout.TagSizePerBlock).ToArray());
        }
        var prebootSearchOffset = DiskCopyLayout.PrebootSearchBlockIndex * DiskCopyLayout.DataBlockSize;
        var formatId = payload.Length >= prebootSearchOffset + DiskCopyLayout.PrebootSearchLength &&
                       payload.AsSpan(prebootSearchOffset, DiskCopyLayout.PrebootSearchLength)
                           .IndexOf(DiskCopyFormat.PrebootMarker) >= 0
            ? DiskImageFormatIds.AppleLisaMacWorks
            : DiskImageFormatIds.AppleLisaOffice;
        var fileWare = blocks.Length == AppleDiskGeometry.LisaFileWareBlockCount;
        return new(formatId, DiskCopyLayout.DataBlockSize,
            fileWare
                ? AppleDiskGeometry.LisaFileWareCylinderCount
                : blocks.Length == AppleDiskGeometry.Macintosh400KBlockCount
                    ? AppleDiskGeometry.MacintoshCylinderCount
                    : Math.Max(AppleDiskGeometry.MinimumCylinderCount, blocks.Length / AppleDiskGeometry.GenericTaggedImageSectorsPerTrack),
            fileWare ? AppleDiskGeometry.LisaFileWareHeadCount : AppleDiskGeometry.GenericTaggedImageHeadCount,
            fileWare
                ? AppleDiskGeometry.LisaFileWareMaximumSectorsPerTrack
                : blocks.Length == AppleDiskGeometry.Macintosh400KBlockCount
                    ? AppleDiskGeometry.MacintoshMaximumSectorsPerTrack
                    : AppleDiskGeometry.GenericTaggedImageSectorsPerTrack,
            blocks,
            capacity: dataLength, logicalBlockCount: blocks.Length);
    }

    /// <summary>Indique si un contenu possède l'en-tête minimal et le mot magique DiskCopy 4.2.</summary>
    /// <param name="container">Contenu complet ou partiel à examiner.</param>
    /// <returns><see langword="true"/> lorsque le mot magique est présent à son offset ; sinon <see langword="false"/>.</returns>
    public static bool HasPrivateWord(ReadOnlySpan<byte> container) =>
        container.Length >= DiskCopyLayout.HeaderSize &&
        BinaryPrimitives.ReadUInt16BigEndian(container[DiskCopyLayout.PrivateWordOffset..]) == DiskCopyFormat.PrivateWord;

    /// <summary>
    /// Compare les checksums non nuls de l’en-tête avec ceux des données et des tags extraits.
    /// Le premier tag DiskCopy est exclu du calcul du checksum des tags conformément au format.
    /// </summary>
    /// <param name="container">Conteneur contenant les checksums stockés dans son en-tête.</param>
    /// <param name="payload">Données sectorielles dont le checksum doit être vérifié.</param>
    /// <param name="tags">Tags sectoriels dont le checksum doit être vérifié.</param>
    /// <exception cref="InvalidDataException">Un checksum présent est invalide ou sa plage de tags est incomplète.</exception>
    private static void ValidateChecksums(ReadOnlySpan<byte> container, ReadOnlySpan<byte> payload, ReadOnlySpan<byte> tags)
    {
        var storedDataChecksum = BinaryPrimitives.ReadUInt32BigEndian(container.Slice(DiskCopyLayout.DataChecksumOffset));
        if (storedDataChecksum != DiskCopyFormat.MissingChecksum)
        {
            var calculatedDataChecksum = CalculateChecksum(payload);
            if (storedDataChecksum != calculatedDataChecksum)
                throw DiskCopyExceptions.InvalidDataChecksum(storedDataChecksum, calculatedDataChecksum);
        }

        var storedTagChecksum = BinaryPrimitives.ReadUInt32BigEndian(container.Slice(DiskCopyLayout.TagChecksumOffset));
        if (storedTagChecksum == DiskCopyFormat.MissingChecksum)
            return;
        if (tags.Length < DiskCopyLayout.TagChecksumExcludedPrefixSize)
            throw DiskCopyExceptions.InvalidTagChecksum(storedTagChecksum, DiskCopyFormat.MissingChecksum);

        var calculatedTagChecksum = CalculateChecksum(tags[DiskCopyLayout.TagChecksumExcludedPrefixSize..]);
        if (storedTagChecksum != calculatedTagChecksum)
            throw DiskCopyExceptions.InvalidTagChecksum(storedTagChecksum, calculatedTagChecksum);
    }

    /// <summary>
    /// Calcule le checksum DiskCopy en additionnant chaque mot 16 bits big-endian,
    /// puis en effectuant une rotation circulaire d’un bit vers la droite après chaque addition.
    /// </summary>
    /// <param name="data">Séquence de longueur paire sur laquelle calculer le checksum.</param>
    /// <returns>Checksum DiskCopy 32 bits de la séquence.</returns>
    /// <exception cref="ArgumentException">La séquence contient un nombre impair d’octets.</exception>
    internal static uint CalculateChecksum(ReadOnlySpan<byte> data)
    {
        const int wordSize = sizeof(ushort);
        const int rotation = 1;
        if (data.Length % wordSize != 0)
            throw DiskCopyExceptions.InvalidChecksumByteCount(data.Length, nameof(data));

        uint checksum = 0;
        for (var offset = 0; offset < data.Length; offset += wordSize)
        {
            var word = BinaryPrimitives.ReadUInt16BigEndian(data[offset..]);
            checksum = unchecked(checksum + word);
            checksum = checksum >> rotation | checksum << (sizeof(uint) * 8 - rotation);
        }

        return checksum;
    }
}
