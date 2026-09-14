using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Formats.Floppy.Atr;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.Xfd;

/// <summary>Reads headerless Atari 8-bit XFD sector images.</summary>
public sealed class XfdReader : IMediaImageReader
{
    private const int SupportedTrailingPaddingLength = 16;
    private sealed record Profile(string FormatId, int SectorSize, int SectorCount, int Length);

    private static readonly IReadOnlyList<Profile> Profiles =
    [
        new(DiskImageFormatIds.AtariXfd90, AtrLayout.SingleDensitySectorSize, AtrLayout.StandardSectorCount,
            AtrLayout.SingleDensitySectorSize * AtrLayout.StandardSectorCount),
        new(DiskImageFormatIds.AtariXfd130, AtrLayout.SingleDensitySectorSize, AtrLayout.EnhancedDensitySectorCount,
            AtrLayout.SingleDensitySectorSize * AtrLayout.EnhancedDensitySectorCount),
        new(DiskImageFormatIds.AtariXfd140, AtrLayout.SingleDensitySectorSize, AtrLayout.ExtendedSingleDensitySectorCount,
            AtrLayout.SingleDensitySectorSize * AtrLayout.ExtendedSingleDensitySectorCount),
        new(DiskImageFormatIds.AtariXfd180, AtrLayout.DoubleDensitySectorSize, AtrLayout.StandardSectorCount,
            AtrLayout.BootSectorCount * AtrLayout.BootSectorSize
            + (AtrLayout.StandardSectorCount - AtrLayout.BootSectorCount) * AtrLayout.DoubleDensitySectorSize)
    ];
    private static readonly IReadOnlySet<string> SupportedFormatIds = Profiles
        .Select(profile => profile.FormatId)
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new[] { DiskImageFileExtensions.Xfd }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;

    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var profile = ProfileFor(context.Length, context.RequestedFormatId);
        return ValueTask.FromResult(profile is not null);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        var bytes = (await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false)).ToArray();
        return MediaImageDocumentFactory.CreateFloppySector(
            context.Source,
            Read(bytes, context.RequestedFormatId, cancellationToken));
    }

    internal static SectorImage Read(
        byte[] bytes,
        string? requestedFormatId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        var profile = ProfileFor(bytes.Length, requestedFormatId)
            ?? throw new InvalidDataException($"Unsupported Atari XFD image length: {bytes.Length} bytes.");
        var trailingLength = bytes.Length - profile.Length;
        if (trailingLength != 0
            && (trailingLength != SupportedTrailingPaddingLength
                || bytes.Skip(profile.Length).Any(value => value != 0)))
        {
            throw new InvalidDataException("The Atari XFD trailing padding is invalid.");
        }
        var geometry = AtrLayout.GetGeometry(profile.SectorSize, profile.SectorCount);
        var blocks = new List<SectorBlock>(profile.SectorCount);
        var offset = 0;
        for (var sector = 1; sector <= profile.SectorCount; sector++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var length = profile.SectorSize > AtrLayout.BootSectorSize && sector <= AtrLayout.BootSectorCount
                ? AtrLayout.BootSectorSize
                : profile.SectorSize;
            var logical = sector - 1;
            var cylinder = logical / geometry.SectorsPerTrack;
            var sectorInTrack = logical % geometry.SectorsPerTrack + 1;
            blocks.Add(new(logical, new(cylinder, 0, sectorInTrack), bytes.AsSpan(offset, length).ToArray()));
            offset += length;
        }
        return new(
            profile.FormatId,
            profile.SectorSize,
            geometry.Cylinders,
            geometry.Heads,
            geometry.SectorsPerTrack,
            blocks,
            allowVariableBlockSize: profile.SectorSize > AtrLayout.BootSectorSize,
            capacity: profile.Length,
            logicalBlockCount: profile.SectorCount);
    }

    private static Profile? ProfileFor(long length, string? requestedFormatId) => Profiles.FirstOrDefault(profile =>
        (profile.Length == length || profile.Length + SupportedTrailingPaddingLength == length)
        && (requestedFormatId is null || profile.FormatId.Equals(requestedFormatId, StringComparison.OrdinalIgnoreCase)));
}
