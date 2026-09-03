using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplay
{
    private const int CellWidth = 8;
    private const int CellHeight = 12;
    private static readonly Segment[] SevenSegments =
    [
        new(.2f,.08f,.8f,.08f), new(.82f,.1f,.82f,.48f), new(.82f,.52f,.82f,.9f),
        new(.2f,.92f,.8f,.92f), new(.18f,.52f,.18f,.9f), new(.18f,.1f,.18f,.48f),
        new(.2f,.5f,.8f,.5f)
    ];
    private static readonly Segment[] FourteenSegments =
    [
        ..SevenSegments,
        new(.2f,.1f,.48f,.47f), new(.8f,.1f,.52f,.47f),
        new(.2f,.9f,.48f,.53f), new(.8f,.9f,.52f,.53f),
        new(.5f,.1f,.5f,.47f), new(.5f,.53f,.5f,.9f)
    ];
    private static readonly Segment[] SixteenSegments =
    [
        ..FourteenSegments,
        new(.28f,.3f,.72f,.3f), new(.28f,.7f,.72f,.7f)
    ];

    internal static void Apply(float[] colors, int width, int height,
        EmulationSegmentDisplayVideoConfiguration configuration)
    {
        var source = colors.ToArray();
        var segments = configuration.Layout switch
        {
            EmulationSegmentDisplayLayout.Fourteen => FourteenSegments,
            EmulationSegmentDisplayLayout.Sixteen => SixteenSegments,
            _ => SevenSegments
        };
        var tint = Tint(configuration.Color);
        var thickness = 0.025f + configuration.Thickness / 100f * 0.105f;
        var contrast = 0.65f + configuration.Contrast / 100f * 2.35f;
        var glow = configuration.Glow / 100f * 0.5f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var index = (y * width + x) * 3;
            var luminance = source[index] * 0.2126f + source[index + 1] * 0.7152f
                + source[index + 2] * 0.0722f;
            var activation = Math.Clamp((luminance - 0.5f) * contrast + 0.5f, 0f, 1f);
            var localX = ((x % CellWidth) + 0.5f) / CellWidth;
            var localY = ((y % CellHeight) + 0.5f) / CellHeight;
            var distance = segments.Min(segment => Distance(localX, localY, segment));
            var core = Math.Clamp((thickness - distance) * 25f, 0f, 1f);
            var halo = Math.Clamp(1f - distance / 0.22f, 0f, 1f) * glow;
            var emission = activation * Math.Clamp(core + halo * (1f - core), 0f, 1f);
            colors[index] = Math.Clamp(emission * tint.R, 0f, 1f);
            colors[index + 1] = Math.Clamp(emission * tint.G, 0f, 1f);
            colors[index + 2] = Math.Clamp(emission * tint.B, 0f, 1f);
        }
    }

    private static float Distance(float x, float y, Segment segment)
    {
        var dx = segment.EndX - segment.StartX;
        var dy = segment.EndY - segment.StartY;
        var lengthSquared = dx * dx + dy * dy;
        var position = Math.Clamp(((x - segment.StartX) * dx + (y - segment.StartY) * dy)
            / lengthSquared, 0f, 1f);
        var nearestX = segment.StartX + position * dx;
        var nearestY = segment.StartY + position * dy;
        return MathF.Sqrt((x - nearestX) * (x - nearestX) + (y - nearestY) * (y - nearestY));
    }

    private static (float R, float G, float B) Tint(EmulationSegmentDisplayColor color) =>
        color switch
        {
            EmulationSegmentDisplayColor.Green => (0.04f, 1f, 0.08f),
            EmulationSegmentDisplayColor.Amber => (1f, 0.42f, 0.015f),
            EmulationSegmentDisplayColor.Blue => (0.03f, 0.28f, 1f),
            EmulationSegmentDisplayColor.White => (1f, 1f, 1f),
            _ => (1f, 0.025f, 0.01f)
        };

    private readonly record struct Segment(float StartX, float StartY, float EndX, float EndY);
}
