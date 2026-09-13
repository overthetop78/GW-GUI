using SkiaSharp;

namespace GWGUI.App.Rendering.Scp;

public sealed partial class SkiaScpRenderer
{
    private static void DrawRecessedBackground(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(new SKColor(239, 244, 247));
        var bounds = new SKRect(0, 0, width, height);
        using var gradient = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(width, height),
            [new SKColor(250, 252, 253), new SKColor(235, 241, 245), new SKColor(222, 230, 235)],
            [0, .62f, 1],
            SKShaderTileMode.Clamp);
        using var fill = new SKPaint { Shader = gradient, IsAntialias = true };
        canvas.DrawRect(bounds, fill);

        var radius = Math.Max(width, height) * .78f;
        using var vignette = SKShader.CreateRadialGradient(
            new SKPoint(width / 2f, height / 2f),
            radius,
            [new SKColor(255, 255, 255, 80), new SKColor(111, 131, 144, 30)],
            [0, 1],
            SKShaderTileMode.Clamp);
        using var overlay = new SKPaint { Shader = vignette, IsAntialias = true };
        canvas.DrawRect(bounds, overlay);
    }
}
