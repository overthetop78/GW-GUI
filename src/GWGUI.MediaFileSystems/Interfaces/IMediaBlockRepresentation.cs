namespace GWGUI.MediaFileSystems.Interfaces;

/// <summary>Accès en lecture aux blocs d'un média déjà chargé.</summary>
public interface IMediaBlockRepresentation : IMediaImageRepresentation
{
    long Capacity { get; }

    int LogicalBlockSize { get; }

    long LogicalBlockCount { get; }

    ValueTask ReadExactlyAsync(long address, Memory<byte> destination, CancellationToken cancellationToken = default);
}
