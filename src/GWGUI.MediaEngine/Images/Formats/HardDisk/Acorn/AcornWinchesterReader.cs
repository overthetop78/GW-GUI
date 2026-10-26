using System.IO;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Interfaces.Reading;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Acorn;

/// <summary>Lit les secteurs Winchester BBC et leur géométrie déclarée, sans ouvrir durablement de flux.</summary>
public sealed class AcornWinchesterReader : IMediaImageReader
{
    public IReadOnlySet<string> FormatIds { get; } = new[] { HardDiskImageFormatIds.AcornWinchester }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public IReadOnlySet<string> Extensions => AcornWinchesterFormat.Extensions;
    public IReadOnlySet<string> AssociatedFileExtensions => AcornWinchesterFormat.AssociatedExtensions;
    public IReadOnlyList<ReadOnlyMemory<byte>> Signatures => [];
    public IReadOnlySet<MediaKind> MediaKinds { get; } = new HashSet<MediaKind> { MediaKind.HardDisk };
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } = new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Blocks };
    public bool SupportsFormatId(string id) => FormatIds.Contains(id);

    public async ValueTask<bool> CanReadAsync(MediaRecognitionContext context, CancellationToken token)
    {
        if (context.RequestedFormatId is { } requested && !SupportsFormatId(requested)) return false;
        if (context.Length <= 0 || context.Length % AcornWinchesterFormat.SectorSize != 0) return false;
        if (context.RequestedFormatId is not null) return true;
        if (!Extensions.Contains(context.Extension)) return false;
        var descriptor = DescriptorPath(context.Source);
        if (File.Exists(descriptor) && AcornWinchesterFormat.Geometry(await File.ReadAllBytesAsync(descriptor, token).ConfigureAwait(false)) is not null) return true;
        if (context.Length < 0x700) return false;
        var header = await context.ReadAsync(0x201, 4, token).ConfigureAwait(false);
        var footer = await context.ReadAsync(0x6fb, 4, token).ConfigureAwait(false);
        return header.Span.SequenceEqual("Hugo"u8) && footer.Span.SequenceEqual(header.Span);
    }

    public async Task<MediaImageDocument> ReadAsync(MediaRecognitionContext context, CancellationToken token)
    {
        if (!await CanReadAsync(context, token).ConfigureAwait(false)) throw new InvalidDataException("The source is not an Acorn Winchester sector image.");
        var descriptorPath = DescriptorPath(context.Source);
        var descriptor = File.Exists(descriptorPath) ? await File.ReadAllBytesAsync(descriptorPath, token).ConfigureAwait(false) : [];
        var geometry = AcornWinchesterFormat.Geometry(descriptor);
        if (geometry is not null && context.Length > geometry.Capacity) throw new InvalidDataException("The DAT image exceeds the geometry declared by its DSC descriptor.");
        var capacity = geometry?.Capacity ?? context.Length;
        var data = new FileRandomAccessData(context.Source.PrimaryPath);
        var ranges = new List<MediaDataRange> { new(0, context.Length, MediaDataRangeKind.Stored, data) };
        var diagnostics = new List<string>();
        if (capacity > context.Length) { ranges.Add(new(context.Length, capacity - context.Length, MediaDataRangeKind.Unavailable)); diagnostics.Add("The DAT source does not contain all sectors declared by the DSC descriptor."); }
        if (geometry is null) diagnostics.Add("No valid DSC geometry is available; physical surfaces and tracks are unknown.");
        var associated = context.Source.AssociatedPaths.Concat(File.Exists(descriptorPath) ? [descriptorPath] : Array.Empty<string>()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return MediaImageDocument.CreateUnexplored(context.Source with { AssociatedPaths = associated }, HardDiskImageFormatIds.AcornWinchester, MediaKind.HardDisk, new BlockMediaImageRepresentation(capacity, AcornWinchesterFormat.SectorSize, ranges, geometry), diagnostics,
            new Dictionary<string, string> { ["logicalBlockSize"] = "256", [AcornWinchesterFormat.DescriptorMetadataKey] = Convert.ToBase64String(descriptor) });
    }

    private static string DescriptorPath(MediaSourceDescriptor source) => source.AssociatedPaths.FirstOrDefault(path => Path.GetExtension(path).Equals(".dsc", StringComparison.OrdinalIgnoreCase)) ?? Path.ChangeExtension(source.PrimaryPath, ".dsc");
}
