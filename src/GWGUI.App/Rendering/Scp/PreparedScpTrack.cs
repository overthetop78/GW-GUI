using GWGUI.App.Enums.Rendering.Scp;
using SkiaSharp;

namespace GWGUI.App.Rendering.Scp;

public sealed partial class SkiaScpRenderer
{
    private sealed record PreparedScpTrack(
        IReadOnlyList<PreparedScpArc> FluxArcs,
        IReadOnlyList<PreparedScpArc> StructureArcs,
        ScpTrackVisualState VisualState,
        int ValidSectors,
        int InvalidSectors,
        int UnverifiedSectors,
        bool HasFlux);
}
