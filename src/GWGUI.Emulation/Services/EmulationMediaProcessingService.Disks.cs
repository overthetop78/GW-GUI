using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.Emulation.Services;

/// <summary>Projection des accès secteurs sur le contrat SDK, sans exposer les types de MediaEngine.</summary>
internal sealed partial class EmulationMediaProcessingService
{
    /// <inheritdoc/>
    public Task CreateBlankDiskAsync(string path, string formatId, CancellationToken cancellationToken = default) => Preparation.CreateBlankDiskAsync(path, formatId, cancellationToken);

    public Task CreateHardDiskAsync(string path, string formatId, GWGUI.Emulation.HardDisks.DiskImageGeometry geometry, string? fileSystemId = null, CancellationToken cancellationToken = default, bool preallocate = false)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        return Preparation.CreateHardDiskAsync(path, formatId, new GWGUI.MediaEngine.Contracts.HardDiskGeometry(geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, geometry.SectorSize), fileSystemId, cancellationToken, preallocate);
    }
    public async Task<IEmulationDiskImage> OpenDiskAsync(string path, bool writable = false, string? formatId = null, CancellationToken cancellationToken = default, IEmulationDriveMechanics? mechanics = null)
    {
        var source = mechanics is null ? null : mechanics is DriveMechanics bridge ? bridge.Source : throw new ArgumentException(nameof(mechanics));
        return new DiskImage(await Preparation.OpenDiskAsync(path, writable, formatId, cancellationToken, source).ConfigureAwait(false), mechanics);
    }

    /// <summary>Possède l'image reçue de MediaEngine et lui transfère exactement les mêmes opérations.</summary>
    private sealed class DiskImage(IMediaRuntimeDisk image, IEmulationDriveMechanics? mechanics) : IEmulationDiskImage
    {
        public IEmulationDriveMechanics Mechanics { get; } = mechanics ?? new DriveMechanics(image.Mechanics);
        public string FormatId => image.FormatId;
        public int SectorSize => image.SectorSize;
        public long SectorCount => image.SectorCount;
        public long Cylinders => image.Cylinders;
        public int Heads => image.Heads;
        public int SectorsPerTrack => image.SectorsPerTrack;
        public bool IsReadOnly => image.IsReadOnly;
        public bool HasChanges => image.HasChanges;
        public IReadOnlyDictionary<string, string> Metadata => image.Metadata;
        public void UpdateMetadata(IReadOnlyDictionary<string, string> metadata) => image.UpdateMetadata(metadata);
        public long? FindSector(int cylinder, int head, int number) => image.FindSector(cylinder, head, number);
        public async ValueTask<EmulationDiskSector> ReadSectorAsync(long index, CancellationToken cancellationToken = default)
        {
            var sector = await image.ReadSectorAsync(index, cancellationToken).ConfigureAwait(false);
            return new(sector.Index, sector.Data, sector.Cylinder, sector.Head, sector.Number, sector.IntegrityValid, sector.DiagnosticCode);
        }
        public void WriteSector(long index, ReadOnlySpan<byte> data) => image.WriteSector(index, data);
        public Task FlushAsync(CancellationToken cancellationToken = default) => image.FlushAsync(cancellationToken);
        public void Dispose() => image.Dispose();
    }
}
