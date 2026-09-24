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
        float zoom = 1,
        IReadOnlySet<(int Surface, int Cylinder)>? revealedTracks = null)
    {
        canvas.Clear(SectorMediaRenderConstants.BackgroundColor);
        var mediaSurface = model?.Surfaces.FirstOrDefault(item => item.Index == surface);
        if (mediaSurface is null || mediaSurface.Tracks.Count == 0) return;
        if (model!.LayoutKind == SectorMediaLayoutKind.Logical)
        {
            RenderLogical(canvas, mediaSurface, selectedPosition, width, height, zoom, revealedTracks);
            return;
        }

        var center = new SKPoint(width / 2f, height / 2f);
        var outerRadius = Math.Max(1, Math.Min(width, height) / 2f - SectorMediaRenderConstants.OuterMargin) * zoom;
        var innerRadius = outerRadius * SectorMediaRenderConstants.InnerRadiusRatio;
        var tracks = direction == MediaVisualizationDirection.Descending
            ? mediaSurface.Tracks.OrderByDescending(track => track.Cylinder).ToArray()
            : mediaSurface.Tracks.OrderBy(track => track.Cylinder).ToArray();
        var trackWidth = (outerRadius - innerRadius) / tracks.Length;
        using var trackBoundary = new SKPaint { Color = SectorMediaRenderConstants.PendingTrackBoundaryColor, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };

        for (var trackIndex = 0; trackIndex < tracks.Length; trackIndex++)
        {
            var sectors = tracks[trackIndex].Sectors.OrderBy(sector => sector.Number).ToArray();
            if (sectors.Length == 0) continue;
            var outer = outerRadius - trackIndex * trackWidth;
            var inner = Math.Max(innerRadius, outer - trackWidth);
            canvas.DrawCircle(center, outer, trackBoundary);
            var sweep = 360f / sectors.Length;
            for (var sectorIndex = 0; sectorIndex < sectors.Length; sectorIndex++)
            {
                var sector = sectors[sectorIndex];
                var start = -90f + sectorIndex * sweep + SectorMediaRenderConstants.SectorGapDegrees / 2;
                var visibleSweep = Math.Max(
                    SectorMediaRenderConstants.MinimumSweepDegrees,
                    sweep - SectorMediaRenderConstants.SectorGapDegrees);
                using var path = CreateRingSector(center, inner, outer, start, visibleSweep);
                var revealed = revealedTracks is null || revealedTracks.Contains((surface, tracks[trackIndex].Cylinder));
                using var fill = new SKPaint
                {
                    Color = revealed ? ColorFor(sector.State) : SectorMediaRenderConstants.PendingColor,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };
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
        if (model!.LayoutKind == SectorMediaLayoutKind.Logical)
            return HitTestLogical(mediaSurface, width, height, point, zoom);
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

    private static void RenderLogical(
        SKCanvas canvas,
        SectorMediaSurface surface,
        long? selectedPosition,
        int width,
        int height,
        float zoom,
        IReadOnlySet<(int Surface, int Cylinder)>? revealedTracks)
    {
        var sectors = surface.Tracks.SelectMany(track => track.Sectors).OrderBy(sector => sector.LogicalBlock).ToArray();
        if (sectors.Length == 0) return;
        var layout = LogicalGrid(sectors.Length, width, height, zoom);
        var revealed = revealedTracks is null || revealedTracks.Contains((surface.Index, 0));
        for (var index = 0; index < sectors.Length; index++)
        {
            var bounds = LogicalCell(layout, index);
            using var fill = new SKPaint
            {
                Color = revealed ? ColorFor(sectors[index].State) : SectorMediaRenderConstants.PendingColor,
                IsAntialias = false,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRect(bounds, fill);
            if (sectors[index].Position != selectedPosition) continue;
            using var selection = new SKPaint
            {
                Color = SectorMediaRenderConstants.SelectionColor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = SectorMediaRenderConstants.SelectionStrokeWidth
            };
            canvas.DrawRect(bounds, selection);
        }
    }

    private static SectorMediaElement? HitTestLogical(
        SectorMediaSurface surface,
        int width,
        int height,
        SKPoint point,
        float zoom)
    {
        var sectors = surface.Tracks.SelectMany(track => track.Sectors).OrderBy(sector => sector.LogicalBlock).ToArray();
        if (sectors.Length == 0) return null;
        var layout = LogicalGrid(sectors.Length, width, height, zoom);
        if (point.X < layout.Left || point.Y < layout.Top || point.X >= layout.Right || point.Y >= layout.Bottom)
            return null;
        var column = (int)((point.X - layout.Left) / layout.CellWidth);
        var row = (int)((point.Y - layout.Top) / layout.CellHeight);
        if (column < 0 || column >= layout.Columns || row < 0 || row >= layout.Rows) return null;
        var index = row * layout.Columns + column;
        return index < sectors.Length ? sectors[index] : null;
    }

    private static LogicalGridLayout LogicalGrid(int count, int width, int height, float zoom)
    {
        var availableWidth = Math.Max(1f, width - SectorMediaRenderConstants.LogicalGridMargin * 2) * zoom;
        var availableHeight = Math.Max(1f, height - SectorMediaRenderConstants.LogicalGridMargin * 2) * zoom;
        var columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(count * availableWidth / availableHeight)));
        var rows = Math.Max(1, (count + columns - 1) / columns);
        var cellWidth = availableWidth / columns;
        var cellHeight = availableHeight / rows;
        var left = (width - availableWidth) / 2f;
        var top = (height - availableHeight) / 2f;
        return new(left, top, columns, rows, cellWidth, cellHeight);
    }

    private static SKRect LogicalCell(LogicalGridLayout layout, int index)
    {
        var row = index / layout.Columns;
        var column = index % layout.Columns;
        var gap = SectorMediaRenderConstants.LogicalGridGap;
        var left = layout.Left + column * layout.CellWidth + gap / 2;
        var top = layout.Top + row * layout.CellHeight + gap / 2;
        return new(left, top, left + Math.Max(1f, layout.CellWidth - gap), top + Math.Max(1f, layout.CellHeight - gap));
    }

    private sealed record LogicalGridLayout(float Left, float Top, int Columns, int Rows, float CellWidth, float CellHeight)
    {
        public float Right => Left + Columns * CellWidth;
        public float Bottom => Top + Rows * CellHeight;
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

    internal static SKColor ColorFor(SectorMediaElementState state) => state switch
    {
        SectorMediaElementState.WithData => SectorMediaRenderConstants.WithDataColor,
        SectorMediaElementState.WithoutData => SectorMediaRenderConstants.WithoutDataColor,
        SectorMediaElementState.Degraded => SectorMediaRenderConstants.DegradedColor,
        SectorMediaElementState.Dead => SectorMediaRenderConstants.DeadColor,
        _ => SectorMediaRenderConstants.WithoutDataColor
    };

    internal static SKColor UnrevealedColor => SectorMediaRenderConstants.PendingColor;

    internal static SKColor UnrevealedTrackBoundaryColor => SectorMediaRenderConstants.PendingTrackBoundaryColor;
}
