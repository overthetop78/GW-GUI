namespace GWGUI.MediaEngine.Encoding;

/// <summary>Associe une piste encodée à son cylindre, sa face et sa durée de cellule.</summary>
public sealed record EncodedDiskTrack(int Cylinder, int Head, uint BitCellTicks, EncodedTrack Track);
