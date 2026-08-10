using System.Buffers.Binary;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Atari.Atr;

/// <summary>Valide un conteneur ATR et expose sa charge utile sous forme de secteurs Atari adressés.</summary>
public sealed class AtrReader : ISectorImageReader
{
    /// <summary>Indique si le chemin porte l'extension utilisée par les conteneurs ATR.</summary>
    /// <param name="path">Chemin du fichier à examiner.</param>
    /// <returns><see langword="true"/> lorsque l'extension est ATR ; sinon <see langword="false"/>.</returns>
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.Atr, StringComparison.OrdinalIgnoreCase);

    /// <summary>Lit et valide un conteneur ATR, puis restitue tous ses secteurs dans leur ordre logique.</summary>
    /// <param name="path">Chemin du conteneur ATR.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle dont les tailles et adresses proviennent du conteneur.</returns>
    /// <exception cref="InvalidDataException">L'en-tête, la taille déclarée ou la disposition des secteurs est invalide.</exception>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await ReadValidatedContainerAsync(path, cancellationToken).ConfigureAwait(false);
        var sectorSize = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.SectorSizeOffset));
        var payloadLength = data.Length - AtrLayout.HeaderSize;
        var bootAreaLength = sectorSize == AtrLayout.SingleDensitySectorSize
            ? 0
            : AtrLayout.BootSectorCount * AtrLayout.BootSectorSize;
        var sectorCount = (sectorSize == AtrLayout.SingleDensitySectorSize ? 0 : AtrLayout.BootSectorCount)
            + (payloadLength - bootAreaLength) / sectorSize;
        var blocks = new List<SectorBlock>(sectorCount);
        var offset = AtrLayout.HeaderSize;
        for (var sector = 1; sector <= sectorCount; sector++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var length = sector <= AtrLayout.BootSectorCount ? AtrLayout.BootSectorSize : sectorSize;
            blocks.Add(new(sector - 1, new(sector - 1, 0, sector), data.AsSpan(offset, length).ToArray()));
            offset += length;
        }

        return new(
            AtrFormat.GetFormatId(sectorSize, sectorCount),
            sectorSize,
            sectorCount,
            1,
            1,
            blocks,
            allowVariableBlockSize: sectorSize != AtrLayout.SingleDensitySectorSize,
            capacity: payloadLength);
    }

    /// <summary>Charge un conteneur ATR et vérifie son en-tête, ses longueurs et l'intégrité de ses limites sectorielles.</summary>
    /// <param name="path">Chemin du conteneur ATR.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Octets complets du conteneur validé, en-tête inclus.</returns>
    /// <exception cref="InvalidDataException">Le fichier ne respecte pas la disposition ATR attendue.</exception>
    internal static async Task<byte[]> ReadValidatedContainerAsync(string path, CancellationToken cancellationToken)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        ushort? observedSignature = data.Length >= sizeof(ushort)
            ? BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.SignatureOffset))
            : null;
        if (data.Length < AtrLayout.HeaderSize || observedSignature != AtrFormat.Signature)
            throw AtrExceptions.InvalidHeader(data.Length, AtrLayout.HeaderSize, observedSignature, AtrFormat.Signature);

        var sectorSize = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.SectorSizeOffset));
        if (!AtrLayout.IsSupportedSectorSize(sectorSize))
            throw AtrExceptions.UnsupportedSectorSize(sectorSize, AtrLayout.SingleDensitySectorSize, AtrLayout.DoubleDensitySectorSize, AtrLayout.ExtendedSectorSize);

        var paragraphCount = ((long)BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.ParagraphCountHighOffset)) << 16)
            | BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.ParagraphCountLowOffset));
        var declaredPayloadLength = paragraphCount * AtrLayout.ParagraphSize;
        var observedPayloadLength = data.Length - AtrLayout.HeaderSize;
        if (declaredPayloadLength != observedPayloadLength)
            throw AtrExceptions.PayloadLengthMismatch(observedPayloadLength, declaredPayloadLength);

        var bootAreaLength = sectorSize == AtrLayout.SingleDensitySectorSize
            ? 0
            : AtrLayout.BootSectorCount * AtrLayout.BootSectorSize;
        if (observedPayloadLength < bootAreaLength || (observedPayloadLength - bootAreaLength) % sectorSize != 0)
            throw AtrExceptions.TruncatedPayload(observedPayloadLength, bootAreaLength, sectorSize);
        return data;
    }
}
