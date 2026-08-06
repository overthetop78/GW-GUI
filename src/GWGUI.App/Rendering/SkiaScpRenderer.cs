using GWGUI.Scp;
using GWGUI.Scp.Decoding;
using SkiaSharp;
using System.Collections.Concurrent;

namespace GWGUI.App.Rendering;

public sealed class SkiaScpRenderer : IScpRenderer
{
    private readonly FluxDecoderRegistry _decoders;
    private IReadOnlyDictionary<ScpTrack, PreparedTrack> _preparedTracks = new ConcurrentDictionary<ScpTrack, PreparedTrack>();
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

    public async Task PrepareAsync(ScpImage image, int head, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        var tracks = image.Tracks.Where(track => track.Head == head).OrderBy(track => track.Cylinder).ToArray();
        var decoderId = DecoderId;
        var prepared = new ConcurrentDictionary<ScpTrack, PreparedTrack>();
        _preparedTracks = prepared;
        await Task.Run(() =>
        {
            for (var trackIndex = 0; trackIndex < tracks.Length; trackIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var track = tracks[trackIndex];
                var revolution = track.Revolutions.FirstOrDefault();
                if (revolution is not null && revolution.FluxIntervals.Count > 0)
                    prepared[track] = PrepareTrack(track, revolution, decoderId, cancellationToken);
                progress?.Report(trackIndex + 1);
            }
        }, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    public void ClearCache() => _preparedTracks = new ConcurrentDictionary<ScpTrack, PreparedTrack>();

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
        using var shortFlux = FluxPaint(new SKColor(143, 104, 255));
        using var longFlux = FluxPaint(new SKColor(83, 173, 255));
        using var normalFlux = FluxPaint(new SKColor(36, 179, 93));
        using var headerStructure = StructurePaint(new SKColor(255, 205, 64));
        using var dataStructure = StructurePaint(new SKColor(67, 220, 255));
        using var errorStructure = StructurePaint(new SKColor(255, 75, 96));
        using var otherStructure = StructurePaint(new SKColor(196, 117, 255));
        var fluxWidth = Math.Max(1, ring * .82f);
        shortFlux.StrokeWidth = longFlux.StrokeWidth = normalFlux.StrokeWidth = fluxWidth;
        var structureWidth = Math.Max(2, ring * .45f);
        headerStructure.StrokeWidth = dataStructure.StrokeWidth = errorStructure.StrokeWidth = otherStructure.StrokeWidth = structureWidth;
        for (var trackIndex = 0; trackIndex < tracks.Length; trackIndex++)
        {
            var track = tracks[trackIndex];
            var radius = outer - ring * (trackIndex + .5f);
            if (!_preparedTracks.TryGetValue(track, out var prepared)) continue;
            var trackRect = new SKRect(request.Center.X - radius, request.Center.Y - radius, request.Center.X + radius, request.Center.Y + radius);
            using var shortPath = new SKPath();
            using var longPath = new SKPath();
            using var normalPath = new SKPath();
            foreach (var arc in prepared.FluxArcs)
            {
                var path = arc.Color == shortFlux.Color ? shortPath : arc.Color == longFlux.Color ? longPath : normalPath;
                path.AddArc(trackRect, arc.Start, arc.Sweep);
            }
            canvas.DrawPath(shortPath, shortFlux);
            canvas.DrawPath(longPath, longFlux);
            canvas.DrawPath(normalPath, normalFlux);
            DrawDecodedStructures(canvas, trackRect, prepared.StructureArcs, headerStructure, dataStructure, errorStructure, otherStructure);
            if (ReferenceEquals(track, request.SelectedTrack))
            {
                using var selected = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };
                canvas.DrawCircle(request.Center, radius, selected);
            }
        }
        DrawCentered(canvas, request.Center, request.SideText, new SKColor(210, 218, 228));
    }

    private PreparedTrack PrepareTrack(ScpTrack track, ScpRevolution revolution, string? decoderId, CancellationToken cancellationToken)
    {
        var intervals = revolution.FluxIntervals;
        var sampleStep = Math.Max(1, intervals.Count / 720);
        var total = intervals.Sum(interval => (double)interval);
        var ordered = intervals.ToArray();
        Array.Sort(ordered);
        var median = ordered[ordered.Length / 2];
        var fluxArcs = new List<PreparedArc>(Math.Min(720, intervals.Count));
        double elapsed = 0;
        for (var index = 0; index < intervals.Count; index += sampleStep)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double span = 0;
            for (var sample = index; sample < Math.Min(index + sampleStep, intervals.Count); sample++) span += intervals[sample];
            var color = intervals[index] < median * .65 ? new SKColor(143, 104, 255) : intervals[index] > median * 1.8 ? new SKColor(83, 173, 255) : new SKColor(36, 179, 93);
            fluxArcs.Add(new((float)(elapsed / total * 360 - 90), Math.Max(.08f, (float)(span / total * 360)), color));
            elapsed += span;
        }

        var structureArcs = new List<PreparedArc>();
        var best = _decoders.DecodeBest(track.Revolutions, decoderId);
        if (best is not null)
        {
            var decodedRevolution = track.Revolutions[best.Value.RevolutionIndex];
            var decoded = best.Value.Result;
            if (decoded.EstimatedBitCellTicks > 0)
            {
                var totalBits = Math.Max(1d, decodedRevolution.FluxIntervals.Sum(interval => (double)interval) / decoded.EstimatedBitCellTicks);
                structureArcs.AddRange(decoded.Structures.Select(structure => new PreparedArc(
                    (float)(structure.BitOffset / totalBits * 360 - 90),
                    Math.Max(.18f, (float)(structure.BitLength / totalBits * 360)),
                    StructureColor(structure.Kind))));
            }
        }
        return new(fluxArcs, structureArcs);
    }

    private static void DrawDecodedStructures(SKCanvas canvas, SKRect trackRect, IReadOnlyList<PreparedArc> arcs, SKPaint header, SKPaint data, SKPaint error, SKPaint other)
    {
        using var headerPath = new SKPath();
        using var dataPath = new SKPath();
        using var errorPath = new SKPath();
        using var otherPath = new SKPath();
        foreach (var arc in arcs)
        {
            var path = arc.Color == header.Color ? headerPath : arc.Color == data.Color ? dataPath : arc.Color == error.Color ? errorPath : otherPath;
            path.AddArc(trackRect, arc.Start, arc.Sweep);
        }
        canvas.DrawPath(headerPath, header);
        canvas.DrawPath(dataPath, data);
        canvas.DrawPath(errorPath, error);
        canvas.DrawPath(otherPath, other);
    }

    private static SKPaint FluxPaint(SKColor color) => new() { Color = color, IsAntialias = false, Style = SKPaintStyle.Stroke };
    private static SKPaint StructurePaint(SKColor color) => new() { Color = color, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round };

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

    private sealed record PreparedTrack(IReadOnlyList<PreparedArc> FluxArcs, IReadOnlyList<PreparedArc> StructureArcs);
    private sealed record PreparedArc(float Start, float Sweep, SKColor Color);
}
