using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
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

    public async Task PrepareAsync(ScpImage image, int head, IProgress<ScpTrackPreparation>? progress = null, CancellationToken cancellationToken = default)
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
                var preparedTrack = revolution is not null && revolution.FluxIntervals.Count > 0
                    ? PrepareTrack(track, revolution, decoderId, cancellationToken)
                    : new PreparedTrack([], [], ScpTrackVisualState.Anomaly, 0, 0, 0, false);
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

    public void ClearCache() => _preparedTracks = new ConcurrentDictionary<ScpTrack, PreparedTrack>();

    public void Render(SKCanvas canvas, ScpRenderRequest request)
    {
        DrawRecessedBackground(canvas, request.Width, request.Height);
        var tracks = request.Image?.Tracks.Where(track => track.Head == request.Head).OrderBy(track => track.Cylinder).ToArray() ?? [];
        var outer = ScpMediaGeometry.FluxRadius(request.Width, request.Height, request.Zoom, request.MediaKind);
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

    private static void DrawMedia(SKCanvas canvas, ScpRenderRequest request, float outer)
    {
        if (request.MediaKind == DiskMediaKind.Unknown) return;
        switch (request.MediaKind)
        {
            case DiskMediaKind.ThreeHalfDd:
                DrawThreeHalf(canvas, request, outer, new SKColor(48, 91, 145), false);
                break;
            case DiskMediaKind.ThreeHalfHd:
                DrawThreeHalf(canvas, request, outer, new SKColor(198, 191, 169), true);
                break;
            case DiskMediaKind.ThreeInch:
                DrawThreeInch(canvas, request, outer);
                break;
            case DiskMediaKind.FiveQuarterDd:
            case DiskMediaKind.FiveQuarterHd:
                DrawFlexibleDisk(canvas, request, outer, false);
                break;
            case DiskMediaKind.EightInch:
                DrawFlexibleDisk(canvas, request, outer, true);
                break;
        }
    }

    private static void DrawRecessedBackground(SKCanvas canvas, int width, int height)
    {
        var background = new SKColor(222, 216, 202);
        canvas.Clear(background);
        var inset = Math.Max(4, Math.Min(width, height) * .012f);
        var panel = new SKRect(inset, inset, width - inset, height - inset);
        using var panelFill = Fill(new SKColor(205, 198, 183));
        using var darkEdge = Stroke(new SKColor(151, 145, 134), Math.Max(2, inset * .45f));
        using var lightEdge = Stroke(new SKColor(242, 238, 228), Math.Max(1, inset * .22f));
        canvas.DrawRoundRect(panel, inset * 1.5f, inset * 1.5f, panelFill);
        canvas.DrawLine(panel.Left, panel.Top, panel.Right, panel.Top, darkEdge);
        canvas.DrawLine(panel.Left, panel.Top, panel.Left, panel.Bottom, darkEdge);
        canvas.DrawLine(panel.Left, panel.Bottom, panel.Right, panel.Bottom, lightEdge);
        canvas.DrawLine(panel.Right, panel.Top, panel.Right, panel.Bottom, lightEdge);
    }

    private static void DrawThreeHalf(SKCanvas canvas, ScpRenderRequest request, float outer, SKColor shellColor, bool highDensity)
    {
        var halfWidth = outer * 1.10f;
        var halfHeight = outer * 1.08f;
        var shellCenter = request.Center;
        var rect = new SKRect(shellCenter.X - halfWidth, shellCenter.Y - halfHeight, shellCenter.X + halfWidth, shellCenter.Y + halfHeight);
        using var shell = Fill(shellColor);
        using var edge = Stroke(Darken(shellColor, 45), outer * .018f);
        canvas.DrawRoundRect(rect, halfWidth * .045f, halfWidth * .045f, shell);
        canvas.DrawRoundRect(rect, halfWidth * .045f, halfWidth * .045f, edge);

        using var groove = Stroke(Darken(shellColor, 28), outer * .012f);
        canvas.DrawRoundRect(new SKRect(rect.Left + halfWidth * .08f, rect.Top + halfHeight * .08f,
            rect.Right - halfWidth * .08f, rect.Bottom - halfHeight * .08f), halfWidth * .03f, halfWidth * .03f, groove);

        using var metal = Fill(new SKColor(171, 175, 176));
        using var metalEdge = Stroke(new SKColor(94, 99, 102), outer * .012f);
        if (request.Head == 0)
        {
            var shutter = new SKRect(request.Center.X - halfWidth * .42f, rect.Top,
                request.Center.X + halfWidth * .42f, rect.Top + halfHeight * .27f);
            canvas.DrawRoundRect(shutter, halfWidth * .035f, halfWidth * .035f, metal);
            canvas.DrawRoundRect(shutter, halfWidth * .035f, halfWidth * .035f, metalEdge);
            using var opening = Fill(Darken(shellColor, 60));
            canvas.DrawRoundRect(new SKRect(request.Center.X - halfWidth * .11f, rect.Top + halfHeight * .04f,
                request.Center.X + halfWidth * .11f, rect.Top + halfHeight * .23f), halfWidth * .018f, halfWidth * .018f, opening);
            DrawLabel(canvas, new SKRect(rect.Left + halfWidth * .13f, rect.Bottom - halfHeight * .22f,
                rect.Right - halfWidth * .13f, rect.Bottom - halfHeight * .06f));
        }
        else
        {
            var shutter = new SKRect(request.Center.X - halfWidth * .34f, rect.Top,
                request.Center.X + halfWidth * .34f, rect.Top + halfHeight * .25f);
            canvas.DrawRoundRect(shutter, halfWidth * .03f, halfWidth * .03f, metal);
            canvas.DrawRoundRect(shutter, halfWidth * .03f, halfWidth * .03f, metalEdge);
            using var hub = Fill(new SKColor(119, 124, 126));
            canvas.DrawCircle(request.Center, outer * .24f, hub);
            using var hubHole = Fill(new SKColor(29, 32, 35));
            canvas.DrawCircle(request.Center, outer * .105f, hubHole);
            canvas.DrawRoundRect(new SKRect(request.Center.X - outer * .07f, request.Center.Y - outer * .18f,
                request.Center.X + outer * .07f, request.Center.Y - outer * .08f), outer * .015f, outer * .015f, hubHole);
        }

        using var hole = Fill(new SKColor(17, 20, 23));
        // The write-protect opening exists on DD and HD media.
        canvas.DrawRect(new SKRect(rect.Right - halfWidth * .15f, rect.Bottom - halfHeight * .13f,
            rect.Right - halfWidth * .08f, rect.Bottom - halfHeight * .05f), hole);
        // The opposite density-identification opening exists only on HD media.
        if (highDensity)
            canvas.DrawRect(new SKRect(rect.Left + halfWidth * .07f, rect.Bottom - halfHeight * .13f,
                rect.Left + halfWidth * .14f, rect.Bottom - halfHeight * .05f), hole);
    }

    private static void DrawThreeInch(SKCanvas canvas, ScpRenderRequest request, float outer)
    {
        var halfWidth = outer * 1.08f;
        var halfHeight = outer * 1.10f;
        var rect = new SKRect(request.Center.X - halfWidth, request.Center.Y - halfHeight,
            request.Center.X + halfWidth, request.Center.Y + halfHeight);
        using var shell = Fill(new SKColor(28, 30, 34));
        using var edge = Stroke(new SKColor(74, 77, 82), outer * .016f);
        canvas.DrawRoundRect(rect, halfWidth * .025f, halfWidth * .025f, shell);
        canvas.DrawRoundRect(rect, halfWidth * .025f, halfWidth * .025f, edge);
        using var recess = Fill(new SKColor(8, 10, 12));
        canvas.DrawRoundRect(new SKRect(request.Center.X - halfWidth * .18f, rect.Top + halfHeight * .05f,
            request.Center.X + halfWidth * .18f, rect.Top + halfHeight * .31f), halfWidth * .08f, halfWidth * .08f, recess);
        canvas.DrawCircle(request.Center.X - halfWidth * .58f, rect.Top + halfHeight * .17f, outer * .07f, recess);
        canvas.DrawCircle(request.Center.X + halfWidth * .58f, rect.Top + halfHeight * .17f, outer * .07f, recess);
        using var hub = Fill(new SKColor(210, 206, 184));
        canvas.DrawCircle(request.Center, outer * .22f, hub);
        canvas.DrawCircle(request.Center, outer * .105f, recess);
        DrawLabel(canvas, new SKRect(rect.Left + halfWidth * .10f, rect.Bottom - halfHeight * .45f,
            rect.Right - halfWidth * .10f, rect.Bottom - halfHeight * .07f));
    }

    private static void DrawFlexibleDisk(SKCanvas canvas, ScpRenderRequest request, float outer, bool eightInch)
    {
        var half = outer * (eightInch ? 1.10f : 1.08f);
        var rect = new SKRect(request.Center.X - half, request.Center.Y - half, request.Center.X + half, request.Center.Y + half);
        using var shell = Fill(new SKColor(26, 27, 29));
        using var edge = Stroke(new SKColor(68, 70, 73), outer * .015f);
        canvas.DrawRoundRect(rect, half * .018f, half * .018f, shell);
        canvas.DrawRoundRect(rect, half * .018f, half * .018f, edge);

        using var seam = Stroke(new SKColor(49, 51, 54), outer * .012f);
        canvas.DrawLine(rect.Left + half * .08f, rect.Top + half * .08f, rect.Left + half * .32f, rect.Top + half * .32f, seam);
        canvas.DrawLine(rect.Right - half * .08f, rect.Top + half * .08f, rect.Right - half * .32f, rect.Top + half * .32f, seam);
        canvas.DrawLine(rect.Left + half * .08f, rect.Bottom - half * .08f, rect.Left + half * .32f, rect.Bottom - half * .32f, seam);
        canvas.DrawLine(rect.Right - half * .08f, rect.Bottom - half * .08f, rect.Right - half * .32f, rect.Bottom - half * .32f, seam);

        using var opening = Fill(new SKColor(7, 9, 11));
        canvas.DrawRoundRect(new SKRect(request.Center.X - half * .13f, rect.Top + half * .06f,
            request.Center.X + half * .13f, rect.Top + half * .38f), half * .07f, half * .07f, opening);
        canvas.DrawRoundRect(new SKRect(request.Center.X - half * .08f, rect.Bottom - half * .42f,
            request.Center.X + half * .08f, rect.Bottom - half * .08f), half * .07f, half * .07f, opening);
        canvas.DrawCircle(request.Center.X - half * .55f, request.Center.Y - half * .09f, outer * .07f, opening);
        using var hubRing = Stroke(new SKColor(133, 132, 126), outer * .055f);
        canvas.DrawCircle(request.Center, outer * .23f, hubRing);
        if (request.Head == 0)
            DrawLabel(canvas, new SKRect(rect.Left + half * .11f, rect.Top + half * .10f, rect.Left + half * .72f, rect.Top + half * .46f));
    }

    private static void DrawLabel(SKCanvas canvas, SKRect rect)
    {
        using var label = Fill(new SKColor(213, 207, 187));
        using var line = Stroke(new SKColor(168, 91, 72), Math.Max(1, rect.Height * .018f));
        canvas.DrawRoundRect(rect, rect.Width * .025f, rect.Width * .025f, label);
        for (var y = rect.Top + rect.Height * .32f; y < rect.Bottom - rect.Height * .08f; y += rect.Height * .19f)
            canvas.DrawLine(rect.Left + rect.Width * .08f, y, rect.Right - rect.Width * .08f, y, line);
    }

    private static SKPaint Fill(SKColor color) => new() { Color = color, IsAntialias = true, Style = SKPaintStyle.Fill };
    private static SKPaint Stroke(SKColor color, float width) => new() { Color = color, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = Math.Max(1, width) };
    private static SKColor Darken(SKColor color, byte amount) => new((byte)Math.Max(0, color.Red - amount), (byte)Math.Max(0, color.Green - amount), (byte)Math.Max(0, color.Blue - amount));

    private PreparedTrack PrepareTrack(ScpTrack track, ScpRevolution revolution, string? decoderId, CancellationToken cancellationToken)
    {
        var intervals = revolution.FluxIntervals;
        var sampleStep = Math.Max(1, intervals.Count / 720);
        var total = intervals.Sum(interval => (double)interval);
        var ordered = intervals.ToArray();
        Array.Sort(ordered);
        var median = ordered[ordered.Length / 2];
        var fluxArcs = new List<PreparedArc>(Math.Min(720, intervals.Count));
        var shortTransitionCount = 0;
        var longTransitionCount = 0;
        var normalFluxCount = 0;
        double elapsed = 0;
        for (var index = 0; index < intervals.Count; index += sampleStep)
        {
            cancellationToken.ThrowIfCancellationRequested();
            double span = 0;
            for (var sample = index; sample < Math.Min(index + sampleStep, intervals.Count); sample++) span += intervals[sample];
            var color = intervals[index] < median * .65 ? new SKColor(143, 104, 255) : intervals[index] > median * 1.8 ? new SKColor(83, 173, 255) : new SKColor(36, 179, 93);
            if (color == new SKColor(143, 104, 255)) shortTransitionCount++;
            else if (color == new SKColor(83, 173, 255)) longTransitionCount++;
            else normalFluxCount++;
            fluxArcs.Add(new((float)(elapsed / total * 360 - 90), Math.Max(.08f, (float)(span / total * 360)), color));
            elapsed += span;
        }

        var structureArcs = new List<PreparedArc>();
        var best = _decoders.DecodeBest(track.Revolutions, decoderId);
        FluxDecodeResult? decodedResult = null;
        if (best is not null)
        {
            var decodedRevolution = track.Revolutions[best.Value.RevolutionIndex];
            var decoded = best.Value.Result;
            decodedResult = decoded;
            if (decoded.EstimatedBitCellTicks > 0)
            {
                var totalBits = Math.Max(1d, decodedRevolution.FluxIntervals.Sum(interval => (double)interval) / decoded.EstimatedBitCellTicks);
                structureArcs.AddRange(decoded.Structures.Select(structure => new PreparedArc(
                    (float)(structure.BitOffset / totalBits * 360 - 90),
                    Math.Max(.18f, (float)(structure.BitLength / totalBits * 360)),
                    StructureColor(structure.Kind))));
            }
        }
        var sectors = decodedResult?.Sectors ?? [];
        return new(
            fluxArcs,
            structureArcs,
            Classify(decodedResult, shortTransitionCount, longTransitionCount, normalFluxCount),
            sectors.Count(sector => sector.IntegrityValid == true),
            sectors.Count(sector => sector.IntegrityValid == false),
            sectors.Count(sector => sector.IntegrityValid is null),
            true);
    }

    internal static ScpTrackVisualState Classify(FluxDecodeResult? decoded, int shortTransitions, int longTransitions, int normalFlux)
    {
        if (decoded is not null)
        {
            var sectors = decoded.Sectors ?? [];
            if (sectors.Any(sector => sector.IntegrityValid == false))
                return ScpTrackVisualState.Anomaly;
            if (sectors.Count > 0 && sectors.All(sector => sector.IntegrityValid == true))
                return ScpTrackVisualState.NormalFlux;
            if (decoded.DecodedBytes.Count > 0 || decoded.Structures.Any(structure => structure.Kind is FluxStructureKind.DataAddressMark or FluxStructureKind.DeletedDataAddressMark or FluxStructureKind.AppleData or FluxStructureKind.FormatData))
                return ScpTrackVisualState.DecodedData;
            if (decoded.Structures.Any(structure => structure.Kind == FluxStructureKind.TimingAnomaly))
                return ScpTrackVisualState.LongTransition;
            if (decoded.Structures.Any(structure => structure.Kind is FluxStructureKind.IdAddressMark or FluxStructureKind.AppleAddress or FluxStructureKind.CommodoreHeader or FluxStructureKind.FormatHeader))
                return ScpTrackVisualState.Header;
            if (decoded.Structures.Count > 0)
                return ScpTrackVisualState.ShortTransition;
        }

        if (shortTransitions > normalFlux && shortTransitions >= longTransitions)
            return ScpTrackVisualState.ShortTransition;
        if (longTransitions > normalFlux)
            return ScpTrackVisualState.LongTransition;
        return ScpTrackVisualState.NormalFlux;
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
        FluxStructureKind.DeletedDataAddressMark => new SKColor(255, 75, 96),
        FluxStructureKind.TimingAnomaly => new SKColor(83, 173, 255),
        _ => new SKColor(196, 117, 255)
    };

    private static void DrawCentered(SKCanvas canvas, SKPoint center, string text, SKColor color)
    {
        using var paint = new SKPaint { Color = color, IsAntialias = true };
        using var font = new SKFont(SKTypeface.Default, 17);
        var lines = text.Split('\n');
        for (var index = 0; index < lines.Length; index++) canvas.DrawText(lines[index], center.X, center.Y + index * 20, SKTextAlign.Center, font, paint);
    }

    private sealed record PreparedTrack(
        IReadOnlyList<PreparedArc> FluxArcs,
        IReadOnlyList<PreparedArc> StructureArcs,
        ScpTrackVisualState VisualState,
        int ValidSectors,
        int InvalidSectors,
        int UnverifiedSectors,
        bool HasFlux);
    private sealed record PreparedArc(float Start, float Sweep, SKColor Color);
}
