using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Traitements vidéo partagés, indépendants du cœur et de l’identité du module.</summary>
/// <remarks>Le service ne possède aucune surface. Les pointeurs et tableaux reçus sont empruntés pendant l’appel ; leur validité et leur libération restent à la charge du propriétaire.</remarks>
public interface IEmulationVideoProcessing
{
    /// <summary>Actualise le rapport d’image avec la géométrie annoncée, selon les règles de repli existantes.</summary>
    void ApplyGeometry(ExternalCoreApi.Geometry geometry, ref float aspectRatio);
    /// <summary>Calcule la longueur d’une frame en octets avec les contrôles de dépassement existants.</summary>
    int FrameLength(uint height, nuint pitch);
    /// <summary>Copie chaque ligne de la zone native vers le tableau emprunté, sans allouer de tableau.</summary>
    /// <remarks>Le propriétaire garantit la validité du pointeur et la capacité du tableau pendant toute la copie.</remarks>
    void CopyRows(nint source, byte[] destination, int height, int pitch);
    /// <summary>Copie la zone native dans un nouveau tableau dont la propriété est transférée à l’appelant.</summary>
    /// <remarks>La copie conserve la hauteur et le pas en octets ; elle ne réutilise aucun tampon de pool.</remarks>
    byte[] CopyFramePixels(nint data, uint height, nuint pitch);
    /// <summary>Calcule le temps écoulé depuis un horodatage Stopwatch, dans la même base temporelle.</summary>
    TimeSpan Timestamp(long startTimestamp);
    /// <summary>Retourne le nom invariant du standard vidéo.</summary>
    string FormatStandard(EmulationVideoStandard standard);
    /// <summary>Formate une fréquence positive et finie en hertz selon la culture invariante.</summary>
    string FormatFrequency(double frequencyHz);
    /// <summary>Réunit le standard et la fréquence selon le format partagé existant.</summary>
    string FormatTiming(EmulationVideoStandard standard, double frequencyHz);
}
