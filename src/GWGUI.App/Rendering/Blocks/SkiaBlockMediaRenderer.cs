using GWGUI.App.Constants.Rendering.Blocks;
using GWGUI.App.Contracts.Rendering.Blocks;
using GWGUI.App.Enums.Rendering.Blocks;
using SkiaSharp;

namespace GWGUI.App.Rendering.Blocks;

public sealed class SkiaBlockMediaRenderer
{
    public void Render(SKCanvas canvas, BlockMediaRenderModel? model, BlockMediaRange? selectedRange, int width, int height, float zoom = 1)
    {
        canvas.Clear(BlockMediaRenderConstants.BackgroundColor);
        if (model is null || model.LogicalLength <= 0) return;
        var center = new SKPoint(width / 2f, height / 2f);
        var outer = Math.Max(1, Math.Min(width, height) / 2f - BlockMediaRenderConstants.OuterMargin) * zoom;
        var inner = outer * BlockMediaRenderConstants.InnerRadiusRatio;
        using (var unknown = new SKPaint { Color = BlockMediaRenderConstants.UnknownColor, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = outer - inner })
            canvas.DrawCircle(center, (outer + inner) / 2, unknown);

        foreach (var range in Aggregate(model.Ranges))
        {
            var start = -90f + 360f * range.Start / model.LogicalLength;
            var sweep = Math.Max(.08f, 360f * range.Length / model.LogicalLength);
            using var paint = new SKPaint
            {
                Color = ColorFor(range.State),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = outer - inner,
                StrokeCap = SKStrokeCap.Butt
            };
            var radius = (outer + inner) / 2;
            var rect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
            canvas.DrawArc(rect, start, sweep, false, paint);
        }

        if (selectedRange is null) return;
        var selectedStart = -90f + 360f * selectedRange.Start / model.LogicalLength;
        var selectedSweep = Math.Max(.08f, 360f * selectedRange.Length / model.LogicalLength);
        using var selection = new SKPaint
        {
            Color = BlockMediaRenderConstants.SelectionColor,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = BlockMediaRenderConstants.SelectionStrokeWidth
        };
        var selectionRect = new SKRect(center.X - outer, center.Y - outer, center.X + outer, center.Y + outer);
        canvas.DrawArc(selectionRect, selectedStart, selectedSweep, false, selection);
    }

    public BlockMediaRange? HitTest(BlockMediaRenderModel? model, int width, int height, SKPoint point, float zoom = 1)
    {
        if (model is null || model.LogicalLength <= 0) return null;
        var center = new SKPoint(width / 2f, height / 2f);
        var outer = Math.Max(1, Math.Min(width, height) / 2f - BlockMediaRenderConstants.OuterMargin) * zoom;
        var inner = outer * BlockMediaRenderConstants.InnerRadiusRatio;
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;
        var radius = MathF.Sqrt(dx * dx + dy * dy);
        if (radius < inner || radius > outer) return null;
        var angle = (MathF.Atan2(dy, dx) * 180f / MathF.PI + 450f) % 360f;
        var address = Math.Min(model.LogicalLength - 1, (long)(angle / 360f * model.LogicalLength));
        return model.Ranges.FirstOrDefault(range => address >= range.Start && address < range.Start + range.Length);
    }

    private static IReadOnlyList<BlockMediaRange> Aggregate(IReadOnlyList<BlockMediaRange> ranges)
    {
        if (ranges.Count <= BlockMediaRenderConstants.MaximumRenderedRanges) return ranges;
        var groupSize = (int)Math.Ceiling(ranges.Count / (double)BlockMediaRenderConstants.MaximumRenderedRanges);
        return ranges.Chunk(groupSize).Select(group =>
        {
            var start = group[0].Start;
            var end = group.Max(item => checked(item.Start + item.Length));
            var state = group.All(item => item.State == group[0].State) ? group[0].State : BlockMediaRangeState.Unknown;
            return new BlockMediaRange(start, end - start, state);
        }).ToArray();
    }

    private static SKColor ColorFor(BlockMediaRangeState state) => state switch
    {
        BlockMediaRangeState.Available => BlockMediaRenderConstants.AvailableColor,
        BlockMediaRangeState.Reserved => BlockMediaRenderConstants.ReservedColor,
        BlockMediaRangeState.Allocated => BlockMediaRenderConstants.AllocatedColor,
        BlockMediaRangeState.Free => BlockMediaRenderConstants.FreeColor,
        _ => BlockMediaRenderConstants.UnknownColor
    };
}
