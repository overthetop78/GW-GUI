using System.Buffers.Binary;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Recognition.Apple;
using GWGUI.MediaEngine.Recognition.Definitions;
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
        if (container.Length < TwoImgLayout.MinimumHeaderSize)
            throw TwoImgExceptions.TruncatedHeader();
        if (!container.AsSpan(TwoImgLayout.SignatureOffset, TwoImgLayout.SignatureLength)
                .SequenceEqual(TwoImgFormat.SignatureBytes))
            throw TwoImgExceptions.InvalidSignature();

        var version = BinaryPrimitives.ReadUInt16LittleEndian(container.AsSpan(TwoImgLayout.VersionOffset));
        if (version != TwoImgFormat.SupportedVersion)
            throw TwoImgExceptions.UnsupportedVersion(version);

        var headerLength = checked((int)BinaryPrimitives.ReadUInt16LittleEndian(container.AsSpan(TwoImgLayout.HeaderSizeOffset)));
        var imageFormat = (TwoImgImageFormat)BinaryPrimitives.ReadUInt32LittleEndian(container.AsSpan(TwoImgLayout.ImageFormatOffset));
        var dataOffset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(container.AsSpan(TwoImgLayout.DataOffsetOffset)));
        var dataLength = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(container.AsSpan(TwoImgLayout.DataLengthOffset)));
        if (headerLength < TwoImgLayout.MinimumHeaderSize || dataOffset < headerLength || dataLength <= 0 ||
            dataOffset > container.Length - dataLength)
            throw TwoImgExceptions.InvalidDataRange();

        if (imageFormat == TwoImgImageFormat.NIB)
            return NibTrackImageReader.Read(container.AsSpan(dataOffset, dataLength));
        if (imageFormat is not TwoImgImageFormat.DOS and not TwoImgImageFormat.ProDOS)
            throw TwoImgExceptions.UnsupportedImageFormat(imageFormat);

        return AppleRawImageReader.Read(container.AsSpan(dataOffset, dataLength).ToArray(),
            imageFormat == TwoImgImageFormat.DOS
                ? DiskImageFileExtensions.Do
                : DiskImageFileExtensions.Po);
    }
}
