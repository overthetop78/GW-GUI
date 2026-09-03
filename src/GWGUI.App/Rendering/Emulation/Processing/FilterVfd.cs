using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVfd
{
    internal static void Apply(float[] colors, int width, int height,
        EmulationVfdVideoConfiguration configuration)
    {
        var source = colors.ToArray();
        var tint = configuration.Color switch
        {
            EmulationVfdColor.Green => (0.12f, 1f, 0.28f),
            EmulationVfdColor.Amber => (1f, 0.48f, 0.04f),
            EmulationVfdColor.Red => (1f, 0.08f, 0.03f),
            _ => (0.05f, 0.55f, 1f)
        };
        var intensity = 0.4f + configuration.PhosphorIntensity / 100f * 1.2f;
        var halo = configuration.HaloIntensity / 100f * 0.6f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var index = (y * width + x) * 3;
            var luminance = Luminance(source, index);
            var neighborhood = 0f;
            for (var offsetY = -1; offsetY <= 1; offsetY++)
            for (var offsetX = -1; offsetX <= 1; offsetX++)
            {
                var sampleX = Math.Clamp(x + offsetX, 0, width - 1);
                var sampleY = Math.Clamp(y + offsetY, 0, height - 1);
                neighborhood += Luminance(source, (sampleY * width + sampleX) * 3);
            }
            var emission = Math.Clamp(luminance * intensity + neighborhood / 9f * halo, 0f, 1f);
            colors[index] = emission * tint.Item1;
            colors[index + 1] = emission * tint.Item2;
            colors[index + 2] = emission * tint.Item3;
        }
    }

    private static float Luminance(float[] colors, int index) =>
        colors[index] * 0.2126f + colors[index + 1] * 0.7152f + colors[index + 2] * 0.0722f;
}
