using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.App.Functions.Rendering.Scp;
using GWGUI.App.Interfaces.Rendering.Scp;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using SkiaSharp;
using System.Collections.Concurrent;

namespace GWGUI.App.Rendering.Scp;

public sealed partial class SkiaScpRenderer : IScpRenderer
{
    private readonly FluxDecoderRegistry _decoders;
    private IReadOnlyDictionary<ScpTrack, PreparedScpTrack> _preparedTracks = new ConcurrentDictionary<ScpTrack, PreparedScpTrack>();
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
        await Task.Run(() =>
        {
            for (var trackIndex = 0; trackIndex < tracks.Length; trackIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var track = tracks[trackIndex];
                var revolution = track.Revolutions.FirstOrDefault();
                var preparedTrack = revolution is not null && revolution.FluxIntervals.Count > 0
                    ? PrepareTrack(track, revolution, decoderId, cancellationToken)
                    : new PreparedScpTrack([], [], ScpTrackVisualState.Anomaly, 0, 0, 0, false);
                prepared[track] = preparedTrack;
                progress?.Report(new ScpTrackPreparation(
                    track.Cylinder,
                    track.Head,
                    preparedTrack.VisualState,
                    preparedTrack.ValidSectors,
                    preparedTrack.InvalidSectors,
                    preparedTrack.UnverifiedSectors,
                    preparedTrack.HasFlux));
            }
        }, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }

    public void ClearCache() => _preparedTracks = new ConcurrentDictionary<ScpTrack, PreparedScpTrack>();

    public void Render(SKCanvas canvas, ScpRenderRequest request)
    {
        DrawRecessedBackground(canvas, request.Width, request.Height);
        var tracks = request.Image?.Tracks.Where(track => track.Head == request.Head).OrderBy(track => track.Cylinder).ToArray() ?? [];
        var outer = ScpMediaGeometryFunctions.FluxRadius(request.Width, request.Height, request.Zoom, request.MediaCategory);
        var inner = outer * .25f;
        DrawMedia(canvas, request, outer);
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

}
