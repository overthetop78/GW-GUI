using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Lecteur sectoriel utilisé par l’inspection des candidats SCP.</summary>
public interface IFileSystemReader
{
    string Id { get; }
    IReadOnlySet<string> CatalogFormatIds { get; }
    bool CanRead(SectorImage image);
    FileSystemVolume Read(SectorImage image);
}
