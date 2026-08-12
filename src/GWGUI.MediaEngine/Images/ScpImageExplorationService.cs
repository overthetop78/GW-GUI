using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Exploration.Documents;
using GWGUI.MediaEngine.Exploration.Interpretation;
using GWGUI.MediaEngine.Images.ScpDetection;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

public sealed class ScpImageExplorationService
{
    private readonly ScpAutomaticImageExplorer automaticExplorer;
    private readonly ScpSectorImageReader sectorImageReader;

    internal ScpImageExplorationService(ScpCandidateRegistry candidates, ScpFamilyProbe familyProbe, FileSystemRegistry fileSystems, DiskImageInterpretationService interpretations, DiskImageDocumentFactory documents)
    {
        automaticExplorer = new(candidates, familyProbe, fileSystems, interpretations, documents);
        sectorImageReader = new(candidates, fileSystems);
    }

    public Task<ExploredDiskImage> ExploreAutomaticallyAsync(string path, CancellationToken cancellationToken) =>
        automaticExplorer.ExploreAsync(path, cancellationToken);

    public Task<SectorImage> ReadAsync(string path, string? formatId, CancellationToken cancellationToken) =>
        sectorImageReader.ReadAsync(path, formatId, cancellationToken);
}
