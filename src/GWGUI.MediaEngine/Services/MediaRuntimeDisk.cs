using System.IO;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Reading.Blocks;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Images.Formats.HardDisk.Acorn;

namespace GWGUI.MediaEngine.Services;

/// <summary>Réutilise les représentations et écrivains d'images pour les accès d'un contrôleur émulé.</summary>
/// <remarks>Les secteurs modifiés sont possédés en mémoire ; les sources ne conservent aucun flux de fichier.
/// Cette classe ne simule ni contrôleur ni temporisation : ces responsabilités restent dans la machine.</remarks>
internal sealed class MediaRuntimeDisk : IMediaRuntimeDisk
{
    private readonly MediaEngineComposition _engine;
    private MediaImageDocument _document;
    private readonly SortedDictionary<long, byte[]> _changes = new();
    private bool _disposed;
    private bool _metadataChanged;
    private readonly bool _ownsMechanics;
    public IMediaDriveMechanics Mechanics => _document.Source.Drive!;
    private SectorImage? Sectors => (_document.Representation as SectorMediaImageRepresentation)?.Image;
    private BlockMediaImageRepresentation? Blocks => _document.Representation as BlockMediaImageRepresentation;
    public string FormatId => _document.FormatId;
    public int SectorSize => Sectors?.BlockSize ?? Blocks!.LogicalBlockSize;
    public long SectorCount => Sectors?.BlockCount ?? Blocks!.LogicalBlockCount;
    public long Cylinders => Sectors?.Cylinders ?? Blocks?.Geometry?.Cylinders ?? 0;
    public int Heads => Sectors?.Heads ?? Blocks?.Geometry?.Heads ?? 0;
    public int SectorsPerTrack => Sectors?.SectorsPerTrack ?? Blocks?.Geometry?.SectorsPerTrack ?? 0;
    public bool IsReadOnly { get; }
    public bool HasChanges => _changes.Count > 0 || _metadataChanged;
    public IReadOnlyDictionary<string, string> Metadata => _document.Metadata;

    /// <summary>Conserve les paramètres de contrôleur dans le document et emploie sa publication multifichier normale.</summary>
    public void UpdateMetadata(IReadOnlyDictionary<string, string> metadata)
    {
        ObjectDisposedException.ThrowIf(_disposed, this); ArgumentNullException.ThrowIfNull(metadata);
        if (IsReadOnly) throw new UnauthorizedAccessException("The disk image is write protected.");
        if (metadata.Count == 0) return;
        if (FormatId != HardDiskImageFormatIds.AcornWinchester || Blocks is not { } blocks || metadata.Count != 1 || !metadata.TryGetValue(AcornWinchesterFormat.DescriptorMetadataKey, out var encoded)) throw new NotSupportedException("The disk format cannot persist these metadata keys.");
        byte[] incoming;
        try { incoming = Convert.FromBase64String(encoded); }
        catch (FormatException error) { throw new ArgumentException("Invalid disk descriptor encoding.", nameof(metadata), error); }
        if (incoming.Length is < 22 or > AcornWinchesterFormat.DescriptorSize) throw new ArgumentException("The Acorn controller descriptor requires 22 to 33 bytes.", nameof(metadata));
        var descriptor = Metadata.TryGetValue(AcornWinchesterFormat.DescriptorMetadataKey, out var old) ? Convert.FromBase64String(old) : [];
        if (descriptor.Length < AcornWinchesterFormat.DescriptorSize) Array.Resize(ref descriptor, AcornWinchesterFormat.DescriptorSize);
        incoming.CopyTo(descriptor, 0);
        var geometry = AcornWinchesterFormat.Geometry(descriptor);
        if (geometry is null || geometry.Capacity != blocks.Capacity) throw new ArgumentException("The descriptor geometry must preserve the image capacity.", nameof(metadata));
        var values = new Dictionary<string, string>(Metadata) { [AcornWinchesterFormat.DescriptorMetadataKey] = Convert.ToBase64String(descriptor) };
        if (Metadata.TryGetValue(AcornWinchesterFormat.DescriptorMetadataKey, out var previous) && previous == values[AcornWinchesterFormat.DescriptorMetadataKey]) return;
        _document = MediaImageDocument.CreateUnexplored(_document.Source, FormatId, _document.MediaKind, new BlockMediaImageRepresentation(blocks.Capacity, blocks.LogicalBlockSize, blocks.Ranges, geometry), _document.Diagnostics, values);
        _metadataChanged = true;
    }

