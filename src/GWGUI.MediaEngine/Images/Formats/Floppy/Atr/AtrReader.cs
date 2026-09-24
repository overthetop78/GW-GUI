using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Atr;

/// <summary>Valide un conteneur ATR et expose sa charge utile sous forme de secteurs Atari adressÃ©s.</summary>
public sealed class AtrReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[]
    {
        DiskImageFormatIds.Atari90,
        DiskImageFormatIds.Atari130,
        DiskImageFormatIds.Atari140,
        DiskImageFormatIds.Atari180
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Atr }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> SupportedSignatures = [new byte[] { 0x96, 0x02 }];
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;
    public AtrReader() : this(File.ReadAllBytesAsync) { }
    internal AtrReader(Func<string, CancellationToken, Task<byte[]>> readBytes) => this.readBytes = readBytes;

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;

    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;

    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => SupportedSignatures;

    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;

    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;

    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;

    bool IMediaImageReader.SupportsFormatId(string formatId)
        => SupportedFormatIds.Contains(formatId) || formatId.StartsWith($"{DiskImageFormatIds.AtariPrefix}atr.", StringComparison.OrdinalIgnoreCase);
    /// <summary>Lit et valide un conteneur ATR, puis restitue tous ses secteurs dans leur ordre logique.</summary>
    /// <param name="path">Chemin du conteneur ATR.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle dont les tailles et adresses proviennent du conteneur.</returns>
    /// <exception cref="ArgumentException">Le chemin est vide ou prÃ©sente un format invalide.</exception>
    /// <exception cref="FileNotFoundException">Le fichier ATR est introuvable.</exception>
    /// <exception cref="DirectoryNotFoundException">Un rÃ©pertoire du chemin est introuvable.</exception>
    /// <exception cref="UnauthorizedAccessException">L'accÃ¨s au fichier ATR est refusÃ©.</exception>
    /// <exception cref="IOException">Une erreur d'entrÃ©e-sortie survient pendant la lecture.</exception>
    /// <exception cref="InvalidDataException">L'en-tÃªte, la taille dÃ©clarÃ©e ou la disposition des secteurs est invalide.</exception>
    /// <exception cref="OperationCanceledException">Le jeton d'annulation demande l'arrÃªt de la lecture.</exception>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await ReadValidatedContainerAsync(path, cancellationToken, readBytes).ConfigureAwait(false);
        return Read(data, cancellationToken);
    }

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !((IMediaImageReader)this).SupportsFormatId(context.RequestedFormatId)) return false;

        try
        {
            var data = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
            ValidateContainer(data.ToArray());
            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var data = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        var bytes = data.ToArray();
        ValidateContainer(bytes);
        var image = Read(bytes, cancellationToken);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, image);
    }

    private static SectorImage Read(byte[] data, CancellationToken cancellationToken)
    {
        var sectorSize = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.SectorSizeOffset));
        var payloadLength = GetUsablePayloadLength(data, sectorSize);
        var sectorCount = sectorSize == AtrLayout.SingleDensitySectorSize
            ? (payloadLength + sectorSize - 1) / sectorSize
            : AtrLayout.GetSectorCount(payloadLength, sectorSize);
        var geometry = AtrLayout.GetGeometry(sectorSize, sectorCount);
        var blocks = new List<SectorBlock>(sectorCount);
        var offset = AtrLayout.HeaderSize;
        for (var sector = AtrLayout.FirstSectorNumber; sector <= sectorCount; sector++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var length = sector <= AtrLayout.BootSectorCount ? AtrLayout.BootSectorSize : sectorSize;
            var logicalIndex = sector - AtrLayout.FirstSectorNumber;
            var cylinder = logicalIndex / geometry.SectorsPerTrack;
            var sectorInTrack = logicalIndex % geometry.SectorsPerTrack + AtrLayout.FirstSectorNumber;
            var sectorData = new byte[length];
            var availableLength = Math.Min(length, data.Length - offset);
            if (availableLength > 0) data.AsSpan(offset, availableLength).CopyTo(sectorData);
            blocks.Add(new(logicalIndex, new(cylinder, AtrLayout.LogicalHeadIndex, sectorInTrack), sectorData));
            offset += availableLength;
        }

        var isTruncatedSingleDensity = AtrLayout.IsTruncatedSingleDensity(sectorSize, sectorCount);
        if (isTruncatedSingleDensity)
        {
            for (var logicalIndex = sectorCount; logicalIndex < AtrLayout.StandardSectorCount; logicalIndex++)
            {
                var cylinder = logicalIndex / geometry.SectorsPerTrack;
                var sectorInTrack = logicalIndex % geometry.SectorsPerTrack + AtrLayout.FirstSectorNumber;
                blocks.Add(new(logicalIndex, new(cylinder, AtrLayout.LogicalHeadIndex, sectorInTrack), new byte[AtrLayout.SingleDensitySectorSize]));
            }
        }
        var capacity = isTruncatedSingleDensity
            ? (long)AtrLayout.StandardSectorCount * AtrLayout.SingleDensitySectorSize
            : payloadLength;
        var logicalBlockCount = isTruncatedSingleDensity ? AtrLayout.StandardSectorCount : sectorCount;
        return new(AtrFormat.GetFormatId(sectorSize, sectorCount), sectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks, allowVariableBlockSize: sectorSize != AtrLayout.SingleDensitySectorSize, capacity: capacity, logicalBlockCount: logicalBlockCount, addressingKind: geometry.AddressingKind);
    }

    /// <summary>Charge un conteneur ATR et vÃ©rifie son en-tÃªte, ses longueurs et l'intÃ©gritÃ© de ses limites sectorielles.</summary>
    /// <param name="path">Chemin du conteneur ATR.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Octets complets du conteneur validÃ©, en-tÃªte inclus.</returns>
    /// <exception cref="ArgumentException">Le chemin est vide ou prÃ©sente un format invalide.</exception>
    /// <exception cref="FileNotFoundException">Le fichier ATR est introuvable.</exception>
    /// <exception cref="DirectoryNotFoundException">Un rÃ©pertoire du chemin est introuvable.</exception>
    /// <exception cref="UnauthorizedAccessException">L'accÃ¨s au fichier ATR est refusÃ©.</exception>
    /// <exception cref="IOException">Une erreur d'entrÃ©e-sortie survient pendant la lecture.</exception>
    /// <exception cref="InvalidDataException">Le fichier ne respecte pas la disposition ATR attendue.</exception>
    /// <exception cref="OperationCanceledException">Le jeton d'annulation demande l'arrÃªt de la lecture.</exception>
    internal static async Task<byte[]> ReadValidatedContainerAsync(string path, CancellationToken cancellationToken,
        Func<string, CancellationToken, Task<byte[]>>? readBytes = null)
    {
        var data = await (readBytes ?? File.ReadAllBytesAsync)(path, cancellationToken).ConfigureAwait(false);
        ValidateContainer(data);
        return data;
    }

    private static void ValidateContainer(byte[] data)
    {
        ushort? observedSignature = data.Length >= sizeof(ushort) ? BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.SignatureOffset)) : null;
        if (data.Length < AtrLayout.HeaderSize || observedSignature != AtrFormat.Signature) throw AtrExceptions.InvalidHeader(data.Length, AtrLayout.HeaderSize, observedSignature, AtrFormat.Signature);

        var sectorSize = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.SectorSizeOffset));
        if (!AtrLayout.IsSupportedSectorSize(sectorSize)) throw AtrExceptions.UnsupportedSectorSize(sectorSize, AtrLayout.SingleDensitySectorSize, AtrLayout.DoubleDensitySectorSize, AtrLayout.ExtendedSectorSize);

        var paragraphCount = ((long)BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.ParagraphCountHighOffset)) << 16) | BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.ParagraphCountLowOffset));
        var declaredPayloadLength = paragraphCount * AtrLayout.ParagraphSize;
        var observedPayloadLength = data.Length - AtrLayout.HeaderSize;
        var usablePayloadLength = GetUsablePayloadLength(data, sectorSize);
        var isRecoverableTruncatedImage = declaredPayloadLength != observedPayloadLength
            && (LooksLikeTruncatedSingleDensityBootImage(data, sectorSize, declaredPayloadLength, observedPayloadLength)
                || LooksLikeMislabeledEnhancedDensityImage(data, sectorSize, declaredPayloadLength, observedPayloadLength)
                || LooksLikeHeaderInclusivePayloadLength(sectorSize, declaredPayloadLength, observedPayloadLength));
        var isExternallyPaddedImage = declaredPayloadLength != observedPayloadLength
            && LooksLikeExternallyPaddedImage(data, sectorSize, declaredPayloadLength, observedPayloadLength);
        if (declaredPayloadLength != observedPayloadLength && !isRecoverableTruncatedImage && !isExternallyPaddedImage)
            throw AtrExceptions.PayloadLengthMismatch(observedPayloadLength, declaredPayloadLength);

        var bootAreaLength = AtrLayout.GetBootAreaLength(sectorSize);
        if (!isRecoverableTruncatedImage &&
            (usablePayloadLength < bootAreaLength || (usablePayloadLength - bootAreaLength) % sectorSize != 0))
            throw AtrExceptions.TruncatedPayload(observedPayloadLength, bootAreaLength, sectorSize);
    }

    private static int GetUsablePayloadLength(byte[] data, int sectorSize)
    {
        var observedPayloadLength = data.Length - AtrLayout.HeaderSize;
        var paragraphCount = ((long)BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.ParagraphCountHighOffset)) << 16)
            | BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AtrLayout.ParagraphCountLowOffset));
        var declaredPayloadLength = paragraphCount * AtrLayout.ParagraphSize;
        var bootAreaLength = AtrLayout.GetBootAreaLength(sectorSize);
        if (LooksLikeHeaderInclusivePayloadLength(sectorSize, declaredPayloadLength, observedPayloadLength))
            return (int)declaredPayloadLength;

        if (declaredPayloadLength is > 0 and <= int.MaxValue
            && declaredPayloadLength < observedPayloadLength
            && declaredPayloadLength >= bootAreaLength
            && (declaredPayloadLength - bootAreaLength) % sectorSize == 0
            && IsExternalPadding(data.AsSpan(AtrLayout.HeaderSize + (int)declaredPayloadLength)))
            return (int)declaredPayloadLength;

        if (sectorSize != AtrLayout.DoubleDensitySectorSize ||
            observedPayloadLength != AtrLayout.StandardSectorCount * sectorSize)
            return observedPayloadLength;

        var standardPayloadLength = AtrLayout.GetBootAreaLength(sectorSize)
            + (AtrLayout.StandardSectorCount - AtrLayout.BootSectorCount) * sectorSize;
        return data.AsSpan(AtrLayout.HeaderSize + standardPayloadLength).IndexOfAnyExcept((byte)0) < 0
            ? standardPayloadLength
            : observedPayloadLength;
    }

    private static bool LooksLikeTruncatedSingleDensityBootImage(byte[] data, int sectorSize, long declaredPayloadLength, int observedPayloadLength)
    {
        if (sectorSize != AtrLayout.SingleDensitySectorSize ||
            observedPayloadLength < AtrLayout.SingleDensitySectorSize ||
            observedPayloadLength >= AtrLayout.StandardSectorCount * AtrLayout.SingleDensitySectorSize ||
            declaredPayloadLength <= observedPayloadLength ||
            declaredPayloadLength > AtrLayout.StandardSectorCount * AtrLayout.SingleDensitySectorSize)
            return false;

        var bootSectorCount = data[AtrLayout.HeaderSize + 1];
        return bootSectorCount > 0 && bootSectorCount * AtrLayout.SingleDensitySectorSize <= observedPayloadLength;
    }

    private static bool LooksLikeMislabeledEnhancedDensityImage(byte[] data, int sectorSize, long declaredPayloadLength, int observedPayloadLength)
    {
        if (sectorSize != AtrLayout.SingleDensitySectorSize ||
            observedPayloadLength != AtrLayout.EnhancedDensitySectorCount * AtrLayout.SingleDensitySectorSize ||
            declaredPayloadLength != AtrLayout.ExtendedSingleDensitySectorCount * AtrLayout.SingleDensitySectorSize)
            return false;

        const int vtocSectorNumber = 360;
        const ushort enhancedDensityUsableSectorCount = 1010;
        const ushort xfPlusManagedSectorCount = 1027;
        var vtocOffset = AtrLayout.HeaderSize + (vtocSectorNumber - AtrLayout.FirstSectorNumber) * AtrLayout.SingleDensitySectorSize;
        var directoryOffset = vtocOffset + AtrLayout.SingleDensitySectorSize;
        if (data.Length < directoryOffset + 1 || (data[directoryOffset] & 0x40) == 0) return false;
        var managedSectorCount = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(vtocOffset + 1));
        return data[vtocOffset] == 2 && managedSectorCount == enhancedDensityUsableSectorCount
            || data[vtocOffset] == 3 && managedSectorCount == xfPlusManagedSectorCount;
    }

    private static bool LooksLikeExternallyPaddedImage(byte[] data, int sectorSize, long declaredPayloadLength, int observedPayloadLength)
    {
        var bootAreaLength = AtrLayout.GetBootAreaLength(sectorSize);
        if (declaredPayloadLength <= 0 || declaredPayloadLength >= observedPayloadLength
            || declaredPayloadLength > int.MaxValue
            || declaredPayloadLength < bootAreaLength
            || (declaredPayloadLength - bootAreaLength) % sectorSize != 0)
            return false;
        return IsExternalPadding(data.AsSpan(AtrLayout.HeaderSize + (int)declaredPayloadLength));
    }

    private static bool LooksLikeHeaderInclusivePayloadLength(
        int sectorSize,
        long declaredPayloadLength,
        int observedPayloadLength)
    {
        var bootAreaLength = AtrLayout.GetBootAreaLength(sectorSize);
        return declaredPayloadLength == observedPayloadLength + AtrLayout.HeaderSize
            && declaredPayloadLength <= int.MaxValue
            && declaredPayloadLength >= bootAreaLength
            && (declaredPayloadLength - bootAreaLength) % sectorSize == 0;
    }

    private static bool IsExternalPadding(ReadOnlySpan<byte> padding)
        => padding.Length > 0
            && (padding.IndexOfAnyExcept((byte)0) < 0
                || padding.IndexOfAnyExcept((byte)0x1A) < 0);
}
