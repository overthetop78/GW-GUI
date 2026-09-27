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
        if (model.Shape == BlockMediaShape.Cartridge)
        {
            RenderCartridge(canvas, model, selectedRange, width, height, zoom);
            return;
        }
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
        if (model.Shape == BlockMediaShape.Cartridge)
            return HitTestCartridge(model, width, height, point, zoom);
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

    private static void RenderCartridge(
        SKCanvas canvas,
        BlockMediaRenderModel model,
        BlockMediaRange? selectedRange,
        int width,
        int height,
        float zoom)
    {
        var body = CartridgeBounds(width, height, zoom);
        using (var bodyPaint = new SKPaint
               {
                   Color = BlockMediaRenderConstants.CartridgeBodyColor,
                   IsAntialias = true,
                   Style = SKPaintStyle.Fill
               })
            canvas.DrawRoundRect(body, BlockMediaRenderConstants.CartridgeCornerRadius,
                BlockMediaRenderConstants.CartridgeCornerRadius, bodyPaint);
        using (var outline = new SKPaint
               {
                   Color = BlockMediaRenderConstants.CartridgeOutlineColor,
                   IsAntialias = true,
                   Style = SKPaintStyle.Stroke,
                   StrokeWidth = BlockMediaRenderConstants.CartridgeOutlineWidth
               })
            canvas.DrawRoundRect(body, BlockMediaRenderConstants.CartridgeCornerRadius,
                BlockMediaRenderConstants.CartridgeCornerRadius, outline);

        var panel = CartridgePanel(body);
        using (var panelPaint = new SKPaint
               {
                   Color = BlockMediaRenderConstants.CartridgePanelColor,
                   IsAntialias = true,
                   Style = SKPaintStyle.Fill
               })
            canvas.DrawRoundRect(panel, BlockMediaRenderConstants.CartridgeCellRadius,
                BlockMediaRenderConstants.CartridgeCellRadius, panelPaint);

        DrawCartridgeConnector(canvas, body);

        var ranges = Aggregate(model.Ranges);
        for (var index = 0; index < ranges.Count; index++)
        {
            var range = ranges[index];
            var cell = CartridgeCell(body, ranges.Count, index);
            using var fill = new SKPaint { Color = ColorFor(range.State), IsAntialias = true, Style = SKPaintStyle.Fill };
            canvas.DrawRoundRect(cell, BlockMediaRenderConstants.CartridgeCellRadius,
                BlockMediaRenderConstants.CartridgeCellRadius, fill);
            if (!string.IsNullOrWhiteSpace(range.Label))
            {
                using var text = new SKPaint
                {
                    Color = BlockMediaRenderConstants.CartridgeTextColor,
                    IsAntialias = true
                };
                using var font = new SKFont { Size = Math.Clamp(cell.Height * .28f, 9f, 18f) };
                canvas.DrawText(
                    range.Label,
                    cell.MidX,
                    cell.MidY - (font.Metrics.Ascent + font.Metrics.Descent) / 2,
                    SKTextAlign.Center,
                    font,
                    text);
            }
            if (!ReferenceEquals(range, selectedRange)) continue;
            using var selection = new SKPaint
            {
                Color = BlockMediaRenderConstants.SelectionColor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = BlockMediaRenderConstants.SelectionStrokeWidth
            };
            canvas.DrawRoundRect(cell, BlockMediaRenderConstants.CartridgeCellRadius,
                BlockMediaRenderConstants.CartridgeCellRadius, selection);
        }
    }

    private static BlockMediaRange? HitTestCartridge(
        BlockMediaRenderModel model,
        int width,
        int height,
        SKPoint point,
        float zoom)
    {
        var ranges = Aggregate(model.Ranges);
        var body = CartridgeBounds(width, height, zoom);
        for (var index = 0; index < ranges.Count; index++)
            if (CartridgeCell(body, ranges.Count, index).Contains(point.X, point.Y)) return ranges[index];
        return null;
    }

    private static SKRect CartridgeBounds(int width, int height, float zoom)
    {
        var availableWidth = Math.Max(1, width * BlockMediaRenderConstants.CartridgeMaximumWidthRatio);
        var availableHeight = Math.Max(1, height * BlockMediaRenderConstants.CartridgeMaximumHeightRatio);
        var bodyWidth = Math.Min(availableWidth, availableHeight * BlockMediaRenderConstants.CartridgeAspectRatio) * zoom;
        var bodyHeight = bodyWidth / BlockMediaRenderConstants.CartridgeAspectRatio;
        var left = (width - bodyWidth) / 2f;
        var top = (height - bodyHeight) / 2f;
        return new SKRect(left, top, left + bodyWidth, top + bodyHeight);
    }

    private static SKRect CartridgePanel(SKRect body)
    {
        return new SKRect(
            body.Left + body.Width * .07f,
            body.Top + body.Height * .08f,
            body.Right - body.Width * .07f,
            body.Bottom - body.Height * .20f);
    }

    private static void DrawCartridgeConnector(SKCanvas canvas, SKRect body)
    {
        var connector = new SKRect(
            body.Left + body.Width * .18f,
            body.Bottom - body.Height * .12f,
            body.Right - body.Width * .18f,
            body.Bottom - body.Height * .035f);
        using var slotPaint = new SKPaint
        {
            Color = BlockMediaRenderConstants.CartridgePanelColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRoundRect(connector, 4f, 4f, slotPaint);
        using var contactPaint = new SKPaint
        {
            Color = BlockMediaRenderConstants.CartridgeConnectorColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        const int contactCount = 20;
        var contactWidth = connector.Width / contactCount;
        for (var contact = 0; contact < contactCount; contact++)
        {
            var left = connector.Left + contact * contactWidth + contactWidth * .20f;
            var right = connector.Left + (contact + 1) * contactWidth - contactWidth * .20f;
            canvas.DrawRect(new SKRect(left, connector.Top + 3f, right, connector.Bottom - 3f), contactPaint);
        }
    }

    private static SKRect CartridgeCell(SKRect body, int count, int index)
    {
        var columns = Math.Min(BlockMediaRenderConstants.CartridgeMaximumColumns, Math.Max(1, count));
        var rows = Math.Max(1, (count + columns - 1) / columns);
        var content = CartridgePanel(body);
        content.Inflate(-body.Width * .025f, -body.Height * .035f);
        var cellWidth = content.Width / columns;
        var cellHeight = content.Height / rows;
        var column = index % columns;
        var row = index / columns;
        var gap = Math.Min(BlockMediaRenderConstants.CartridgeCellGap, Math.Min(cellWidth, cellHeight) * .18f);
        return new SKRect(
            content.Left + column * cellWidth + gap,
            content.Top + row * cellHeight + gap,
            content.Left + (column + 1) * cellWidth - gap,
            content.Top + (row + 1) * cellHeight - gap);
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

    internal static SKColor ColorFor(BlockMediaRangeState state) => state switch
    {
        BlockMediaRangeState.Available => BlockMediaRenderConstants.AvailableColor,
        BlockMediaRangeState.Reserved => BlockMediaRenderConstants.ReservedColor,
        BlockMediaRangeState.Allocated => BlockMediaRenderConstants.AllocatedColor,
        BlockMediaRangeState.Free => BlockMediaRenderConstants.FreeColor,
        _ => BlockMediaRenderConstants.UnknownColor
    };
}
