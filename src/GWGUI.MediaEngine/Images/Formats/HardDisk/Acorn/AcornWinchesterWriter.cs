using System.IO;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Reading.Blocks;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Interfaces.Writing;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Acorn;

/// <summary>Publie les secteurs DAT et le descripteur DSC par la transaction multifichier commune.</summary>
public sealed class AcornWinchesterWriter : IMediaImageWriter
{
    public string Id => MediaImageWriterIds.AcornWinchester;
    public IReadOnlySet<string> FormatIds { get; } = new[] { HardDiskImageFormatIds.AcornWinchester }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } = new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Blocks };
    public IReadOnlySet<string> ProducedFileExtensions => AcornWinchesterFormat.Extensions;
    public bool ProducesMultipleFiles => true;

    public bool CanWrite(MediaImageDocument document, string format, string extension) => FormatIds.Contains(format) && ProducedFileExtensions.Contains(extension) && document.MediaKind == MediaKind.HardDisk
        && document.Representation is BlockMediaImageRepresentation { LogicalBlockSize: AcornWinchesterFormat.SectorSize, Geometry: { Heads: <= 255, Cylinders: <= 65535, SectorsPerTrack: AcornWinchesterFormat.SectorsPerTrack, SectorSize: AcornWinchesterFormat.SectorSize } geometry } blocks
        && blocks.Capacity == geometry.Capacity && blocks.HasCompleteReadableCoverage();

    public Task<IReadOnlyList<string>> WriteAsync(MediaImageDocument document, string outputPath, string format, CancellationToken token = default) => WriteFilesAsync(document, outputPath, format, true, token);

    /// <summary>Publie une nouvelle paire sans remplacer un DAT ou un DSC déjà présent, y compris après une concurrence.</summary>
    public Task<IReadOnlyList<string>> CreateAsync(MediaImageDocument document, string outputPath, string format, CancellationToken token = default) => WriteFilesAsync(document, outputPath, format, false, token);

    private async Task<IReadOnlyList<string>> WriteFilesAsync(MediaImageDocument document, string outputPath, string format, bool replaceExisting, CancellationToken token)
    {
        if (!CanWrite(document, format, Path.GetExtension(outputPath))) throw new InvalidDataException("Winchester output requires complete 256-byte sectors and a declared 33-sector CHS geometry.");
        var blocks = (BlockMediaImageRepresentation)document.Representation;
        var descriptor = document.Metadata.TryGetValue(AcornWinchesterFormat.DescriptorMetadataKey, out var encoded) ? Convert.FromBase64String(encoded) : [];
        if (descriptor.Length < AcornWinchesterFormat.DescriptorSize) Array.Resize(ref descriptor, AcornWinchesterFormat.DescriptorSize);
        var geometry = blocks.Geometry!;
        descriptor[13] = (byte)(geometry.Cylinders >> 8); descriptor[14] = (byte)geometry.Cylinders; descriptor[15] = (byte)geometry.Heads;
        var dat = Path.GetFullPath(outputPath); var dsc = Path.ChangeExtension(dat, ".dsc");
        await AtomicMediaFileSetWriter.WriteAsync(new Dictionary<string, Func<Stream, CancellationToken, Task>>(StringComparer.OrdinalIgnoreCase)
        {
            [dat] = (stream, cancellation) => BlockMediaDataReader.CopyToAsync(blocks, stream, cancellation),
            [dsc] = (stream, cancellation) => stream.WriteAsync(descriptor, cancellation).AsTask()
        }, token, replaceExisting).ConfigureAwait(false);
        return [dat, dsc];
    }
}
