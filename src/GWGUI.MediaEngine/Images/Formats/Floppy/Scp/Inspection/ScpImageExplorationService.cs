using GWGUI.MediaEngine.Contracts.Explorer;
using GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Decoding.Sectors;

using GWGUI.MediaEngine.Images.Formats.Floppy.Scp;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Scp.Inspection;

/// <summary>Façade de l'exploration automatique et de la reconstruction sectorielle des captures SCP.</summary>
public sealed class ScpImageExplorationService
{
    private readonly ScpAutomaticImageExplorer automaticExplorer;
    private readonly ScpSectorImageReader sectorImageReader;

    /// <summary>Construit la façade avec les deux services auxquels elle délègue.</summary>
    internal ScpImageExplorationService(ScpAutomaticImageExplorer automaticExplorer, ScpSectorImageReader sectorImageReader)
    {
        this.automaticExplorer = automaticExplorer;
        this.sectorImageReader = sectorImageReader;
    }
    /// <summary>Délègue l'exploration automatique du chemin et propage l'annulation et les erreurs.</summary>
    public Task<ExploredDiskImage> ExploreAutomaticallyAsync(string path, IProgress<ScpExplorationProgress>? progress, CancellationToken cancellationToken) => automaticExplorer.ExploreAsync(path, progress, cancellationToken);
    /// <summary>Explore une capture SCP déjà disponible en mémoire.</summary>
    public Task<ExploredDiskImage> ExploreAutomaticallyAsync(
        string path,
        ScpImage image,
        IProgress<ScpExplorationProgress>? progress,
        CancellationToken cancellationToken) => automaticExplorer.ExploreAsync(path, image, progress, cancellationToken);
    /// <summary>Délègue la reconstruction du chemin et du format explicitement demandé.</summary>
    public Task<SectorImage> ReadAsync(string path, string? formatId, CancellationToken cancellationToken) => sectorImageReader.ReadAsync(path, formatId, cancellationToken);
}
