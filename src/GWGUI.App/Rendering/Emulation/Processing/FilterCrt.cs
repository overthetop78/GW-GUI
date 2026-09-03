using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterCrt
{
    public static void Apply(float[] colors, int width, int height, int sourceWidth, int sourceHeight,
        EmulationCrtVideoConfiguration configuration)
    {
        ApplyGeometry(colors, width, height, configuration.HorizontalCurvature,
            configuration.VerticalCurvature, configuration.Trapezoid);
        ApplyBeam(colors, width, height, configuration.BeamWidth,
            configuration.BeamIntensity, configuration.BeamDiffusion);
        ApplyHalo(colors, width, height, configuration.HaloIntensity);
        ApplyMask(colors, width, height, sourceWidth, sourceHeight, configuration.Mask,
            configuration.ColorMode == EmulationCrtColorMode.Color
                ? configuration.MaskSubpixels : EmulationSubpixelLayout.Monochrome,
            configuration.MaskIntensity);
        ApplyScanlines(colors, width, height, sourceWidth, sourceHeight,
            configuration.ScanlinesEnabled,
            configuration.ScanlineOrientation, configuration.ScanlineIntensity,
            configuration.ScanlineThickness, configuration.ScanlinePhase,
            configuration.ScanlineCompensation);
        ApplyPattern(colors, width, height, sourceWidth, sourceHeight, configuration.PatternEnabled,
            configuration.PatternOrientation, configuration.PatternFrequency,
            configuration.PatternPhase, configuration.PatternIntensity);
        ApplyVignette(colors, width, height, configuration.Vignette);
    }

    internal static void ApplyGeometry(float[] colors, int width, int height,
        int horizontalSetting, int verticalSetting, int trapezoidSetting)
    {
        if ((horizontalSetting == 0 && verticalSetting == 0 && trapezoidSetting == 0)
            || width < 2 || height < 2) return;
        var source = colors.ToArray();
        var horizontal = horizontalSetting / 100f * 0.28f;
        var vertical = verticalSetting / 100f * 0.28f;
        var trapezoid = trapezoidSetting / 100f * 0.22f;
        for (var y = 0; y < height; y++)
        {
            var normalizedY = 2f * (y + 0.5f) / height - 1f;
            for (var x = 0; x < width; x++)
            {
                var normalizedX = 2f * (x + 0.5f) / width - 1f;
                var warpedX = normalizedX * (1f + horizontal * normalizedY * normalizedY
                    + trapezoid * normalizedY);
                var warpedY = normalizedY * (1f + vertical * normalizedX * normalizedX);
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
        var diffusionMix = diffusionSetting / 100f * 0.72f;
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
        var strength = setting / 100f * 0.85f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        for (var channel = 0; channel < 3; channel++)
        {
            var index = (y * width + x) * 3 + channel;
            var glow = Math.Max(0f, Neighborhood(source, width, height, x, y, channel) - 0.35f);
            colors[index] = Math.Clamp(source[index] + glow * strength, 0f, 1f);
        }
    }

    internal static void ApplyMask(float[] colors, int width, int height,
        int sourceWidth, int sourceHeight,
        EmulationCrtMask mask, EmulationSubpixelLayout layout, int setting)
    {
        if (mask == EmulationCrtMask.None || setting == 0) return;
        var strength = setting / 100f * 0.75f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var maskX = x * sourceWidth / Math.Max(1, width);
            var maskY = y * sourceHeight / Math.Max(1, height);
            var selected = layout == EmulationSubpixelLayout.Monochrome
                ? -1
                : (layout == EmulationSubpixelLayout.Bgr ? 2 - maskX % 3 : maskX % 3);
            if (mask == EmulationCrtMask.ShadowMask) selected = (selected + maskY % 2) % 3;
            var slotGap = mask == EmulationCrtMask.SlotMask && maskY % 4 == 3;
            for (var channel = 0; channel < 3; channel++)
            {
                var attenuation = slotGap || (selected >= 0 && channel != selected)
                    ? strength : strength * 0.18f;
                if (layout == EmulationSubpixelLayout.Monochrome)
                    attenuation = ((maskX + maskY) & 1) == 0 ? strength * 0.10f : strength;
                var index = (y * width + x) * 3 + channel;
                colors[index] *= 1f - attenuation;
            }
        }
    }

    internal static void ApplyVignette(float[] colors, int width, int height, int setting)
    {
        if (setting == 0) return;
        var strength = setting / 100f * 0.92f;
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

    internal static void ApplyScanlines(float[] colors, int width, int height,
        int sourceWidth, int sourceHeight, bool enabled, EmulationPatternOrientation orientation,
        int intensitySetting, int thicknessSetting, EmulationScanlinePhase phaseSetting,
        int compensationSetting)
    {
        if (!enabled || intensitySetting == 0) return;
        var intensity = intensitySetting / 100f;
        var thickness = thicknessSetting / 100f;
        var phase = (int)phaseSetting * 0.25f;
        var gapStart = Lerp(0.47f, 0.18f, thickness);
        var gapCoverage = 1f - gapStart * 2f;
        var compensation = 1f + compensationSetting / 100f * intensity
            * gapCoverage * 0.45f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var coordinate = orientation == EmulationPatternOrientation.Horizontal
                ? (y + 0.5f) * sourceHeight / Math.Max(1f, height)
                : (x + 0.5f) * sourceWidth / Math.Max(1f, width);
            var cycle = (coordinate + phase) * 0.5f;
            cycle -= MathF.Floor(cycle);
            var distanceFromBeam = MathF.Abs(cycle - 0.25f);
            distanceFromBeam = Math.Min(distanceFromBeam, 1f - distanceFromBeam);
            var gap = SmoothStep(gapStart, Math.Min(0.5f, gapStart + 0.055f),
                distanceFromBeam);
            var factor = (1f - intensity * gap * 0.94f) * compensation;
            var index = (y * width + x) * 3;
            colors[index] = Math.Clamp(colors[index] * factor, 0f, 1f);
            colors[index + 1] = Math.Clamp(colors[index + 1] * factor, 0f, 1f);
            colors[index + 2] = Math.Clamp(colors[index + 2] * factor, 0f, 1f);
        }
    }

    internal static void ApplyPattern(float[] colors, int width, int height,
        int sourceWidth, int sourceHeight, bool enabled,
        EmulationPatternOrientation orientation, int frequencySetting, int phaseSetting,
        int intensitySetting)
    {
        if (!enabled || intensitySetting == 0) return;
        var axisLength = orientation == EmulationPatternOrientation.Horizontal
            ? sourceHeight : sourceWidth;
        var cycles = 1f + frequencySetting / 100f * 31f;
        var phase = phaseSetting / 100f * 2f * MathF.PI;
        var intensity = intensitySetting / 100f * 0.85f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var coordinate = orientation == EmulationPatternOrientation.Horizontal
                ? y * sourceHeight / Math.Max(1f, height)
                : x * sourceWidth / Math.Max(1f, width);
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

    private static float SmoothStep(float edge0, float edge1, float value)
    {
        var amount = Math.Clamp((value - edge0) / Math.Max(0.0001f, edge1 - edge0), 0f, 1f);
        return amount * amount * (3f - 2f * amount);
    }
}
