using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Atari.Atr;

/// <summary>Écrit un conteneur ATR complet avec son en-tête et ses secteurs logiques.</summary>
public sealed class AtrWriter(GWGUI.MediaEngine.Containers.Storage.IAtomicImageFileWriter? fileSystem = null)
{
    private readonly GWGUI.MediaEngine.Containers.Storage.IAtomicImageFileWriter files = fileSystem ?? new GWGUI.MediaEngine.Containers.Storage.AtomicImageFileWriter();
    /// <summary>Valide le profil demandé et écrit tous les secteurs sans remplissage implicite.</summary>
    public async Task WriteAsync(SectorImage image, string path, string formatId, CancellationToken cancellationToken = default)
    {
        if (!AtrFormatCatalog.TryGet(formatId, out var profile)) throw AtrExceptions.UnsupportedFormat(formatId);
        if (!image.FormatId.Equals(profile.FormatId, StringComparison.OrdinalIgnoreCase) || image.BlockCount != profile.SectorCount || image.Capacity != profile.PayloadLength) throw AtrExceptions.IncompatibleSectorImage(image, profile);
        await files.WriteAsync(path, async (output, cancellationToken) =>
        {
        var header = new byte[AtrLayout.HeaderSize];
        var paragraphs = profile.PayloadLength / AtrLayout.ParagraphSize;
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(AtrLayout.SignatureOffset), AtrFormat.Signature);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(AtrLayout.ParagraphCountLowOffset), unchecked((ushort)paragraphs));
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(AtrLayout.SectorSizeOffset), checked((ushort)profile.SectorSize));
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(AtrLayout.ParagraphCountHighOffset), checked((ushort)(paragraphs >> AtrLayout.ParagraphCountHighWordShift)));
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        for (var logical = 0; logical < profile.SectorCount; logical++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!image.TryGetBlock(logical, out var block)) throw AtrExceptions.MissingSector(logical + AtrLayout.FirstSectorNumber);
            var expectedSize = profile.SectorSize == AtrLayout.DoubleDensitySectorSize && logical < AtrLayout.BootSectorCount ? AtrLayout.BootSectorSize : profile.SectorSize;
            if (block.Data.Count != expectedSize) throw AtrExceptions.InvalidSectorSize(logical + AtrLayout.FirstSectorNumber, block.Data.Count, expectedSize);
            await output.WriteAsync(block.Data.ToArray(), cancellationToken).ConfigureAwait(false);
        }
        }, cancellationToken).ConfigureAwait(false);
    }
}
