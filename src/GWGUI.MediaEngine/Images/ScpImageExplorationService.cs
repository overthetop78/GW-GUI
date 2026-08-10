using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images.ScpDetection;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

public sealed class ScpImageExplorationService
{
    private readonly ScpAutomaticImageExplorer automaticExplorer;
    private readonly ScpSectorImageReader sectorImageReader;

    internal ScpImageExplorationService(
        ScpCandidateRegistry candidates,
        ScpFamilyProbe familyProbe,
        FileSystemRegistry fileSystems)
    {
        var interpretations = new DiskImageInterpretationService(fileSystems);
        automaticExplorer = new(candidates, familyProbe, fileSystems, interpretations);
        sectorImageReader = new(candidates, fileSystems);
    }

    public Task<ExploredDiskImage> ExploreAutomaticallyAsync(string path, CancellationToken cancellationToken) =>
        automaticExplorer.ExploreAsync(path, cancellationToken);

    public Task<SectorImage> ReadAsync(string path, string? formatId, CancellationToken cancellationToken) =>
        sectorImageReader.ReadAsync(path, formatId, cancellationToken);
}
