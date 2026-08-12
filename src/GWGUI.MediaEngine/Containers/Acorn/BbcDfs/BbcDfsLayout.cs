using GWGUI.MediaEngine.Geometries.Acorn;

namespace GWGUI.MediaEngine.Containers.Acorn.BbcDfs;

/// <summary>Décrit l'ordre DSD où les pistes complètes sont entrelacées par face pour chaque cylindre.</summary>
internal static class BbcDfsLayout
{
    /// <summary>Calcule l'offset source d'un secteur dans l'ordre cylindre, face, secteur du fichier.</summary>
    public static int SourceOffset(int cylinder, int head, int sector, int heads) => checked(((cylinder * heads + head) * BbcDfsGeometry.SectorsPerTrack + sector) * BbcDfsGeometry.SectorSize);
}
