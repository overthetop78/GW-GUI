using GWGUI.App.Constants.Rendering.Blocks;
using GWGUI.App.Contracts.Rendering.Blocks;
using SkiaSharp;

namespace GWGUI.App.Rendering.Blocks;

public sealed class SkiaHardDiskGeometryRenderer
{
    public void Render(SKCanvas canvas, BlockMediaRenderModel? model, int width, int height, int? selectedSurface = null, float zoom = 1)
    {
        canvas.Clear(BlockMediaRenderConstants.BackgroundColor);
        var geometry = model?.Geometry;
        if (geometry is null || geometry.Heads <= 0 || geometry.Cylinders <= 0) return;

        var columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(geometry.Heads)));
        var rows = Math.Max(1, (int)Math.Ceiling(geometry.Heads / (double)columns));
        var cellWidth = width / (float)columns;
        var cellHeight = height / (float)rows;
        var radius = Math.Max(1, Math.Min(cellWidth, cellHeight) / 2f - BlockMediaRenderConstants.OuterMargin) * zoom;
        using var surfacePaint = new SKPaint { Color = BlockMediaRenderConstants.UnknownColor, IsAntialias = true };
        using var trackPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };
        using var hubPaint = new SKPaint { Color = BlockMediaRenderConstants.BackgroundColor, IsAntialias = true };
        using var selectedPaint = new SKPaint
        {
            Color = BlockMediaRenderConstants.SelectionColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = BlockMediaRenderConstants.SelectionStrokeWidth
        };

        for (var head = 0; head < geometry.Heads; head++)
        {
            var column = head % columns;
            var row = head / columns;
            var center = new SKPoint(column * cellWidth + cellWidth / 2, row * cellHeight + cellHeight / 2);
            canvas.DrawCircle(center, radius, surfacePaint);
            var inner = radius * BlockMediaRenderConstants.InnerRadiusRatio;
            var step = Math.Max(1L, (long)Math.Ceiling(geometry.Cylinders / Math.Max(1, radius - inner)));
            for (long cylinder = 0; cylinder < geometry.Cylinders; cylinder += step)
            {
                var count = Math.Min(step, geometry.Cylinders - cylinder);
                var track = (cylinder * geometry.Heads + head) * geometry.SectorsPerTrack;
                var range = FindRange(model!.Ranges, track);
                trackPaint.Color = range is null ? BlockMediaRenderConstants.UnknownColor : SkiaBlockMediaRenderer.ColorFor(range.State);
                trackPaint.StrokeWidth = Math.Max(1, (radius - inner) * count / geometry.Cylinders);
                canvas.DrawCircle(center, radius - (radius - inner) * (cylinder + count / 2f) / geometry.Cylinders, trackPaint);
            }
            canvas.DrawCircle(center, radius * BlockMediaRenderConstants.InnerRadiusRatio, hubPaint);
            if (selectedSurface == head) canvas.DrawCircle(center, radius, selectedPaint);
        }
    }

    public int? HitTest(BlockMediaRenderModel? model, int width, int height, SKPoint point, float zoom = 1)
    {
        var geometry = model?.Geometry;
        if (geometry is null || geometry.Heads <= 0) return null;
        var columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(geometry.Heads)));
        var rows = Math.Max(1, (int)Math.Ceiling(geometry.Heads / (double)columns));
        var cellWidth = width / (float)columns;
        var cellHeight = height / (float)rows;
        var column = Math.Clamp((int)(point.X / cellWidth), 0, columns - 1);
        var row = Math.Clamp((int)(point.Y / cellHeight), 0, rows - 1);
        var head = row * columns + column;
        if (head >= geometry.Heads) return null;
        var center = new SKPoint(column * cellWidth + cellWidth / 2, row * cellHeight + cellHeight / 2);
        var radius = Math.Max(1, Math.Min(cellWidth, cellHeight) / 2f - BlockMediaRenderConstants.OuterMargin) * zoom;
        var distance = SKPoint.Distance(point, center);
        return distance <= radius && distance >= radius * BlockMediaRenderConstants.InnerRadiusRatio ? head : null;
    }

    /// <summary>Traduit une piste visible en adresse LBA, en tenant compte du zoom et de la surface.</summary>
    public long? HitTestPosition(BlockMediaRenderModel? model, int width, int height, SKPoint point, float zoom = 1)
    {
        if (HitTest(model, width, height, point, zoom) is not { } head || model?.Geometry is not { } geometry) return null;
        var columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(geometry.Heads)));
        var rows = Math.Max(1, (int)Math.Ceiling(geometry.Heads / (double)columns));
        var cellWidth = width / (float)columns; var cellHeight = height / (float)rows;
        var center = new SKPoint(head % columns * cellWidth + cellWidth / 2, head / columns * cellHeight + cellHeight / 2);
        var radius = Math.Max(1, Math.Min(cellWidth, cellHeight) / 2f - BlockMediaRenderConstants.OuterMargin) * zoom;
        var cylinder = Math.Clamp((long)((radius - SKPoint.Distance(point, center)) / (radius * (1 - BlockMediaRenderConstants.InnerRadiusRatio)) * geometry.Cylinders), 0, geometry.Cylinders - 1);
        return (cylinder * geometry.Heads + head) * geometry.SectorsPerTrack;
    }

    private static BlockMediaRange? FindRange(IReadOnlyList<BlockMediaRange> ranges, long position)
    {
        var low = 0; var high = ranges.Count - 1;
        while (low <= high)
        {
            var middle = low + (high - low) / 2;
            var range = ranges[middle];
            if (position < range.Start) high = middle - 1;
            else if (position >= range.Start + range.Length) low = middle + 1;
            else return range;
        }
        return null;
    }
}
