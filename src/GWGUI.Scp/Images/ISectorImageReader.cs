using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public interface ISectorImageReader
{
    bool CanRead(string path);
    Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default);
}
