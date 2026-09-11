using GWGUI.App.Enums.Rendering.Scp;
using SkiaSharp;

namespace GWGUI.App.Rendering.Scp;

public sealed partial class SkiaScpRenderer
{
    private sealed record PreparedScpTrack(
        IReadOnlyList<PreparedScpRevolution> Revolutions,
        IReadOnlyList<PreparedScpArc> StructureArcs,
        ScpTrackVisualState VisualState,
        int ValidSectors,
        int InvalidSectors,
        int UnverifiedSectors,
        bool HasFlux);

    private sealed record PreparedScpRevolution(IReadOnlyList<PreparedScpArc> FluxArcs);
}
