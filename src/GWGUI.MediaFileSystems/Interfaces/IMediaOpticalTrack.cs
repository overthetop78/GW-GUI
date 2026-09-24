namespace GWGUI.MediaFileSystems.Interfaces;

/// <summary>Métadonnées d'une piste optique déjà reconnue.</summary>
public interface IMediaOpticalTrack
{
    int SessionNumber { get; }

    int TrackNumber { get; }

    long FirstSector { get; }

    long SectorCount { get; }

    int UserDataLength { get; }

    bool IsAudio { get; }

    ValueTask<byte[]> ReadUserDataAsync(long relativeSector, CancellationToken cancellationToken = default);
}
