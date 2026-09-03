using GWGUI.Emulation.Contracts;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterPlasma
{
    private static readonly int[] Bayer4X4 =
    [
        0, 8, 2, 10,
        12, 4, 14, 6,
        3, 11, 1, 9,
        15, 7, 13, 5
    ];

    internal static void Apply(float[] colors, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, long sequence,
        EmulationPlasmaVideoConfiguration configuration)
    {
        ApplyCells(colors, sourceWidth, sourceHeight, outputWidth, outputHeight,
            configuration.CellStructure);
        ApplyTemporalDithering(colors, outputWidth, outputHeight, sequence,
            configuration.TemporalDithering);
        ApplyDiffusion(colors, outputWidth, outputHeight, configuration.Diffusion);
    }

    internal static void ApplyCells(float[] colors, int sourceWidth, int sourceHeight,
        int outputWidth, int outputHeight, int setting)
    {
        if (setting == 0) return;
        var strength = setting / 100f;
        var channelAttenuation = strength * 0.35f;
        var halfGap = strength * 0.20f;
        var scaleX = sourceWidth / (float)outputWidth;
        var scaleY = sourceHeight / (float)outputHeight;
        for (var y = 0; y < outputHeight; y++)
        {
            var sourceY = (y + 0.5f) * scaleY;
            var fractionY = sourceY - MathF.Floor(sourceY);
            for (var x = 0; x < outputWidth; x++)
            {
                var sourceX = (x + 0.5f) * scaleX;
                var fractionX = sourceX - MathF.Floor(sourceX);
                var selected = Math.Min(2, (int)(fractionX * 3f));
                var index = (y * outputWidth + x) * 3;
                for (var channel = 0; channel < 3; channel++)
                    if (channel != selected) colors[index + channel] *= 1f - channelAttenuation;

                var edge = Math.Min(Math.Min(fractionX, 1f - fractionX),
                    Math.Min(fractionY, 1f - fractionY));
                if (edge >= halfGap) continue;
                var coverage = 1f - edge / Math.Max(halfGap, float.Epsilon);
                var factor = 1f - strength * 0.5f * coverage;
                colors[index] *= factor;
                colors[index + 1] *= factor;
                colors[index + 2] *= factor;
            }
        }
    }

    internal static void ApplyTemporalDithering(float[] colors, int width, int height,
        long sequence, int setting)
    {
        if (setting == 0) return;
        var amplitude = setting / 100f * 0.04f;
        var phase = (int)(Math.Abs(sequence) % 4);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var bayer = Bayer4X4[((y + phase) & 3) * 4 + ((x + phase) & 3)];
            var offset = (bayer - 7.5f) / 7.5f * amplitude;
            var index = (y * width + x) * 3;
            colors[index] = Math.Clamp(colors[index] + offset, 0f, 1f);
            colors[index + 1] = Math.Clamp(colors[index + 1] + offset, 0f, 1f);
            colors[index + 2] = Math.Clamp(colors[index + 2] + offset, 0f, 1f);
        }
    }

    internal static void ApplyDiffusion(float[] colors, int width, int height, int setting)
    {
        if (setting == 0) return;
        var source = colors.ToArray();
        var amount = setting / 100f * 0.5f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var channel = 0; channel < 3; channel++)
        {
            var sum = 0f;
            for (var offsetY = -1; offsetY <= 1; offsetY++)
            for (var offsetX = -1; offsetX <= 1; offsetX++)
            {
                var sampleX = Math.Clamp(x + offsetX, 0, width - 1);
                var sampleY = Math.Clamp(y + offsetY, 0, height - 1);
                sum += source[(sampleY * width + sampleX) * 3 + channel];
            }
            var index = (y * width + x) * 3 + channel;
            colors[index] += (sum / 9f - colors[index]) * amount;
        }
    }
}
