using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.Atr;

/// <summary>Valide un conteneur ATR et expose sa charge utile sous forme de secteurs Atari adressÃ©s.</summary>
public sealed class AtrReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[]
    {
        DiskImageFormatIds.Atari90,
        DiskImageFormatIds.Atari130,
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
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, Read(bytes, cancellationToken));
    }

    private static SectorImage Read(byte[] data, CancellationToken cancellationToken)
    {
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
        if (declaredPayloadLength != observedPayloadLength) throw AtrExceptions.PayloadLengthMismatch(observedPayloadLength, declaredPayloadLength);

        var bootAreaLength = AtrLayout.GetBootAreaLength(sectorSize);
        if (observedPayloadLength < bootAreaLength || (observedPayloadLength - bootAreaLength) % sectorSize != 0) throw AtrExceptions.TruncatedPayload(observedPayloadLength, bootAreaLength, sectorSize);
    }
}
