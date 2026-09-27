using System.Buffers.Binary;
using System.Collections.Frozen;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;

namespace GWGUI.MediaEngine.Images.Formats.Cartridge.AmstradCpr;

/// <summary>Writes mapped 16 KiB cartridge pages as ordered RIFF AMS! cbNN chunks.</summary>
public sealed class AmstradCprWriter : IMediaImageWriter
{
    private const int BankLength = 16 * 1024;
    private const int MaximumBankCount = 32;
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { DiskImageFormatIds.AmstradCpr }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new[] { DiskImageFileExtensions.Cpr }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public AmstradCprWriter(IAtomicImageFileWriter? files = null) => this.files = files ?? new AtomicImageFileWriter();

    public string Id => MediaImageWriterIds.AmstradCpr;
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
            throw new InvalidDataException("The block document cannot be written as a complete CPR cartridge.");
        await files.WriteAsync(outputPath, (output, token) => WriteCprAsync(output, banks, token), cancellationToken)
            .ConfigureAwait(false);
        return [outputPath];
    }

    private static async Task WriteCprAsync(Stream output, IReadOnlyList<Bank> banks, CancellationToken cancellationToken)
    {
        var riffSize = checked(4 + banks.Sum(bank => 8 + bank.Length + (bank.Length & 1)));
        var header = new byte[12];
        System.Text.Encoding.ASCII.GetBytes("RIFF", header);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(4), checked((uint)riffSize));
        System.Text.Encoding.ASCII.GetBytes("AMS!", header.AsSpan(8));
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        var buffer = new byte[64 * 1024];
        foreach (var bank in banks)
        {
            var chunkHeader = new byte[8];
            System.Text.Encoding.ASCII.GetBytes($"cb{bank.Number:D2}", chunkHeader);
            BinaryPrimitives.WriteUInt32LittleEndian(chunkHeader.AsSpan(4), checked((uint)bank.Length));
            await output.WriteAsync(chunkHeader, cancellationToken).ConfigureAwait(false);
            var completed = 0L;
            while (completed < bank.Length)
            {
                var count = (int)Math.Min(buffer.Length, bank.Length - completed);
                await bank.Range.Source!.ReadExactlyAsync(bank.Range.SourceOffset + completed, buffer.AsMemory(0, count), cancellationToken)
                    .ConfigureAwait(false);
                await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
                completed += count;
            }
            if ((bank.Length & 1) != 0) await output.WriteAsync(new byte[1], cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool TryGetBanks(MediaImageDocument document, out IReadOnlyList<Bank> banks)
    {
        banks = [];
        if (document.Representation is not BlockMediaImageRepresentation blocks
            || blocks.Capacity <= 0
            || blocks.Capacity > MaximumBankCount * (long)BankLength)
            return false;
        var selected = new List<Bank>();
        foreach (var range in blocks.Ranges)
        {
            if (range.Kind != MediaDataRangeKind.Stored || range.Source is null
                || range.Address % BankLength != 0 || range.Length is <= 0 or > BankLength)
                return false;
            var number = checked((int)(range.Address / BankLength));
            var length = document.Metadata.TryGetValue($"bank.{number}.length", out var text)
                && int.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var storedLength)
                ? storedLength : checked((int)range.Length);
            if (length <= 0 || length > range.Length) return false;
            selected.Add(new Bank(number, length, range));
        }
        if (selected.Count == 0 || selected.Select(bank => bank.Number).Distinct().Count() != selected.Count) return false;
        banks = selected.OrderBy(bank => bank.Number).ToArray();
        return true;
    }

    private sealed record Bank(int Number, int Length, MediaDataRange Range);
}
