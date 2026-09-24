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
        using var surfacePaint = new SKPaint { Color = BlockMediaRenderConstants.AvailableColor, IsAntialias = true };
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
            canvas.DrawCircle(center, radius * BlockMediaRenderConstants.InnerRadiusRatio, hubPaint);
            if (selectedSurface == head) canvas.DrawCircle(center, radius, selectedPaint);
        }
    }

    public int? HitTest(BlockMediaRenderModel? model, int width, int height, SKPoint point)
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
        return head < geometry.Heads ? head : null;
    }
}
