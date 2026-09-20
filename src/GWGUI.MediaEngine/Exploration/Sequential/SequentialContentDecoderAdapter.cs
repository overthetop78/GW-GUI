using System.Collections.Frozen;
using System.IO;
using MediaVolumeOrigins = global::GWGUI.MediaFileSystems.Constants.MediaVolumeOrigins;
using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Interfaces.Exploration;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaFileSystems.Exploration.Sequential;

namespace GWGUI.MediaEngine.Exploration.Sequential;

/// <summary>Décode la bande dans le moteur puis confie les vrais fichiers au lecteur de systèmes de fichiers.</summary>
public sealed class SequentialContentDecoderAdapter : IMediaFileSystemReader
{
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private readonly SequentialDecoderRegistry decoders;
    private readonly SequentialContentFileSystemReader fileSystems = new();

    public SequentialContentDecoderAdapter(SequentialDecoderRegistry decoders)
    {
        ArgumentNullException.ThrowIfNull(decoders);
        this.decoders = decoders;
    }

    public string Id => FileSystemIds.SequentialContent;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;

    public bool CanRead(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        return document.MediaKind == MediaKind.Tape
            && document.Representation is SequentialMediaImageRepresentation
            && (volume.FileSystemId?.Equals(Id, StringComparison.OrdinalIgnoreCase) == true
                || volume.Origin.Equals(MediaVolumeOrigins.SequentialContent, StringComparison.OrdinalIgnoreCase));
    }

    public FileSystemVolume Read(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        if (!CanRead(document, volume))
            throw new InvalidDataException("The requested volume is not decoded sequential media content.");

        var compatible = decoders.FindCompatible(document);
        var results = compatible
            .Select(decoder => decoder.DecodeAsync(document).GetAwaiter().GetResult())
            .OrderByDescending(result => result.Blocks.Count > 0)
            .ThenByDescending(result => result.Confidence)
            .ThenByDescending(result => result.Blocks.Count)
            .ToArray();
        var selected = results.FirstOrDefault();
        var diagnostics = new List<string>(selected?.Diagnostics ?? []);
        if (results.Length > 1 && selected is not null)
            diagnostics.Add($"Selected decoder '{selected.DecoderId}' from {results.Length} compatible sequential decoders.");
        foreach (var segment in selected?.UndecodedSegments ?? [])
            diagnostics.Add($"Undecoded {segment.Kind} segment at position {segment.Position}.");

        var blocks = selected?.Blocks
            .Select(block => new MediaSequentialDecodedBlock(
                block.Position, block.Data, block.IntegrityValid, block.Metadata))
            .ToArray() ?? [];
        var content = new DecodedContent(selected?.DecoderId, blocks, diagnostics);
        var volumeName = document.Metadata.TryGetValue(AtariCasConstants.InternalNameMetadataKey, out var internalName)
            && !string.IsNullOrWhiteSpace(internalName)
                ? internalName.Trim()
                : string.Empty;
        return MediaFileSystemsReaderAdapter.ConvertVolume(fileSystems.Read(document, volume, content, volumeName));
    }

    private sealed record DecodedContent(
        string? DecoderId,
        IReadOnlyList<MediaSequentialDecodedBlock> Blocks,
        IReadOnlyList<string> Diagnostics) : IMediaSequentialContent;
}
