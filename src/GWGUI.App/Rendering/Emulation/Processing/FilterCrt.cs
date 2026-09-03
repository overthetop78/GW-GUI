using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterCrt
{
    public static void Apply(float[] colors, int width, int height,
        EmulationCrtVideoConfiguration configuration)
    {
        ApplyCurvature(colors, width, height, configuration.Curvature);
        ApplyBeam(colors, width, height, configuration.BeamWidth,
            configuration.BeamIntensity, configuration.BeamDiffusion);
        ApplyHalo(colors, width, height, configuration.HaloIntensity);
        ApplyMask(colors, width, height, configuration.Mask,
            configuration.MaskSubpixels, configuration.MaskIntensity);
        ApplyScanlines(colors, width, height, configuration.ScanlinesEnabled,
            configuration.ScanlineOrientation, configuration.ScanlineIntensity,
            configuration.ScanlineThickness, configuration.ScanlinePhase,
            configuration.ScanlineCompensation);
        ApplyPattern(colors, width, height, configuration.PatternEnabled,
            configuration.PatternOrientation, configuration.PatternFrequency,
            configuration.PatternPhase, configuration.PatternIntensity);
        ApplyVignette(colors, width, height, configuration.Vignette);
    }

    internal static void ApplyCurvature(float[] colors, int width, int height, int setting)
    {
        if (setting == 0 || width < 2 || height < 2) return;
        var source = colors.ToArray();
        var curvature = setting / 100f * 0.18f;
        for (var y = 0; y < height; y++)
        {
            var normalizedY = 2f * (y + 0.5f) / height - 1f;
            for (var x = 0; x < width; x++)
            {
                var normalizedX = 2f * (x + 0.5f) / width - 1f;
                var warpedX = normalizedX * (1f + curvature * normalizedY * normalizedY);
                var warpedY = normalizedY * (1f + curvature * normalizedX * normalizedX);
                var output = (y * width + x) * 3;
                if (MathF.Abs(warpedX) > 1f || MathF.Abs(warpedY) > 1f)
                {
                    colors[output] = colors[output + 1] = colors[output + 2] = 0f;
                    continue;
                }
                var sourceX = (warpedX + 1f) * width * 0.5f - 0.5f;
                var sourceY = (warpedY + 1f) * height * 0.5f - 0.5f;
                for (var channel = 0; channel < 3; channel++)
                    colors[output + channel] = Bilinear(source, width, height,
                        sourceX, sourceY, channel);
            }
        }
    }

    internal static void ApplyBeam(float[] colors, int width, int height,
        int widthSetting, int intensitySetting, int diffusionSetting)
    {
        if (widthSetting == 0 && intensitySetting == 0 && diffusionSetting == 0) return;
        var source = colors.ToArray();
        var verticalMix = widthSetting / 100f * 0.45f;
        var diffusionMix = diffusionSetting / 100f * 0.35f;
        var gain = 1f + intensitySetting / 100f * 0.5f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var channel = 0; channel < 3; channel++)
        {
            var index = (y * width + x) * 3 + channel;
            var vertical = (Sample(source, width, height, x, y - 1, channel)
                + Sample(source, width, height, x, y + 1, channel)) * 0.5f;
            var neighborhood = Neighborhood(source, width, height, x, y, channel);
            var value = Lerp(source[index], vertical, verticalMix);
            value = Lerp(value, neighborhood, diffusionMix);
            colors[index] = Math.Clamp(value * gain, 0f, 1f);
        }
    }

    internal static void ApplyHalo(float[] colors, int width, int height, int setting)
    {
        if (setting == 0) return;
        var source = colors.ToArray();
        var strength = setting / 100f * 0.5f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var channel = 0; channel < 3; channel++)
        {
            var index = (y * width + x) * 3 + channel;
            colors[index] = Math.Clamp(source[index]
                + Neighborhood(source, width, height, x, y, channel) * strength, 0f, 1f);
        }
    }

    internal static void ApplyMask(float[] colors, int width, int height,
        EmulationCrtMask mask, EmulationSubpixelLayout layout, int setting)
    {
        if (mask == EmulationCrtMask.None || setting == 0) return;
        var strength = setting / 100f * 0.75f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var selected = layout == EmulationSubpixelLayout.Monochrome
                ? -1
                : (layout == EmulationSubpixelLayout.Bgr ? 2 - x % 3 : x % 3);
            if (mask == EmulationCrtMask.ShadowMask) selected = (selected + y % 2) % 3;
            var slotGap = mask == EmulationCrtMask.SlotMask && y % 4 == 3;
            for (var channel = 0; channel < 3; channel++)
            {
                var attenuation = slotGap || (selected >= 0 && channel != selected)
                    ? strength : strength * 0.18f;
                if (layout == EmulationSubpixelLayout.Monochrome)
                    attenuation = ((x + y) & 1) == 0 ? strength * 0.18f : strength;
                var index = (y * width + x) * 3 + channel;
                colors[index] *= 1f - attenuation;
            }
        }
    }

    internal static void ApplyVignette(float[] colors, int width, int height, int setting)
    {
        if (setting == 0) return;
        var strength = setting / 100f * 0.75f;
        for (var y = 0; y < height; y++)
        {
            var normalizedY = 2f * (y + 0.5f) / height - 1f;
            for (var x = 0; x < width; x++)
            {
                var normalizedX = 2f * (x + 0.5f) / width - 1f;
                var radius = Math.Clamp((normalizedX * normalizedX
                    + normalizedY * normalizedY) * 0.5f, 0f, 1f);
                var factor = 1f - strength * MathF.Pow(radius, 1.5f);
                var index = (y * width + x) * 3;
                colors[index] *= factor;
                colors[index + 1] *= factor;
                colors[index + 2] *= factor;
            }
        }
    }

    internal static void ApplyScanlines(float[] colors, int width, int height, bool enabled,
        EmulationPatternOrientation orientation, int intensitySetting, int thicknessSetting,
        int phaseSetting, int compensationSetting)
    {
        if (!enabled || intensitySetting == 0) return;
        var intensity = intensitySetting / 100f;
        var exponent = Lerp(8f, 0.5f, thicknessSetting / 100f);
        var phase = phaseSetting / 50f;
        var compensation = 1f + compensationSetting / 100f * intensity * 0.5f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var coordinate = orientation == EmulationPatternOrientation.Horizontal ? y : x;
            var wave = 0.5f + 0.5f * MathF.Cos(MathF.PI * (coordinate + 0.25f + phase));
            var factor = (1f - intensity * MathF.Pow(wave, exponent)) * compensation;
            var index = (y * width + x) * 3;
            colors[index] = Math.Clamp(colors[index] * factor, 0f, 1f);
            colors[index + 1] = Math.Clamp(colors[index + 1] * factor, 0f, 1f);
            colors[index + 2] = Math.Clamp(colors[index + 2] * factor, 0f, 1f);
        }
    }

    internal static void ApplyPattern(float[] colors, int width, int height, bool enabled,
        EmulationPatternOrientation orientation, int frequencySetting, int phaseSetting,
        int intensitySetting)
    {
        if (!enabled || intensitySetting == 0) return;
        var axisLength = orientation == EmulationPatternOrientation.Horizontal ? height : width;
        var cycles = 1f + frequencySetting / 100f * 31f;
        var phase = phaseSetting / 100f * 2f * MathF.PI;
        var intensity = intensitySetting / 100f * 0.5f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var coordinate = orientation == EmulationPatternOrientation.Horizontal ? y : x;
            var wave = 0.5f + 0.5f * MathF.Cos(2f * MathF.PI
                * (coordinate + 0.5f) * cycles / axisLength + phase);
            var factor = 1f - intensity * wave;
            var index = (y * width + x) * 3;
            colors[index] *= factor;
            colors[index + 1] *= factor;
            colors[index + 2] *= factor;
        }
    }

    private static float Neighborhood(float[] colors, int width, int height,
        int x, int y, int channel)
    {
        var sum = 0f;
        for (var offsetY = -1; offsetY <= 1; offsetY++)
        for (var offsetX = -1; offsetX <= 1; offsetX++)
            sum += Sample(colors, width, height, x + offsetX, y + offsetY, channel);
        return sum / 9f;
    }

    private static float Bilinear(float[] colors, int width, int height,
        float x, float y, int channel)
    {
        var left = (int)MathF.Floor(x);
        var top = (int)MathF.Floor(y);
        var fx = x - left;
        var fy = y - top;
        return Lerp(Lerp(Sample(colors, width, height, left, top, channel),
            Sample(colors, width, height, left + 1, top, channel), fx),
            Lerp(Sample(colors, width, height, left, top + 1, channel),
                Sample(colors, width, height, left + 1, top + 1, channel), fx), fy);
    }

    private static float Sample(float[] colors, int width, int height,
        int x, int y, int channel) => colors[(Math.Clamp(y, 0, height - 1) * width
        + Math.Clamp(x, 0, width - 1)) * 3 + channel];

    private static float Lerp(float first, float second, float amount) =>
        first + (second - first) * amount;
}
