using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Visualization.Policies;

namespace GWGUI.MediaEngine.Visualization;

/// <summary>Résout, dans un ordre stable, la politique de visualisation d'une image sectorielle.</summary>
internal sealed class SectorImageVisualizationPolicyRegistry
{
    private readonly IReadOnlyList<ISectorImageVisualizationPolicy> policies;

    /// <summary>Crée le catalogue de politiques par défaut.</summary>
    public SectorImageVisualizationPolicyRegistry() : this(
    [
        new AppleVisualizationPolicy(),
        new CommodoreVisualizationPolicy(),
        new DecRx02VisualizationPolicy(),
        new AtariVisualizationPolicy(),
        new PrefixVisualizationPolicy(FluxCodecIds.AmigaMfm, DiskImageFormatIds.AmigaPrefix),
        new PrefixVisualizationPolicy(FluxCodecIds.IsoFm, DiskImageFormatIds.AcornDfsPrefix),
        new PrefixVisualizationPolicy(FluxCodecIds.IsoMfm, DiskImageFormatIds.AcornAdfsPrefix),
        new PrefixVisualizationPolicy(FluxCodecIds.IsoMfm, DiskImageFormatIds.IbmPrefix, DiskImageFormatIds.AmstradPrefix, DiskImageFormatIds.MsxPrefix, DiskImageFormatIds.UcsdPrefix, DiskImageFormatIds.EpsonQx10Prefix),
        new ExactVisualizationPolicy(FluxCodecIds.IsoMfm, DiskImageFormatIds.Imd, DiskImageFormatIds.Td0)
    ]) { }

    /// <summary>Crée un registre depuis une collection ordonnée de politiques.</summary>
    /// <param name="policies">Politiques dans leur ordre de priorité.</param>
    /// <exception cref="ArgumentNullException">La collection ou l'une de ses entrées est nulle.</exception>
    public SectorImageVisualizationPolicyRegistry(IEnumerable<ISectorImageVisualizationPolicy> policies)
    {
        ArgumentNullException.ThrowIfNull(policies);
        var copy = policies.ToArray();
        if (copy.Any(policy => policy is null)) throw new ArgumentException("Une politique de visualisation ne peut pas être nulle.", nameof(policies));
        this.policies = Array.AsReadOnly(copy);
    }

    /// <summary>Retourne la première politique compatible.</summary>
    /// <param name="image">Image sectorielle à résoudre.</param>
    /// <returns>Première politique compatible, ou <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException">L'image est nulle.</exception>
    public ISectorImageVisualizationPolicy? Resolve(SectorImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        return policies.FirstOrDefault(policy => policy.CanHandle(image));
    }
}
