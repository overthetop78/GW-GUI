using GWGUI.App.Constants.Rendering.Sequential;
using GWGUI.App.Contracts.Rendering.Sequential;
using GWGUI.App.Enums.Rendering.Sequential;
using SkiaSharp;

namespace GWGUI.App.Rendering.Sequential;

public sealed class SkiaSequentialMediaRenderer
{
    public void Render(
        SKCanvas canvas,
        SequentialMediaRenderModel? model,
        SequentialMediaSegment? selectedSegment,
        int preparedSegmentCount,
        int wrappedLineCount,
        int width,
        int height,
        float zoom = 1)
    {
        canvas.Clear(SequentialMediaRenderConstants.BackgroundColor);
        if (model is null) return;
        var duration = TotalDuration(model);
        if (duration <= TimeSpan.Zero) return;
        var lanes = model.Segments.Select(segment => segment.Lane).Distinct().Order().DefaultIfEmpty(0).ToArray();
        var lineCount = Math.Clamp(wrappedLineCount, 1, SequentialMediaRenderConstants.MaximumWrappedLineCount);
        var contentWidth = Math.Max(1, width - SequentialMediaRenderConstants.OuterMargin * 2) * zoom;
        var rowHeight = Math.Max(2, (height - SequentialMediaRenderConstants.OuterMargin * 2) / (float)(lineCount * lanes.Length));
        var lineDuration = duration.Ticks / (double)lineCount;

        foreach (var segment in model.Segments.Take(Math.Clamp(preparedSegmentCount, 0, model.Segments.Count)))
        {
            var laneIndex = Array.IndexOf(lanes, segment.Lane);
            var remainingStart = (double)segment.Start.Ticks;
            var remainingLength = (double)segment.Duration.Ticks;
            while (remainingLength > 0)
            {
                var line = Math.Clamp((int)(remainingStart / lineDuration), 0, lineCount - 1);
                var lineEnd = (line + 1) * lineDuration;
                var partLength = Math.Min(remainingLength, lineEnd - remainingStart);
                var x = SequentialMediaRenderConstants.OuterMargin + (float)((remainingStart - line * lineDuration) / lineDuration * contentWidth);
                var y = SequentialMediaRenderConstants.OuterMargin + (line * lanes.Length + laneIndex) * rowHeight;
                var segmentWidth = Math.Max(1, (float)(partLength / lineDuration * contentWidth));
                var rect = new SKRect(x, y + SequentialMediaRenderConstants.LaneGap / 2, x + segmentWidth, y + rowHeight - SequentialMediaRenderConstants.LaneGap / 2);
                using var fill = new SKPaint { Color = ColorFor(segment), IsAntialias = true };
                canvas.DrawRoundRect(rect, 2, 2, fill);
                if (ReferenceEquals(segment, selectedSegment))
                {
                    using var selection = new SKPaint
                    {
                        Color = SequentialMediaRenderConstants.SelectionColor,
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = SequentialMediaRenderConstants.SelectionStrokeWidth
                    };
                    canvas.DrawRoundRect(rect, 2, 2, selection);
                }
                remainingStart += partLength;
                remainingLength -= partLength;
            }
        }
    }

    public SequentialMediaSegment? HitTest(
        SequentialMediaRenderModel? model,
        int wrappedLineCount,
        int width,
        int height,
        SKPoint point,
        float zoom = 1)
    {
        if (model is null) return null;
        var duration = TotalDuration(model);
        if (duration <= TimeSpan.Zero) return null;
        var lanes = model.Segments.Select(segment => segment.Lane).Distinct().Order().DefaultIfEmpty(0).ToArray();
        var lineCount = Math.Clamp(wrappedLineCount, 1, SequentialMediaRenderConstants.MaximumWrappedLineCount);
        var contentWidth = Math.Max(1, width - SequentialMediaRenderConstants.OuterMargin * 2) * zoom;
        var rowHeight = Math.Max(2, (height - SequentialMediaRenderConstants.OuterMargin * 2) / (float)(lineCount * lanes.Length));
        var relativeY = point.Y - SequentialMediaRenderConstants.OuterMargin;
        var row = (int)(relativeY / rowHeight);
        if (row < 0 || row >= lineCount * lanes.Length) return null;
        var line = row / lanes.Length;
        var lane = lanes[row % lanes.Length];
        var relativeX = point.X - SequentialMediaRenderConstants.OuterMargin;
        if (relativeX < 0 || relativeX > contentWidth) return null;
        var ticks = (long)((line + relativeX / contentWidth) * duration.Ticks / lineCount);
        return model.Segments.FirstOrDefault(segment =>
            segment.Lane == lane && ticks >= segment.Start.Ticks && ticks < segment.Start.Ticks + segment.Duration.Ticks);
    }

    private static TimeSpan TotalDuration(SequentialMediaRenderModel model) => model.Duration
        ?? model.Segments.Select(segment => segment.Start + segment.Duration).DefaultIfEmpty().Max();

    private static SKColor ColorFor(SequentialMediaSegment segment)
    {
        if (!string.IsNullOrWhiteSpace(segment.DecodeError)) return SequentialMediaRenderConstants.ErrorColor;
        return segment.Kind switch
        {
            SequentialSegmentKind.Signal => SequentialMediaRenderConstants.SignalColor,
            SequentialSegmentKind.Silence => SequentialMediaRenderConstants.SilenceColor,
            SequentialSegmentKind.DecodedBlock => SequentialMediaRenderConstants.DecodedBlockColor,
            _ => SequentialMediaRenderConstants.UnknownColor
        };
    }
}
