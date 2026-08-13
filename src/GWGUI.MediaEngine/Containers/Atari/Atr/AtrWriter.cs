using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Atari.Atr;

/// <summary>Écrit un conteneur ATR complet avec son en-tête et ses secteurs logiques.</summary>
public sealed class AtrWriter
{
    /// <summary>Valide le profil demandé et écrit tous les secteurs sans remplissage implicite.</summary>
    public async Task WriteAsync(SectorImage image, string path, string formatId, CancellationToken cancellationToken = default)
    {
        if (!AtrFormatCatalog.TryGet(formatId, out var profile)) throw AtrExceptions.UnsupportedFormat(formatId);
        if (!image.FormatId.Equals(profile.FormatId, StringComparison.OrdinalIgnoreCase) || image.BlockCount != profile.SectorCount || image.Capacity != profile.PayloadLength) throw AtrExceptions.IncompatibleSectorImage(image, profile);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var output = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, profile.SectorSize, FileOptions.Asynchronous))
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
            }
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
