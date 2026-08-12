using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Reconnaît un volume Commodore DOS et conserve les données déjà lues.</summary>
internal static class CommodoreDosRecognizer
{
    /// <summary>Tente de reconnaître l'en-tête et une chaîne de répertoire plausible.</summary>
    public static bool TryRecognize(SectorImage image, out CommodoreDosRecognition recognition)
    {
        recognition = null!;
        var layout = CommodoreDosLayout.Resolve(image.FormatId);
        if (layout is null || image.BlockSize != CommodoreDosLayout.SectorSize || !CommodoreDosSectorReader.TryRead(image, layout.HeaderTrack, layout.HeaderSector, out var header)) return false;
        if (header[CommodoreDosLayout.DirectoryEntriesOffset] != layout.HeaderSignature) return false;
        var directoryTrack = header[CommodoreDosLayout.NextTrackOffset] == 0 ? layout.DirectoryTrack : header[CommodoreDosLayout.NextTrackOffset];
        var directorySector = header[CommodoreDosLayout.NextTrackOffset] == 0 ? layout.DirectorySector : header[CommodoreDosLayout.NextSectorOffset];
        if (!CommodoreDosDirectoryReader.IsPlausible(image, directoryTrack, directorySector)) return false;
        recognition = new(layout, header, directoryTrack, directorySector);
        return true;
    }
}
