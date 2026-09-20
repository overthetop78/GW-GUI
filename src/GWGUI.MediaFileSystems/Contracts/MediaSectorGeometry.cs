namespace GWGUI.MediaFileSystems.Contracts;

/// <summary>Géométrie cible résolue par le moteur avant construction du système de fichiers.</summary>
public sealed record MediaSectorGeometry(
    string FormatId,
    int SectorSize,
    int Cylinders,
    int Heads,
    int SectorsPerTrack,
    int BlockCount);
