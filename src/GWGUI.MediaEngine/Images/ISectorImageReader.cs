using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

public interface ISectorImageReader
{
    bool CanRead(string path);
    Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default);
}
