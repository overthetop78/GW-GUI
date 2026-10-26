using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Interfaces;

/// <summary>Image possédée par un runtime : accès sectoriel et publication par son écrivain de conteneur.</summary>
/// <remarks>Le propriétaire sérialise ses appels et libère l'image à l'arrêt. Les écritures restent en mémoire
/// jusqu'à FlushAsync ; une erreur conserve les changements pour une nouvelle tentative.</remarks>
public interface IMediaRuntimeDisk : IDisposable
{
    string FormatId { get; }
    int SectorSize { get; }
    long SectorCount { get; }
    long Cylinders { get; }
    int Heads { get; }
    int SectorsPerTrack { get; }
    bool IsReadOnly { get; }
    bool HasChanges { get; }
    IReadOnlyDictionary<string, string> Metadata { get; }
    void UpdateMetadata(IReadOnlyDictionary<string, string> metadata);
    long? FindSector(int cylinder, int head, int number);
    ValueTask<MediaDiskSector> ReadSectorAsync(long index, CancellationToken cancellationToken = default);
    void WriteSector(long index, ReadOnlySpan<byte> data);
    Task FlushAsync(CancellationToken cancellationToken = default);
}
