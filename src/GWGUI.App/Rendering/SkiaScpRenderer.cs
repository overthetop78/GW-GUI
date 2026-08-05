using GWGUI.Scp;
using GWGUI.Scp.Decoding;
using SkiaSharp;

namespace GWGUI.App.Rendering;

public sealed class SkiaScpRenderer : IScpRenderer
{
    private readonly FluxDecoderRegistry _decoders;
    private readonly Dictionary<ScpTrack, (ScpRevolution Revolution, FluxDecodeResult Result)> _decodeCache = [];
    private string? _decoderId;

    public SkiaScpRenderer(FluxDecoderRegistry? decoders = null) => _decoders = decoders ?? new FluxDecoderRegistry();

    public string? DecoderId
    {
        get => _decoderId;
        set
        {
            if (_decoderId == value) return;
            _decoderId = value;
            ClearCache();
        }
    }

    public void ClearCache() => _decodeCache.Clear();

    public void Render(SKCanvas canvas, ScpRenderRequest request)
    {
        canvas.Clear(new SKColor(7, 10, 14));
        var tracks = request.Image?.Tracks.Where(track => track.Head == request.Head).OrderBy(track => track.Cylinder).ToArray() ?? [];
        var outer = Math.Min(request.Width, request.Height) * .47f * request.Zoom;
        var inner = outer * .25f;
        using var disk = new SKPaint { Color = new SKColor(17, 61, 43), IsAntialias = true };
        canvas.DrawCircle(request.Center, outer, disk);
        using var hub = new SKPaint { Color = new SKColor(4, 6, 8), IsAntialias = true };
        canvas.DrawCircle(request.Center, inner, hub);
        if (tracks.Length == 0)
        {
            DrawCentered(canvas, request.Center, request.EmptySideText, SKColors.White);
            return;
        }

        var ring = (outer - inner) / Math.Max(1, tracks.Length);
        for (var trackIndex = 0; trackIndex < tracks.Length; trackIndex++)
        {
            var track = tracks[trackIndex];
            var radius = outer - ring * (trackIndex + .5f);
            var revolution = track.Revolutions.FirstOrDefault();
            if (revolution is null || revolution.FluxIntervals.Count == 0) continue;
            var sampleStep = Math.Max(1, revolution.FluxIntervals.Count / 1400);
            var total = revolution.FluxIntervals.Sum(interval => (double)interval);
            double elapsed = 0;
            var median = revolution.FluxIntervals.Order().ElementAt(revolution.FluxIntervals.Count / 2);
            for (var index = 0; index < revolution.FluxIntervals.Count; index += sampleStep)
            {
                double span = 0;
                for (var sample = index; sample < Math.Min(index + sampleStep, revolution.FluxIntervals.Count); sample++) span += revolution.FluxIntervals[sample];
                var start = (float)(elapsed / total * 360 - 90);
                var sweep = Math.Max(.08f, (float)(span / total * 360));
                elapsed += span;
                var interval = revolution.FluxIntervals[index];
                var color = interval < median * .65 ? new SKColor(143, 104, 255) : interval > median * 1.8 ? new SKColor(83, 173, 255) : new SKColor(36, 179, 93);
                using var paint = new SKPaint { Color = color, IsAntialias = false, Style = SKPaintStyle.Stroke, StrokeWidth = Math.Max(1, ring * .82f) };
                canvas.DrawArc(new SKRect(request.Center.X - radius, request.Center.Y - radius, request.Center.X + radius, request.Center.Y + radius), start, sweep, false, paint);
            }
            DrawDecodedStructures(canvas, request.Center, radius, ring, track);
            if (ReferenceEquals(track, request.SelectedTrack))
            {
                using var selected = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };
                canvas.DrawCircle(request.Center, radius, selected);
            }
        }
        DrawCentered(canvas, request.Center, request.SideText, new SKColor(210, 218, 228));
    }

    private void DrawDecodedStructures(SKCanvas canvas, SKPoint center, float radius, float ring, ScpTrack track)
    {
        if (!_decodeCache.TryGetValue(track, out var analysis))
        {
            var best = _decoders.DecodeBest(track.Revolutions, DecoderId);
            if (best is null) return;
            analysis = (track.Revolutions[best.Value.RevolutionIndex], best.Value.Result);
            _decodeCache[track] = analysis;
        }
        var (revolution, decoded) = analysis;
        if (decoded.Structures.Count == 0 || decoded.EstimatedBitCellTicks <= 0) return;
        var totalBits = Math.Max(1d, revolution.FluxIntervals.Sum(interval => (double)interval) / decoded.EstimatedBitCellTicks);
        foreach (var structure in decoded.Structures)
        {
            var start = (float)(structure.BitOffset / totalBits * 360 - 90);
            var sweep = Math.Max(.18f, (float)(structure.BitLength / totalBits * 360));
            using var paint = new SKPaint { Color = StructureColor(structure.Kind), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = Math.Max(2, ring * .45f), StrokeCap = SKStrokeCap.Round };
            canvas.DrawArc(new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius), start, sweep, false, paint);
        }
    }

    private static SKColor StructureColor(FluxStructureKind kind) => kind switch
    {
        FluxStructureKind.IdAddressMark or FluxStructureKind.AppleAddress or FluxStructureKind.CommodoreHeader or FluxStructureKind.FormatHeader => new SKColor(255, 205, 64),
        FluxStructureKind.DataAddressMark or FluxStructureKind.AppleData or FluxStructureKind.FormatData => new SKColor(67, 220, 255),
        FluxStructureKind.DeletedDataAddressMark or FluxStructureKind.TimingAnomaly => new SKColor(255, 75, 96),
        _ => new SKColor(196, 117, 255)
    };

    private static void DrawCentered(SKCanvas canvas, SKPoint center, string text, SKColor color)
    {
        using var paint = new SKPaint { Color = color, IsAntialias = true };
        using var font = new SKFont(SKTypeface.Default, 17);
        var lines = text.Split('\n');
        for (var index = 0; index < lines.Length; index++) canvas.DrawText(lines[index], center.X, center.Y + index * 20, SKTextAlign.Center, font, paint);
    }
}
