namespace GWGUI.MediaFileSystems.Contracts;

/// <summary>Bloc logique et données produits par un constructeur de système de fichiers cible.</summary>
public sealed record MediaSectorWriteBlock(
    int LogicalBlock,
    MediaSectorAddress Address,
    IReadOnlyList<byte> Data);
