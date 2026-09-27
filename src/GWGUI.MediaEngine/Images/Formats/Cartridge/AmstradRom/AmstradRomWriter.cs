using System.Collections.Frozen;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;

namespace GWGUI.MediaEngine.Images.Formats.Cartridge.AmstradRom;

/// <summary>Writes contiguous CPC cartridge banks as a headerless ROM or BIN dump.</summary>
public sealed class AmstradRomWriter : IMediaImageWriter
{
    private const int BankLength = 16 * 1024;
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { DiskImageFormatIds.AmstradRom }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new[] { DiskImageFileExtensions.Rom, DiskImageFileExtensions.Bin }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public AmstradRomWriter(IAtomicImageFileWriter? files = null) => this.files = files ?? new AtomicImageFileWriter();

    public string Id => MediaImageWriterIds.AmstradRom;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;
    public IReadOnlySet<string> ProducedFileExtensions => SupportedExtensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension) =>
        SupportedFormatIds.Contains(targetFormatId)
        && SupportedExtensions.Contains(targetExtension)
        && TryGetBanks(document, out _);

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        if (!CanWrite(document, targetFormatId, Path.GetExtension(outputPath).ToLowerInvariant())
            || !TryGetBanks(document, out var banks))
            throw new InvalidDataException("The block document does not contain contiguous CPC ROM banks.");
        await files.WriteAsync(outputPath, (output, token) => WriteBanksAsync(output, banks, token), cancellationToken)
            .ConfigureAwait(false);
        return [outputPath];
    }

    private static async Task WriteBanksAsync(Stream output, IReadOnlyList<MediaDataRange> banks, CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        foreach (var bank in banks)
        {
            var completed = 0L;
            while (completed < bank.Length)
            {
                var count = (int)Math.Min(buffer.Length, bank.Length - completed);
                await bank.Source!.ReadExactlyAsync(bank.SourceOffset + completed, buffer.AsMemory(0, count), cancellationToken)
                    .ConfigureAwait(false);
                await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
                completed += count;
            }
        }
    }

    private static bool TryGetBanks(MediaImageDocument document, out IReadOnlyList<MediaDataRange> banks)
    {
        banks = [];
        if (document.Representation is not BlockMediaImageRepresentation { Ranges.Count: > 0 } blocks) return false;
        var ordered = blocks.Ranges.OrderBy(range => range.Address).ToArray();
        for (var index = 0; index < ordered.Length; index++)
        {
            var range = ordered[index];
            if (range.Kind != MediaDataRangeKind.Stored || range.Source is null
                || range.Address != index * (long)BankLength
                || range.Length <= 0 || range.Length > BankLength
                || index < ordered.Length - 1 && range.Length != BankLength)
                return false;
        }
        banks = ordered;
        return true;
    }
}
