using System.Buffers.Binary;
using System.Collections.Frozen;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaFileSystems.Contracts;

namespace GWGUI.MediaEngine.Images.Formats.Cartridge.AmstradCpr;

/// <summary>Reads RIFF AMS! cartridge pages using their declared cb00 through cb31 bank identifiers.</summary>
public sealed class AmstradCprReader : IMediaImageReader
{
    private const int HeaderLength = 12;
    private const int ChunkHeaderLength = 8;
    private const int BankLength = 16 * 1024;
    private const int MaximumBankCount = 32;
    private static readonly byte[] RiffSignature = System.Text.Encoding.ASCII.GetBytes("RIFF");
    private static readonly byte[] FormSignature = System.Text.Encoding.ASCII.GetBytes("AMS!");
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { DiskImageFormatIds.AmstradCpr }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new[] { DiskImageFileExtensions.Cpr }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Cartridge }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [RiffSignature];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentations;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        if (context.Length < HeaderLength || !SupportedExtensions.Contains(context.Extension)) return false;
        var header = await context.ReadHeaderAsync(HeaderLength, cancellationToken).ConfigureAwait(false);
        return header.Span[..4].SequenceEqual(RiffSignature) && header.Span.Slice(8, 4).SequenceEqual(FormSignature);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        await using var input = new FileStream(context.Source.PrimaryPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var header = new byte[HeaderLength];
        await input.ReadExactlyAsync(header, cancellationToken).ConfigureAwait(false);
        if (!header.AsSpan(0, 4).SequenceEqual(RiffSignature) || !header.AsSpan(8, 4).SequenceEqual(FormSignature))
            throw new InvalidDataException("The RIFF AMS! cartridge header is missing.");
        var riffEnd = checked(8L + BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4)));
        if (riffEnd > input.Length || riffEnd < HeaderLength) throw new InvalidDataException("The CPR RIFF length is invalid.");

        var banks = new Dictionary<int, Bank>();
        var diagnostics = new List<string>();
        while (input.Position + ChunkHeaderLength <= riffEnd)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var chunkHeader = new byte[ChunkHeaderLength];
            await input.ReadExactlyAsync(chunkHeader, cancellationToken).ConfigureAwait(false);
            var id = System.Text.Encoding.ASCII.GetString(chunkHeader, 0, 4);
            var length = BinaryPrimitives.ReadUInt32LittleEndian(chunkHeader.AsSpan(4));
            var dataOffset = input.Position;
            var paddedLength = checked((long)length + (length & 1));
            if (paddedLength > riffEnd - dataOffset) throw new InvalidDataException($"CPR chunk '{id}' exceeds the RIFF container.");
            if (TryParseBankId(id, out var bankNumber))
            {
                if (!banks.TryAdd(bankNumber, new Bank(bankNumber, id, dataOffset, checked((int)Math.Min(length, BankLength)))))
                    throw new InvalidDataException($"CPR bank '{id}' is duplicated.");
                if (length > BankLength) diagnostics.Add($"CPR bank '{id}' contains bytes beyond its 16 KiB page; they were not mapped.");
            }
            else
            {
                diagnostics.Add($"RIFF chunk '{id}' is not a CPR cartridge bank.");
            }
            input.Position = checked(dataOffset + paddedLength);
        }
        if (input.Position != riffEnd) throw new InvalidDataException("The CPR RIFF container ends inside a chunk header.");
        if (banks.Count == 0) throw new InvalidDataException("The CPR image contains no cb00 through cb31 cartridge bank.");

        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        var capacity = checked((banks.Keys.Max() + 1L) * BankLength);
        var ranges = banks.Values.OrderBy(bank => bank.Number)
            .Where(bank => bank.Length > 0)
            .Select(bank => new MediaDataRange(
                bank.Number * (long)BankLength,
                bank.Length,
                MediaDataRangeKind.Stored,
                source,
                bank.SourceOffset))
            .ToArray();
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["systemId"] = "amstrad",
            ["mediaRole"] = "cartridge",
            ["bankSize"] = BankLength.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["bankCount"] = banks.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        foreach (var bank in banks.Values)
        {
            metadata[$"bank.{bank.Number}.chunkId"] = bank.Id;
            metadata[$"bank.{bank.Number}.length"] = bank.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        var volume = new MediaVolumeDescriptor(0, capacity, MediaVolumeOrigins.DirectVolume, name: "Amstrad cartridge");
        return new MediaImageDocument(
            context.Source,
            DiskImageFormatIds.AmstradCpr,
            MediaKind.Cartridge,
            new BlockMediaImageRepresentation(capacity, 1, ranges),
            [volume],
            diagnostics,
            metadata);
    }

    private static bool TryParseBankId(string id, out int bankNumber)
    {
        bankNumber = -1;
        return id.Length == 4
            && id.StartsWith("cb", StringComparison.Ordinal)
            && int.TryParse(id.AsSpan(2), System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out bankNumber)
            && bankNumber is >= 0 and < MaximumBankCount;
    }

    private sealed record Bank(int Number, string Id, long SourceOffset, int Length);
}
