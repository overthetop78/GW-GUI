using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaFileSystems.Contracts;

namespace GWGUI.MediaEngine.Images.Formats.Cartridge.Raw;

/// <summary>
/// Exposes raw console cartridge dumps as declared 16 KiB banks.
/// The final partial bank remains unavailable instead of being padded with invented data.
/// </summary>
public sealed class RawCartridgeReader : IMediaImageReader
{
    public const int BankLength = 16 * 1024;

    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { DiskImageFormatIds.RawCartridge }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlySet<string> SupportedExtensions = new[]
    {
        DiskImageFileExtensions.Bin, DiskImageFileExtensions.Rom,
        DiskImageFileExtensions.Nes, DiskImageFileExtensions.Sfc,
        DiskImageFileExtensions.Smc, DiskImageFileExtensions.N64,
        DiskImageFileExtensions.Z64, DiskImageFileExtensions.V64,
        DiskImageFileExtensions.Gb, DiskImageFileExtensions.Gbc,
        DiskImageFileExtensions.Gba, DiskImageFileExtensions.Nds,
        DiskImageFileExtensions.ThreeDs, DiskImageFileExtensions.Cia,
        DiskImageFileExtensions.ThreeDsx, DiskImageFileExtensions.Sms,
        DiskImageFileExtensions.Sg, DiskImageFileExtensions.Md,
        DiskImageFileExtensions.Gen, DiskImageFileExtensions.Gg,
        DiskImageFileExtensions.ThirtyTwoX, DiskImageFileExtensions.A26,
        DiskImageFileExtensions.A52, DiskImageFileExtensions.A78,
        DiskImageFileExtensions.Pce, DiskImageFileExtensions.Vb,
        DiskImageFileExtensions.Ws, DiskImageFileExtensions.Wsc,
        DiskImageFileExtensions.Cgb
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.Cartridge }.ToFrozenSet();

    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions =>
        FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds =>
        SupportedRepresentations;
    bool IMediaImageReader.SupportsFormatId(string formatId) =>
        SupportedFormatIds.Contains(formatId);

    ValueTask<bool> IMediaImageReader.CanReadAsync(
        MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.Length <= 0 || !SupportedExtensions.Contains(context.Extension))
            return ValueTask.FromResult(false);
        return ValueTask.FromResult(context.RequestedFormatId is null
            || SupportedFormatIds.Contains(context.RequestedFormatId));
    }

    Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!SupportedExtensions.Contains(context.Extension))
            throw new InvalidDataException("The file extension is not a raw cartridge extension.");
        if (context.Length <= 0)
            throw new InvalidDataException("The cartridge image is empty.");

        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        var bankCount = checked((int)((context.Length + BankLength - 1) / BankLength));
        var capacity = checked((long)bankCount * BankLength);
        var ranges = new List<MediaDataRange>(bankCount);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["systemId"] = "console",
            ["mediaRole"] = "raw-cartridge",
            ["bankSize"] = BankLength.ToString(CultureInfo.InvariantCulture),
            ["bankCount"] = bankCount.ToString(CultureInfo.InvariantCulture),
            ["storedLength"] = context.Length.ToString(CultureInfo.InvariantCulture),
            ["sourceExtension"] = context.Extension
        };

        for (var bank = 0; bank < bankCount; bank++)
        {
            var offset = bank * (long)BankLength;
            var length = (int)Math.Min(BankLength, context.Length - offset);
            ranges.Add(new MediaDataRange(offset, length, MediaDataRangeKind.Stored,
                source, offset));
            metadata[$"bank.{bank}.length"] = length.ToString(CultureInfo.InvariantCulture);
            metadata[$"bank.{bank}.name"] = $"bank{bank:D2}";
        }

        var diagnostics = context.Length % BankLength == 0
            ? Array.Empty<string>()
            : new[] { "The final cartridge bank is partial and its unavailable tail is not padded." };
        var volume = new MediaVolumeDescriptor(0, capacity,
            MediaVolumeOrigins.DirectVolume, name: "Raw cartridge");
        return Task.FromResult(new MediaImageDocument(
            context.Source,
            DiskImageFormatIds.RawCartridge,
            MediaKind.Cartridge,
            new BlockMediaImageRepresentation(capacity, BankLength, ranges),
            [volume],
            diagnostics,
            metadata));
    }
}
