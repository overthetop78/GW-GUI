namespace GWGUI.Emulation.Interfaces;

/// <summary>Accès disque commun aux émulateurs internes, relayé vers les lecteurs et écrivains de MediaEngine.</summary>
/// <remarks>Le cœur possède cette image, sérialise ses appels, la sauvegarde et la libère dans finally.
/// Une géométrie nulle est représentée par zéro ; elle n'est pas déduite d'une simple capacité.</remarks>
public interface IEmulationDiskImage : IDisposable
{
    /// <summary>Mécanique du lecteur emprunté ; fermer cette image ne détruit pas un lecteur fourni.</summary>
    IEmulationDriveMechanics Mechanics { get; }
    string FormatId { get; }
    int SectorSize { get; }
    long SectorCount { get; }
    long Cylinders { get; }
    int Heads { get; }
    int SectorsPerTrack { get; }
    bool IsReadOnly { get; }
    bool HasChanges { get; }
    IReadOnlyDictionary<string, string> Metadata { get; }
    /// <summary>Prépare les métadonnées prises en charge par le format ; FlushAsync les publie avec les secteurs.</summary>
    /// <remarks>Une clé non prise en charge ou une géométrie modifiant la capacité est refusée.</remarks>
    void UpdateMetadata(IReadOnlyDictionary<string, string> metadata);
    /// <summary>Recherche l'identifiant physique d'un secteur sans supposer une numérotation universelle à zéro ou un.</summary>
    long? FindSector(int cylinder, int head, int number);
    ValueTask<EmulationDiskSector> ReadSectorAsync(long index, CancellationToken cancellationToken = default);
    /// <summary>Conserve un secteur modifié ; l'écriture physique utilise FlushAsync.</summary>
    void WriteSector(long index, ReadOnlySpan<byte> data);
    Task FlushAsync(CancellationToken cancellationToken = default);
}
