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
        var scaledMargin = SequentialMediaRenderConstants.OuterMargin * zoom;
        var scaledGap = SequentialMediaRenderConstants.LaneGap * zoom;
        var contentWidth = Math.Max(1, width - scaledMargin * 2);
        var rowHeight = Math.Max(2, (height - scaledMargin * 2) / (float)(lineCount * lanes.Length));
        var bandHeight = Math.Clamp(rowHeight - scaledGap, 20 * zoom, 42 * zoom);
        var lineDuration = duration.Ticks / (double)lineCount;

        using (var tape = new SKPaint { Color = SequentialMediaRenderConstants.TapeColor, IsAntialias = true })
        {
            for (var line = 0; line < lineCount; line++)
            {
                for (var laneIndex = 0; laneIndex < lanes.Length; laneIndex++)
                {
                    var centerY = scaledMargin + (line * lanes.Length + laneIndex + .5f) * rowHeight;
                    canvas.DrawRoundRect(
                        new SKRect(
                            scaledMargin,
                            centerY - bandHeight / 2,
                            scaledMargin + contentWidth,
                            centerY + bandHeight / 2),
                        bandHeight / 2,
                        bandHeight / 2,
                        tape);
                }
            }
        }

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
                var x = scaledMargin + (float)((remainingStart - line * lineDuration) / lineDuration * contentWidth);
                var centerY = scaledMargin + (line * lanes.Length + laneIndex + .5f) * rowHeight;
                var segmentWidth = Math.Max(.25f, (float)(partLength / lineDuration * contentWidth));
                var rect = new SKRect(x, centerY - bandHeight / 2, x + segmentWidth, centerY + bandHeight / 2);
                using var fill = new SKPaint { Color = ColorFor(segment).WithAlpha(210), IsAntialias = false };
                canvas.DrawRect(rect, fill);
                if (ReferenceEquals(segment, selectedSegment))
                {
                    using var selection = new SKPaint
                    {
                        Color = SequentialMediaRenderConstants.SelectionColor,
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = SequentialMediaRenderConstants.SelectionStrokeWidth * zoom
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
        var scaledMargin = SequentialMediaRenderConstants.OuterMargin * zoom;
        var scaledGap = SequentialMediaRenderConstants.LaneGap * zoom;
        var contentWidth = Math.Max(1, width - scaledMargin * 2);
        var rowHeight = Math.Max(2, (height - scaledMargin * 2) / (float)(lineCount * lanes.Length));
        var bandHeight = Math.Clamp(rowHeight - scaledGap, 20 * zoom, 42 * zoom);
        var lineDuration = duration.Ticks / (double)lineCount;

        foreach (var segment in model.Segments.Reverse())
        {
            var laneIndex = Array.IndexOf(lanes, segment.Lane);
            var remainingStart = (double)segment.Start.Ticks;
            var remainingLength = (double)segment.Duration.Ticks;
            while (remainingLength > 0)
            {
                var line = Math.Clamp((int)(remainingStart / lineDuration), 0, lineCount - 1);
                var lineEnd = (line + 1) * lineDuration;
                var partLength = Math.Min(remainingLength, lineEnd - remainingStart);
                var x = scaledMargin + (float)((remainingStart - line * lineDuration) / lineDuration * contentWidth);
                var centerY = scaledMargin + (line * lanes.Length + laneIndex + .5f) * rowHeight;
                var segmentWidth = Math.Max(.25f, (float)(partLength / lineDuration * contentWidth));
                if (new SKRect(x, centerY - bandHeight / 2, x + segmentWidth, centerY + bandHeight / 2).Contains(point))
                    return segment;
                remainingStart += partLength;
                remainingLength -= partLength;
            }
        }

        return null;
    }

    private static TimeSpan TotalDuration(SequentialMediaRenderModel model) => model.Duration
        ?? model.Segments.Select(segment => segment.Start + segment.Duration).DefaultIfEmpty().Max();

    internal static SKColor ColorFor(SequentialMediaSegment segment)
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
