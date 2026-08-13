using GWGUI.MediaEngine.Encoding.Rare;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Visualization.Policies;

/// <summary>Raccorde les images sectorielles rares aux encodeurs de pistes déjà catalogués.</summary>
internal sealed class RareEncodedVisualizationPolicy : SectorImageVisualizationPolicy
{
    public override bool CanHandle(SectorImage image) => RareTrackEncodingProfiles.TryResolve(image.FormatId, out _);

    public override string EncoderId(SectorImage image) => Resolve(image).EncoderId;

    public override uint BitCellTicks(SectorImage image, int cylinder) => Resolve(image).BitCellTicks;

    public override uint IndexTimeTicks(SectorImage image, int cylinder) => Resolve(image).IndexTimeTicks;

    private static RareTrackEncodingProfile Resolve(SectorImage image) =>
        RareTrackEncodingProfiles.TryResolve(image.FormatId, out var profile)
            ? profile
            : throw new InvalidOperationException($"Aucun profil d'encodage rare n'est associé au format '{image.FormatId}'.");
}
