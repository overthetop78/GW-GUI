using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Reading;

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
        var sectorCount = AtrLayout.GetSectorCount(payloadLength, sectorSize);
        var blocks = new List<SectorBlock>(sectorCount);
        var offset = AtrLayout.HeaderSize;
        for (var sector = AtrLayout.FirstSectorNumber; sector <= sectorCount; sector++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var length = sector <= AtrLayout.BootSectorCount ? AtrLayout.BootSectorSize : sectorSize;
            var logicalIndex = sector - AtrLayout.FirstSectorNumber;
            blocks.Add(new(logicalIndex, new(logicalIndex, AtrLayout.LogicalHeadIndex, sector), data.AsSpan(offset, length).ToArray()));
            offset += length;
        }

        return new(AtrFormat.GetFormatId(sectorSize, sectorCount), sectorSize, sectorCount, AtrLayout.LogicalHeadCount, AtrLayout.LogicalSectorsPerCylinder, blocks, allowVariableBlockSize: sectorSize != AtrLayout.SingleDensitySectorSize, capacity: payloadLength);
    }

    /// <summary>Charge un conteneur ATR et vérifie son en-tête, ses longueurs et l'intégrité de ses limites sectorielles.</summary>
    /// <param name="path">Chemin du conteneur ATR.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Octets complets du conteneur validé, en-tête inclus.</returns>
    /// <exception cref="InvalidDataException">Le fichier ne respecte pas la disposition ATR attendue.</exception>
    internal static async Task<byte[]> ReadValidatedContainerAsync(string path, CancellationToken cancellationToken)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        ushort? observedSignature = data.Length >= sizeof(ushort) ? BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.SignatureOffset)) : null;
        if (data.Length < AtrLayout.HeaderSize || observedSignature != AtrFormat.Signature) throw AtrExceptions.InvalidHeader(data.Length, AtrLayout.HeaderSize, observedSignature, AtrFormat.Signature);

        var sectorSize = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.SectorSizeOffset));
        if (!AtrLayout.IsSupportedSectorSize(sectorSize)) throw AtrExceptions.UnsupportedSectorSize(sectorSize, AtrLayout.SingleDensitySectorSize, AtrLayout.DoubleDensitySectorSize, AtrLayout.ExtendedSectorSize);

        var paragraphCount = ((long)BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.ParagraphCountHighOffset)) << 16) | BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.ParagraphCountLowOffset));
        var declaredPayloadLength = paragraphCount * AtrLayout.ParagraphSize;
        var observedPayloadLength = data.Length - AtrLayout.HeaderSize;
        if (declaredPayloadLength != observedPayloadLength) throw AtrExceptions.PayloadLengthMismatch(observedPayloadLength, declaredPayloadLength);

        var bootAreaLength = AtrLayout.GetBootAreaLength(sectorSize);
        if (observedPayloadLength < bootAreaLength || (observedPayloadLength - bootAreaLength) % sectorSize != 0) throw AtrExceptions.TruncatedPayload(observedPayloadLength, bootAreaLength, sectorSize);
        return data;
    }
}
