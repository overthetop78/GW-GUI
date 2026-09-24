using GWGUI.App.Constants.Rendering.Optical;
using GWGUI.App.Contracts.Rendering.Optical;
using GWGUI.App.Enums.Rendering.Optical;
using SkiaSharp;

namespace GWGUI.App.Rendering.Optical;

public sealed class SkiaOpticalMediaRenderer
{
    public void Render(
        SKCanvas canvas,
        OpticalMediaRenderModel? model,
        OpticalMediaTrack? selectedTrack,
        int? face,
        int? layer,
        int? session,
        int width,
        int height,
        float zoom = 1)
    {
        canvas.Clear(OpticalMediaRenderConstants.BackgroundColor);
        if (model is null) return;
        var faces = RenderedFaces(model, face);
        if (faces.Count == 0) return;
        var cellWidth = width / (float)faces.Count;
        for (var index = 0; index < faces.Count; index++)
        {
            var center = new SKPoint(index * cellWidth + cellWidth / 2, height / 2f);
            var outer = Math.Max(1, Math.Min(cellWidth, height) / 2f - OpticalMediaRenderConstants.OuterMargin) * zoom;
            DrawFace(canvas, model, faces[index], layer, session, selectedTrack, center, outer);
        }
    }

    public OpticalMediaTrack? HitTest(
        OpticalMediaRenderModel? model,
        int? face,
        int? layer,
        int? session,
        int width,
        int height,
        SKPoint point,
        float zoom = 1)
    {
        if (model is null) return null;
        var faces = RenderedFaces(model, face);
        if (faces.Count == 0) return null;
        var cellWidth = width / (float)faces.Count;
        var faceIndex = Math.Clamp((int)(point.X / cellWidth), 0, faces.Count - 1);
        var center = new SKPoint(faceIndex * cellWidth + cellWidth / 2, height / 2f);
        var outer = Math.Max(1, Math.Min(cellWidth, height) / 2f - OpticalMediaRenderConstants.OuterMargin) * zoom;
        var inner = outer * OpticalMediaRenderConstants.InnerRadiusRatio;
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;
        var radius = MathF.Sqrt(dx * dx + dy * dy);
        if (radius < inner || radius > outer) return null;
        var tracks = FilterTracks(model, faces[faceIndex], layer, session);
        var logicalLength = LogicalLength(model, tracks);
        if (logicalLength <= 0) return null;
        var angle = (MathF.Atan2(dy, dx) * 180f / MathF.PI + 450f) % 360f;
        var sector = (long)(angle / 360f * logicalLength);
        return tracks.FirstOrDefault(track => sector >= track.FirstSector && sector < track.FirstSector + track.SectorCount);
    }

    private static void DrawFace(
        SKCanvas canvas,
        OpticalMediaRenderModel model,
        int face,
        int? layer,
        int? session,
        OpticalMediaTrack? selectedTrack,
        SKPoint center,
        float outer)
    {
        var tracks = FilterTracks(model, face, layer, session);
        var logicalLength = LogicalLength(model, tracks);
        if (logicalLength <= 0) return;
        var inner = outer * OpticalMediaRenderConstants.InnerRadiusRatio;
        var radius = (outer + inner) / 2;
        var rect = new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
        foreach (var track in tracks)
        {
            var start = -90f + 360f * track.FirstSector / logicalLength;
            var sweep = Math.Max(.08f, 360f * track.SectorCount / logicalLength - OpticalMediaRenderConstants.TrackGapDegrees);
            using var paint = new SKPaint
            {
                Color = ColorFor(track.Kind),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = outer - inner,
                StrokeCap = SKStrokeCap.Butt
            };
            canvas.DrawArc(rect, start, sweep, false, paint);
            if (!ReferenceEquals(track, selectedTrack)) continue;
            using var selection = new SKPaint
            {
                Color = OpticalMediaRenderConstants.SelectionColor,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = OpticalMediaRenderConstants.SelectionStrokeWidth
            };
            var selectionRect = new SKRect(center.X - outer, center.Y - outer, center.X + outer, center.Y + outer);
            canvas.DrawArc(selectionRect, start, sweep, false, selection);
        }
        using var hub = new SKPaint { Color = OpticalMediaRenderConstants.BackgroundColor, IsAntialias = true };
        canvas.DrawCircle(center, inner, hub);
    }

    private static IReadOnlyList<int> RenderedFaces(OpticalMediaRenderModel model, int? face)
    {
        if (face is { } selectedFace) return [selectedFace];
        var knownFaces = model.Tracks.Where(track => track.FaceNumber.HasValue)
            .Select(track => track.FaceNumber!.Value).Distinct().Order().ToArray();
        return knownFaces.Length > 0 ? knownFaces : [-1];
    }

    private static OpticalMediaTrack[] FilterTracks(OpticalMediaRenderModel model, int face, int? layer, int? session) =>
        model.Tracks.Where(track =>
            (face < 0 ? track.FaceNumber is null : track.FaceNumber == face) &&
            (layer is null || track.LayerNumber == layer) &&
            (session is null || track.SessionNumber == session)).OrderBy(track => track.FirstSector).ToArray();

    private static long LogicalLength(OpticalMediaRenderModel model, IReadOnlyList<OpticalMediaTrack> tracks) =>
        model.LogicalLength ?? tracks.Select(track => track.FirstSector + track.SectorCount).DefaultIfEmpty().Max();

    private static SKColor ColorFor(OpticalTrackKind kind) => kind switch
    {
        OpticalTrackKind.Data => OpticalMediaRenderConstants.DataTrackColor,
        OpticalTrackKind.Audio => OpticalMediaRenderConstants.AudioTrackColor,
        _ => OpticalMediaRenderConstants.UnknownTrackColor
    };
}
