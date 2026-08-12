using System.Buffers.Binary;
using GWGUI.MediaEngine.Containers.Apple.Nib;
using GWGUI.MediaEngine.Containers.Apple.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Apple.TwoImg;

/// <summary>
/// Lit un conteneur Apple 2IMG validé et transmet sa charge utile au lecteur correspondant à son ordre sectoriel.
/// </summary>
internal static class TwoImgReader
{
    /// <summary>
    /// Valide l’en-tête 2IMG, délimite sa charge utile puis lit une image DOS, ProDOS ou NIB.
    /// </summary>
    /// <param name="container">Octets complets du conteneur 2IMG, en-tête inclus.</param>
    /// <returns>L’image sectorielle reconstruite à partir de la charge utile du conteneur.</returns>
    /// <exception cref="InvalidDataException">
    /// L’en-tête est tronqué, la signature est invalide ou la plage de données déclarée sort du conteneur.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// La version du conteneur ou son format d’image n’est pas pris en charge.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Un offset ou une longueur 32 bits déclaré par l’en-tête ne peut pas être représenté par un entier signé.
    /// </exception>
    public static SectorImage Read(byte[] container)
    {
        var (headerLength, imageFormat, dataOffset, dataLength) = ReadHeader(container);
        ValidateDataRange(container.Length, headerLength, dataOffset, dataLength);
        return ReadPayload(container.AsMemory(dataOffset, dataLength), imageFormat);
    }

    /// <summary>Valide la signature et la version de l'en-tête, puis lit ses champs de routage.</summary>
    /// <param name="container">Contenu complet du conteneur.</param>
    /// <returns>Taille d'en-tête, format interne, offset et longueur de la charge utile.</returns>
    private static (int HeaderLength, TwoImgImageFormat ImageFormat, int DataOffset, int DataLength) ReadHeader(ReadOnlySpan<byte> container)
    {
        if (container.Length < TwoImgLayout.MinimumHeaderSize) throw TwoImgExceptions.TruncatedHeader();
        if (!container.Slice(TwoImgLayout.SignatureOffset, TwoImgLayout.SignatureLength).SequenceEqual(TwoImgFormat.SignatureBytes)) throw TwoImgExceptions.InvalidSignature();

        var version = BinaryPrimitives.ReadUInt16LittleEndian(container[TwoImgLayout.VersionOffset..]);
        if (version != TwoImgFormat.SupportedVersion) throw TwoImgExceptions.UnsupportedVersion(version);

        var headerLength = checked((int)BinaryPrimitives.ReadUInt16LittleEndian(container[TwoImgLayout.HeaderSizeOffset..]));
        var imageFormat = (TwoImgImageFormat)BinaryPrimitives.ReadUInt32LittleEndian(container[TwoImgLayout.ImageFormatOffset..]);
        var dataOffset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(container[TwoImgLayout.DataOffsetOffset..]));
        var dataLength = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(container[TwoImgLayout.DataLengthOffset..]));
        return (headerLength, imageFormat, dataOffset, dataLength);
    }

    /// <summary>Vérifie que la charge utile commence après l'en-tête et reste entièrement dans le conteneur.</summary>
    /// <param name="containerLength">Longueur totale du conteneur.</param>
    /// <param name="headerLength">Longueur d'en-tête déclarée.</param>
    /// <param name="dataOffset">Offset déclaré de la charge utile.</param>
    /// <param name="dataLength">Longueur déclarée de la charge utile.</param>
    private static void ValidateDataRange(int containerLength, int headerLength, int dataOffset, int dataLength)
    {
        if (headerLength < TwoImgLayout.MinimumHeaderSize || dataOffset < headerLength || dataLength < TwoImgLayout.MinimumDataLength || dataOffset > containerLength - dataLength) throw TwoImgExceptions.InvalidDataRange();
    }

    /// <summary>Route la charge utile vers le lecteur correspondant à son organisation DOS, ProDOS ou NIB.</summary>
    /// <param name="payload">Charge utile validée du conteneur.</param>
    /// <param name="imageFormat">Organisation déclarée par l'en-tête.</param>
    /// <returns>Image sectorielle reconstruite.</returns>
    private static SectorImage ReadPayload(ReadOnlyMemory<byte> payload, TwoImgImageFormat imageFormat)
    {
        return imageFormat switch
        {
            TwoImgImageFormat.Dos => AppleRawImageReader.Read(payload, DiskImageFileExtensions.Do).Image,
            TwoImgImageFormat.ProDos => AppleRawImageReader.Read(payload, DiskImageFileExtensions.Po).Image,
            TwoImgImageFormat.Nib => NibReader.Read(payload.Span),
            _ => throw TwoImgExceptions.UnsupportedImageFormat(imageFormat)
        };
    }
}
