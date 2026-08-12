using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Scp;

namespace GWGUI.MediaEngine.Exploration.Scp;

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
    public Task<ExploredDiskImage> ExploreAutomaticallyAsync(string path, CancellationToken cancellationToken) => automaticExplorer.ExploreAsync(path, cancellationToken);
    /// <summary>Délègue la reconstruction du chemin et du format explicitement demandé.</summary>
    public Task<SectorImage> ReadAsync(string path, string? formatId, CancellationToken cancellationToken) => sectorImageReader.ReadAsync(path, formatId, cancellationToken);
}
