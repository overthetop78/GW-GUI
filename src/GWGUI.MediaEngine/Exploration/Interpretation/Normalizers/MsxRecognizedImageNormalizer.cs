using GWGUI.MediaEngine.Exploration.Interpretation.Contracts;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaEngine.Images.Reading.Recognition.Msx;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Exploration.Interpretation.Normalizers;

/// <summary>Normalise une image MSX uniquement après reconnaissance effective de FAT12.</summary>
internal sealed class MsxRecognizedImageNormalizer(MsxSectorImageInterpreter interpreter) : IRecognizedImageNormalizer
{
    /// <summary>Transmet l'image à l'interpréteur commun lorsque le Reader est FAT12.</summary>
    public bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized)
    {
        normalized = image;
        return readerId.Equals(FileSystemIds.Fat12, StringComparison.OrdinalIgnoreCase) && interpreter.TryInterpret(image, out normalized);
    }
}
