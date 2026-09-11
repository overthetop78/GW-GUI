using GWGUI.App.Constants.Rendering.Sectors;
using GWGUI.App.Contracts.Rendering.Sectors;
using GWGUI.App.Enums.Rendering.Sectors;
using GWGUI.MediaEngine.Enums;
using SkiaSharp;

namespace GWGUI.App.Rendering.Sectors;

public sealed class SkiaSectorMediaRenderer
{
    public void Render(
        SKCanvas canvas,
        SectorMediaRenderModel? model,
        int surface,
        MediaVisualizationDirection direction,
        long? selectedPosition,
        int width,
        int height,
        float zoom = 1)
    {
        canvas.Clear(SectorMediaRenderConstants.BackgroundColor);
        var mediaSurface = model?.Surfaces.FirstOrDefault(item => item.Index == surface);
        if (mediaSurface is null || mediaSurface.Tracks.Count == 0) return;

        var center = new SKPoint(width / 2f, height / 2f);
        var outerRadius = Math.Max(1, Math.Min(width, height) / 2f - SectorMediaRenderConstants.OuterMargin) * zoom;
        var innerRadius = outerRadius * SectorMediaRenderConstants.InnerRadiusRatio;
        var tracks = direction == MediaVisualizationDirection.Descending
            ? mediaSurface.Tracks.OrderByDescending(track => track.Cylinder).ToArray()
            : mediaSurface.Tracks.OrderBy(track => track.Cylinder).ToArray();
        var trackWidth = (outerRadius - innerRadius) / tracks.Length;

        for (var trackIndex = 0; trackIndex < tracks.Length; trackIndex++)
        {
            var sectors = tracks[trackIndex].Sectors.OrderBy(sector => sector.Number).ToArray();
            if (sectors.Length == 0) continue;
            var outer = outerRadius - trackIndex * trackWidth;
            var inner = Math.Max(innerRadius, outer - trackWidth);
            var sweep = 360f / sectors.Length;
            for (var sectorIndex = 0; sectorIndex < sectors.Length; sectorIndex++)
            {
                var sector = sectors[sectorIndex];
                var start = -90f + sectorIndex * sweep + SectorMediaRenderConstants.SectorGapDegrees / 2;
                var visibleSweep = Math.Max(
                    SectorMediaRenderConstants.MinimumSweepDegrees,
                    sweep - SectorMediaRenderConstants.SectorGapDegrees);
                using var path = CreateRingSector(center, inner, outer, start, visibleSweep);
                using var fill = new SKPaint { Color = ColorFor(sector.State), IsAntialias = true, Style = SKPaintStyle.Fill };
                canvas.DrawPath(path, fill);
                if (sector.Position != selectedPosition) continue;
                using var selection = new SKPaint
                {
                    Color = SectorMediaRenderConstants.SelectionColor,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = SectorMediaRenderConstants.SelectionStrokeWidth
                };
                canvas.DrawPath(path, selection);
            }
        }
    }

    public SectorMediaElement? HitTest(
        SectorMediaRenderModel? model,
        int surface,
        MediaVisualizationDirection direction,
        int width,
        int height,
        SKPoint point,
        float zoom = 1)
    {
        var mediaSurface = model?.Surfaces.FirstOrDefault(item => item.Index == surface);
        if (mediaSurface is null || mediaSurface.Tracks.Count == 0) return null;
        var center = new SKPoint(width / 2f, height / 2f);
        var outerRadius = Math.Max(1, Math.Min(width, height) / 2f - SectorMediaRenderConstants.OuterMargin) * zoom;
        var innerRadius = outerRadius * SectorMediaRenderConstants.InnerRadiusRatio;
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;
        var radius = MathF.Sqrt(dx * dx + dy * dy);
        if (radius < innerRadius || radius > outerRadius) return null;

        var tracks = direction == MediaVisualizationDirection.Descending
            ? mediaSurface.Tracks.OrderByDescending(track => track.Cylinder).ToArray()
            : mediaSurface.Tracks.OrderBy(track => track.Cylinder).ToArray();
        var trackWidth = (outerRadius - innerRadius) / tracks.Length;
        var trackIndex = Math.Clamp((int)((outerRadius - radius) / trackWidth), 0, tracks.Length - 1);
        var sectors = tracks[trackIndex].Sectors.OrderBy(sector => sector.Number).ToArray();
        if (sectors.Length == 0) return null;
        var angle = (MathF.Atan2(dy, dx) * 180f / MathF.PI + 450f) % 360f;
        var sectorIndex = Math.Clamp((int)(angle / (360f / sectors.Length)), 0, sectors.Length - 1);
        return sectors[sectorIndex];
    }

    private static SKPath CreateRingSector(SKPoint center, float innerRadius, float outerRadius, float start, float sweep)
    {
        var outer = new SKRect(center.X - outerRadius, center.Y - outerRadius, center.X + outerRadius, center.Y + outerRadius);
        var inner = new SKRect(center.X - innerRadius, center.Y - innerRadius, center.X + innerRadius, center.Y + innerRadius);
        var path = new SKPath();
        path.AddArc(outer, start, sweep);
        path.ArcTo(inner, start + sweep, -sweep, false);
        path.Close();
        return path;
    }

    private static SKColor ColorFor(SectorMediaElementState state) => state switch
    {
        SectorMediaElementState.Available => SectorMediaRenderConstants.AvailableColor,
        SectorMediaElementState.Missing => SectorMediaRenderConstants.MissingColor,
        SectorMediaElementState.IntegrityUnknown => SectorMediaRenderConstants.IntegrityUnknownColor,
        SectorMediaElementState.IntegrityInvalid => SectorMediaRenderConstants.IntegrityInvalidColor,
        _ => SectorMediaRenderConstants.MissingColor
    };
}
