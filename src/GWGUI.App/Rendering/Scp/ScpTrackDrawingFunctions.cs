using GWGUI.App.Contracts.Rendering.Scp;
using GWGUI.MediaEngine.Decoding;
using SkiaSharp;

namespace GWGUI.App.Rendering.Scp;

public sealed partial class SkiaScpRenderer
{
    private static void DrawDecodedStructures(SKCanvas canvas, SKRect trackRect, IReadOnlyList<PreparedScpArc> arcs, SKPaint header, SKPaint data, SKPaint error, SKPaint other)
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
}
