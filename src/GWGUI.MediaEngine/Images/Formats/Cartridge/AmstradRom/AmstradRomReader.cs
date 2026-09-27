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

namespace GWGUI.MediaEngine.Images.Formats.Cartridge.AmstradRom;

/// <summary>Maps raw CPC ROM and cartridge dumps into their physical 16 KiB banks.</summary>
public sealed class AmstradRomReader : IMediaImageReader
{
    private const int BankLength = 16 * 1024;
    private const int OptionalHeaderLength = 128;
    private static readonly IReadOnlySet<long> CorpusLengths = new long[]
    {
        8192, 12101, 16383, 16384, 16512, 32768, 131072, 524288
    }.ToFrozenSet();
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { DiskImageFormatIds.AmstradRom }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new[] { DiskImageFileExtensions.Rom, DiskImageFileExtensions.Bin }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Cartridge }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentations;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!SupportedExtensions.Contains(context.Extension) || !CorpusLengths.Contains(context.Length)) return false;
        if (context.RequestedFormatId is not null) return SupportedFormatIds.Contains(context.RequestedFormatId);
        if (!string.Equals(context.Extension, DiskImageFileExtensions.Rom, StringComparison.OrdinalIgnoreCase)) return false;

        if (context.Length == BankLength + OptionalHeaderLength)
        {
            var header = await context.ReadHeaderAsync(OptionalHeaderLength, cancellationToken).ConfigureAwait(false);
            return HasValidAmsdosHeader(header.Span);
        }
        if (context.Length == 32L * BankLength)
        {
            var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
            return ContainsDandanatorCommand(bytes.Span);
        }
        return false;
    }

    Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!CorpusLengths.Contains(context.Length)) throw new InvalidDataException("The CPC ROM length is outside the demonstrated corpus profiles.");
        var headerLength = context.Length % BankLength == OptionalHeaderLength ? OptionalHeaderLength : 0;
        var dataLength = context.Length - headerLength;
        if (dataLength <= 0) throw new InvalidDataException("The CPC ROM contains no bank data.");
        var bankCount = checked((int)((dataLength + BankLength - 1) / BankLength));
        if (bankCount > 32) throw new NotSupportedException("The CPC ROM exceeds 32 cartridge banks.");

        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        var ranges = new List<MediaDataRange>(bankCount);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["systemId"] = "amstrad",
            ["mediaRole"] = "rom",
            ["bankSize"] = BankLength.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["bankCount"] = bankCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["sourceHeaderLength"] = headerLength.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        for (var bank = 0; bank < bankCount; bank++)
        {
            var length = (int)Math.Min(BankLength, dataLength - bank * (long)BankLength);
            ranges.Add(new MediaDataRange(bank * (long)BankLength, length, MediaDataRangeKind.Stored, source,
                headerLength + bank * (long)BankLength));
            metadata[$"bank.{bank}.length"] = length.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        var capacity = bankCount * (long)BankLength;
        var diagnostics = dataLength % BankLength == 0
            ? Array.Empty<string>()
            : new[] { "The last CPC ROM bank is incomplete and is exposed without invented padding." };
        var volume = new MediaVolumeDescriptor(0, capacity, MediaVolumeOrigins.DirectVolume, name: "Amstrad ROM");
        return Task.FromResult(new MediaImageDocument(
            context.Source,
            DiskImageFormatIds.AmstradRom,
            MediaKind.Cartridge,
            new BlockMediaImageRepresentation(capacity, 1, ranges),
            [volume],
            diagnostics,
            metadata));
    }

    private static bool HasValidAmsdosHeader(ReadOnlySpan<byte> header)
    {
        if (header.Length < OptionalHeaderLength) return false;
        var checksum = 0;
        for (var index = 0; index <= 66; index++) checksum = (checksum + header[index]) & ushort.MaxValue;
        var storedChecksum = header[67] | header[68] << 8;
        return checksum == storedChecksum;
    }

    private static bool ContainsDandanatorCommand(ReadOnlySpan<byte> bytes)
    {
        for (var index = 0; index <= bytes.Length - 4; index++)
        {
            if (bytes[index] != 0xfd || bytes[index + 1] != 0xfd || bytes[index + 2] != 0xfd) continue;
            if (bytes[index + 3] is 0x70 or 0x71 or 0x77) return true;
        }
        return false;
    }
}
