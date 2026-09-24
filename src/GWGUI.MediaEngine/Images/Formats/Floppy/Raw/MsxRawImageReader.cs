using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaFileSystems.FileSystems.Fat12;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Recognition.Msx;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Sectors;
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Raw;

/// <summary>Lit et valide une image sectorielle brute MSX-DOS.</summary>
public sealed class MsxRawImageReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = MsxDiskGeometryCatalog.Supported.Select(geometry => geometry.FormatId).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Dsk }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public MsxRawImageReader() : this(File.ReadAllBytesAsync) { }

    internal MsxRawImageReader(Func<string, CancellationToken, Task<byte[]>> readBytes)
    {
        this.readBytes = readBytes ?? throw new ArgumentNullException(nameof(readBytes));
    }

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => formatId.StartsWith(DiskImageFormatIds.MsxPrefix, StringComparison.OrdinalIgnoreCase);

    /// <summary>Lit l'image, valide son BPB MSX-DOS et reconstruit ses secteurs dans l'ordre CHS linÃ©aire dÃ©crit par sa gÃ©omÃ©trie.</summary>
    /// <param name="path">Chemin de l'image brute Ã  lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et la construction sectorielle.</param>
    /// <returns>L'image sectorielle MSX-DOS validÃ©e et associÃ©e Ã  sa gÃ©omÃ©trie.</returns>
    /// <exception cref="InvalidDataException">Le secteur d'amorÃ§age n'est pas reconnu comme MSX-DOS, ou la capacitÃ© et le descripteur de mÃ©dia ne correspondent Ã  aucune gÃ©omÃ©trie prise en charge.</exception>
    /// <remarks>Les capacitÃ©s et tailles sectorielles manipulÃ©es sont exprimÃ©es en octets. Les adresses sectorielles utilisent une numÃ©rotation commenÃ§ant Ã  un.</remarks>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await readBytes(path, cancellationToken).ConfigureAwait(false);
        return Read(data, cancellationToken);
    }

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !((IMediaImageReader)this).SupportsFormatId(context.RequestedFormatId)) return false;
        if (context.Length > int.MaxValue) return false;
        var data = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        if (!MsxBootSectorProbe.LooksLikeMsx(data.Span) || data.Length <= FatBootSectorLayout.MediaDescriptorOffset) return false;
        return MsxDiskGeometryCatalog.Find(data.Length, data.Span[FatBootSectorLayout.MediaDescriptorOffset]) is not null;
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var data = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, Read(data.ToArray(), cancellationToken));
    }

    private static SectorImage Read(byte[] data, CancellationToken cancellationToken)
    {
        if (!MsxBootSectorProbe.LooksLikeMsx(data)) throw MsxRawImageExceptions.InvalidBootSector(data.Length);
        var mediaDescriptor = data[FatBootSectorLayout.MediaDescriptorOffset];
        var geometry = MsxDiskGeometryCatalog.Find(data.Length, mediaDescriptor) ?? throw MsxRawImageExceptions.UnsupportedGeometry(data.Length, mediaDescriptor);
        var linear = new LinearSectorImageGeometry(FatBootSectorLayout.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, SectorNumbering.OneBased);
        return LinearSectorImageBuilder.Create(data, geometry.FormatId, linear, cancellationToken);
    }
}
