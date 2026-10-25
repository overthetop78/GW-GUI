using System.Collections.Frozen;
using System.IO;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Interfaces.Reading;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Raw;

internal sealed class DefinedRawImageReader : IMediaImageReader
{
    public IReadOnlySet<string> FormatIds { get; } = DiskDefsFormatReader.RawDefinitions.Keys.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public IReadOnlySet<string> Extensions { get; } = new[] { ".img" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyList<ReadOnlyMemory<byte>> Signatures => [];
    public IReadOnlySet<string> AssociatedFileExtensions => FrozenSet<string>.Empty;
    public IReadOnlySet<MediaKind> MediaKinds { get; } = new[] { MediaKind.Floppy }.ToFrozenSet();
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();
    public bool SupportsFormatId(string formatId) => FormatIds.Contains(formatId);

    public ValueTask<bool> CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(context.RequestedFormatId is { } id
            && DiskDefsFormatReader.RawDefinitions.TryGetValue(id, out var definition)
            && context.Length == definition.Layout().Sum(sector => (long)sector.ImageSize));
    }

    public async Task<MediaImageDocument> ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var definition = DiskDefsFormatReader.RawDefinitions[context.RequestedFormatId!];
        var layout = definition.Layout();
        var data = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        if (data.Length != layout.Sum(sector => sector.ImageSize))
            throw new InvalidDataException($"Image length does not match disk definition '{definition.Id}'.");
        var blocks = new List<SectorBlock>(layout.Count);
        var offset = 0;
        foreach (var sector in layout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            blocks.Add(new(sector.LogicalBlock, new(sector.Cylinder, sector.Head, sector.Number),
                data.Slice(offset, sector.Size).ToArray(), null, 0));
            offset += sector.ImageSize;
        }
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, CreateImage(definition, layout, blocks));
    }

    internal static SectorImage CreateImage(DiskDefinition definition, IReadOnlyList<DefinedDiskSector> layout, IEnumerable<SectorBlock> blocks)
        => new(definition.Id, layout.Max(sector => sector.Size), definition.Cylinders, definition.Heads,
            layout.GroupBy(sector => (sector.Cylinder, sector.Head)).Max(group => group.Count()), blocks,
            allowVariableBlockSize: true, capacity: layout.Sum(sector => (long)sector.ImageSize), logicalBlockCount: layout.Count);
}
