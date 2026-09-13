using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.App.Functions.Rendering.Scp;
using GWGUI.App.Interfaces.Rendering.Scp;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Decoding;
using SkiaSharp;
using System.Collections.Concurrent;

using GWGUI.MediaEngine.Formats.Floppy.Scp;

namespace GWGUI.App.Rendering.Scp;

public sealed partial class SkiaScpRenderer : IScpRenderer
{
    private readonly FluxDecoderRegistry _decoders;
    private IReadOnlyDictionary<ScpTrack, PreparedScpTrack> _preparedTracks = new ConcurrentDictionary<ScpTrack, PreparedScpTrack>();
    private IReadOnlySet<int> _revealedCylinders = new HashSet<int>();
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

    public async Task PrepareAsync(ScpImage image, int head, IProgress<ScpTrackPreparation>? progress = null, CancellationToken cancellationToken = default)
    {
        var tracks = image.Tracks.Where(track => track.Head == head).OrderBy(track => track.Cylinder).ToArray();
        var decoderId = DecoderId;
        var prepared = new ConcurrentDictionary<ScpTrack, PreparedScpTrack>();
        _preparedTracks = prepared;
        _revealedCylinders = new HashSet<int>();
        await Task.Run(() =>
        {
            Parallel.ForEach(tracks, new ParallelOptions { CancellationToken = cancellationToken }, track =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var preparedTrack = track.Revolutions.Any(revolution => revolution.FluxIntervals.Count > 0)
                    ? PrepareTrack(track, decoderId, cancellationToken)
                    : new PreparedScpTrack([], new PreparedScpRevolution([], 0), [], ScpTrackVisualState.Anomaly, 0, 0, 0, false);
                prepared[track] = preparedTrack;
                progress?.Report(new ScpTrackPreparation(
                    track.Cylinder,
                    track.Head,
                    preparedTrack.VisualState,
                    preparedTrack.ValidSectors,
                    preparedTrack.InvalidSectors,
                    preparedTrack.UnverifiedSectors,
                    preparedTrack.HasFlux,
                    preparedTrack.Synthesis.Quality));
            });
        }, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    public void ClearCache()
    {
        _preparedTracks = new ConcurrentDictionary<ScpTrack, PreparedScpTrack>();
        _revealedCylinders = new HashSet<int>();
    }

    public void RevealTrack(int cylinder)
    {
        var revealed = _revealedCylinders.ToHashSet();
        revealed.Add(cylinder);
        _revealedCylinders = revealed;
    }

    internal int RevealedTrackCount => _revealedCylinders.Count;

    public void Render(SKCanvas canvas, ScpRenderRequest request)
    {
        DrawRecessedBackground(canvas, request.Width, request.Height);
        var tracks = request.Image?.Tracks.Where(track => track.Head == request.Head).OrderBy(track => track.Cylinder).ToArray() ?? [];
        var outer = ScpMediaGeometryFunctions.FluxRadius(request.Width, request.Height, request.Zoom, request.MediaCategory);
        var inner = outer * .25f;
        using var disk = new SKPaint { Color = new SKColor(17, 24, 29), IsAntialias = true };
        canvas.DrawCircle(request.Center, outer, disk);
        using var hub = new SKPaint { Color = new SKColor(4, 6, 8), IsAntialias = true };
        canvas.DrawCircle(request.Center, inner, hub);
        if (tracks.Length == 0)
        {
            DrawCentered(canvas, request.Center, request.EmptySideText, SKColors.White);
            return;
        }

        var ring = (outer - inner) / Math.Max(1, tracks.Length);
        using var shortFlux = FluxPaint(new SKColor(68, 151, 143));
        using var longFlux = FluxPaint(new SKColor(72, 115, 154));
        using var normalFlux = FluxPaint(new SKColor(55, 137, 101));
        using var headerStructure = StructurePaint(new SKColor(218, 174, 82));
        using var dataStructure = StructurePaint(new SKColor(83, 161, 181));
        using var errorStructure = StructurePaint(new SKColor(201, 82, 91));
        using var otherStructure = StructurePaint(new SKColor(138, 151, 160));
        var fluxWidth = Math.Max(1, ring * .24f);
        shortFlux.StrokeWidth = longFlux.StrokeWidth = normalFlux.StrokeWidth = fluxWidth;
        var structureWidth = Math.Max(1, ring * .24f);
        headerStructure.StrokeWidth = dataStructure.StrokeWidth = errorStructure.StrokeWidth = otherStructure.StrokeWidth = structureWidth;
        using var emptyTrackPaint = new SKPaint
        {
            Color = new SKColor(74, 66, 57),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = Math.Max(1, ring * .72f)
        };
        for (var trackIndex = 0; trackIndex < tracks.Length; trackIndex++)
        {
            var track = tracks[trackIndex];
            var radius = outer - ring * (trackIndex + .5f);
            if (!_revealedCylinders.Contains(track.Cylinder) || !_preparedTracks.TryGetValue(track, out var prepared))
            {
                canvas.DrawCircle(request.Center, radius, emptyTrackPaint);
                continue;
            }
            var trackRect = new SKRect(request.Center.X - radius, request.Center.Y - radius, request.Center.X + radius, request.Center.Y + radius);
            var preparedRevolution = request.SelectedRevolutionIndex is int revolutionIndex
                ? prepared.Revolutions.ElementAtOrDefault(revolutionIndex) ?? prepared.Synthesis
                : prepared.Synthesis;
            using var qualityPaint = new SKPaint
            {
                Color = QualityColor(preparedRevolution.Quality),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = Math.Max(1, ring * .72f)
            };
            canvas.DrawCircle(request.Center, radius, qualityPaint);
            using var shortPath = new SKPath();
            using var longPath = new SKPath();
            using var normalPath = new SKPath();
            foreach (var arc in preparedRevolution.FluxArcs)
            {
                var path = arc.Color == shortFlux.Color ? shortPath : arc.Color == longFlux.Color ? longPath : normalPath;
                path.AddArc(trackRect, arc.Start, arc.Sweep);
            }
            canvas.DrawPath(shortPath, shortFlux);
            canvas.DrawPath(longPath, longFlux);
            canvas.DrawPath(normalPath, normalFlux);
            if (ReferenceEquals(track, request.SelectedTrack))
            {
                DrawDecodedStructures(canvas, trackRect, prepared.StructureArcs, headerStructure, dataStructure, errorStructure, otherStructure);
                using var selected = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };
                canvas.DrawCircle(request.Center, radius, selected);
            }
        }
        DrawCentered(canvas, request.Center, request.SideText, new SKColor(210, 218, 228));
    }

    internal static SKColor QualityColor(double quality) => quality switch
    {
        <= .10 => new SKColor(190, 55, 62),
        <= .40 => new SKColor(211, 113, 48),
        <= .65 => new SKColor(132, 118, 57),
        <= .85 => new SKColor(63, 116, 72),
        _ => new SKColor(47, 166, 91)
    };

}