    internal MediaRuntimeDisk(MediaEngineComposition engine, MediaImageDocument document, bool writable, bool ownsMechanics = true)
    {
        if (document.MediaKind is not (MediaKind.Floppy or MediaKind.HardDisk) || document.Representation is not (SectorMediaImageRepresentation or BlockMediaImageRepresentation)) throw new NotSupportedException("A decoded sector or block disk image is required.");
        _engine = engine; _document = document; _ownsMechanics = ownsMechanics;
        IsReadOnly = !writable || new FileInfo(document.Source.PrimaryPath).IsReadOnly || !engine.Writing.Writers.Any(writer => writer.CanWrite(document, document.FormatId, Path.GetExtension(document.Source.PrimaryPath)));
    }

    public long? FindSector(int cylinder, int head, int number)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (Sectors is { } sectors) return sectors.AvailableBlocks.FirstOrDefault(block => block.Address == new SectorAddress(cylinder, head, number))?.LogicalBlock;
        if (cylinder < 0 || cylinder >= Cylinders || head < 0 || head >= Heads || number < 0 || number >= SectorsPerTrack) return null;
        return ((long)cylinder * Heads + head) * SectorsPerTrack + number;
    }

    public async ValueTask<MediaDiskSector> ReadSectorAsync(long index, CancellationToken cancellationToken = default)
    {
        Validate(index); cancellationToken.ThrowIfCancellationRequested();
        SectorBlock? sector = null;
        if (Sectors is { } image) image.TryGetBlock(checked((int)index), out sector!);
        var address = sector?.Address ?? Address(index);
        Mechanics.BeginRead(Position(index, address));
        try
        {
        if (_changes.TryGetValue(index, out var changed)) return new(index, changed.ToArray(), address.Cylinder, address.Head, address.Number, true);
        if (sector is not null) return new(index, sector.Data.ToArray(), address.Cylinder, address.Head, address.Number, sector.IntegrityValid, sector.DiagnosticCode);
        if (Blocks is not { } blocks) throw new InvalidDataException($"Sector {index} is unavailable in the source image.");
        var data = new byte[SectorSize];
        await BlockMediaDataReader.ReadExactlyAsync(blocks, checked(index * SectorSize), data, cancellationToken).ConfigureAwait(false);
        return new(index, data, address.Cylinder, address.Head, address.Number);
        }
        finally { Mechanics.EndAccess(); }
    }

    public void WriteSector(long index, ReadOnlySpan<byte> data)
    {
        Validate(index);
        if (IsReadOnly) throw new UnauthorizedAccessException("The disk image is write protected.");
        var size = Sectors is { } sectors && sectors.TryGetBlock(checked((int)index), out var original) ? original.Data.Count : SectorSize;
        if (data.Length != size) throw new ArgumentException("The data must contain exactly one sector.", nameof(data));
        var address = Sectors is { } image && image.TryGetBlock(checked((int)index), out var sector) ? sector.Address : Address(index);
        Mechanics.BeginWrite(Position(index, address));
        try { _changes[index] = data.ToArray(); }
        finally { Mechanics.EndAccess(); }
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!HasChanges) return;
        var document = UpdatedDocument();
        document = new(document.Source with { Drive = null }, document.FormatId, document.MediaKind,
            document.Representation, document.FileSystemVolumes, document.Diagnostics, document.Metadata);
        await _engine.Writing.WritingService.WriteAsync(document, _document.Source.PrimaryPath, FormatId, cancellationToken: cancellationToken).ConfigureAwait(false);
        _document = await _engine.ReadingService.ReadAsync(_document.Source with { KnownLength = null, RequestedFormatId = FormatId }, cancellationToken).ConfigureAwait(false);
        _changes.Clear();
        _metadataChanged = false;
    }

    private MediaImageDocument UpdatedDocument()
    {
        GWGUI.MediaEngine.Interfaces.IMediaImageRepresentation representation;
        if (Sectors is { } sectors)
        {
            var updated = sectors.AvailableBlocks.ToDictionary(block => block.LogicalBlock);
            foreach (var change in _changes)
            {
                var index = checked((int)change.Key);
                updated[index] = updated.TryGetValue(index, out var original) ? original with { Data = change.Value, IntegrityValid = true, DiagnosticCode = null } : new(index, Address(index), change.Value);
            }
            representation = new SectorMediaImageRepresentation(new SectorImage(sectors.FormatId, sectors.BlockSize, sectors.Cylinders, sectors.Heads, sectors.SectorsPerTrack, updated.Values, sectors.AllowsVariableBlockSize, sectors.Capacity, sectors.BlockCount, sectors.AddressingKind));
        }
        else
        {
            var blocks = Blocks!;
            var boundaries = new SortedSet<long> { 0, blocks.Capacity };
            foreach (var range in blocks.Ranges) { boundaries.Add(range.Address); boundaries.Add(range.Address + range.Length); }
            foreach (var change in _changes) { boundaries.Add(change.Key * SectorSize); boundaries.Add((change.Key + 1) * SectorSize); }
            var points = boundaries.ToArray(); var ranges = new List<MediaDataRange>();
            for (var index = 0; index < points.Length - 1; index++)
            {
                var start = points[index]; var length = points[index + 1] - start;
                if (_changes.TryGetValue(start / SectorSize, out var data)) ranges.Add(new(start, length, MediaDataRangeKind.Stored, new MemoryRandomAccessData(data), start % SectorSize));
                else
                {
                    var source = blocks.Ranges.FirstOrDefault(range => start >= range.Address && start < range.Address + range.Length);
                    ranges.Add(source is null ? new(start, length, MediaDataRangeKind.Unavailable) : new(start, length, source.Kind, source.Source, source.Kind == MediaDataRangeKind.Stored ? source.SourceOffset + start - source.Address : 0));
                }
            }
            representation = new BlockMediaImageRepresentation(blocks.Capacity, blocks.LogicalBlockSize, ranges, blocks.Geometry);
        }
        return new(_document.Source, _document.FormatId, _document.MediaKind, representation, _document.FileSystemVolumes, _document.Diagnostics, _document.Metadata);
    }

    private SectorAddress Address(long index) => Heads > 0 && SectorsPerTrack > 0 ? new(checked((int)(index / (Heads * (long)SectorsPerTrack))), (int)(index / SectorsPerTrack % Heads), (int)(index % SectorsPerTrack)) : new(-1, -1, -1);
    private static MediaDrivePosition Position(long index, SectorAddress address) => address.Cylinder >= 0 && address.Head >= 0
        ? new(address.Cylinder, address.Head, address.Number) : new(0, 0, LogicalBlock: index);
    private void Validate(long index) { ObjectDisposedException.ThrowIf(_disposed, this); if (index < 0 || index >= SectorCount) throw new ArgumentOutOfRangeException(nameof(index)); }

    public void Dispose()
    {
        if (_disposed) return;
        try { FlushAsync().GetAwaiter().GetResult(); }
        finally
        {
            try { if (_ownsMechanics) Mechanics.Reset(); }
            finally { _changes.Clear(); _disposed = true; }
        }
    }
}
