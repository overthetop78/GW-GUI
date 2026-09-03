using GWGUI.Emulation.Enums;

namespace GWGUI.App.Functions.Rendering.Emulation;

internal static class EmulationImageRestorationFunctions
{
    private const float SimilarityThreshold = 0.025f;
    private const float MinimumPatternContrast = 0.015f;
    private const float MaximumPatternContrast = 0.45f;

    internal static void ApplyDeinterlacing(float[] colors, int width, int height,
        EmulationDeinterlacingMode mode)
    {
        if (mode == EmulationDeinterlacingMode.Off || height < 2) return;
        var source = colors.ToArray();
        for (var y = 0; y < height; y++)
        {
            if (mode == EmulationDeinterlacingMode.BobEvenLines && (y & 1) == 0) continue;
            if (mode == EmulationDeinterlacingMode.BobOddLines && (y & 1) != 0) continue;
            var firstY = Math.Max(0, y - 1);
            var secondY = Math.Min(height - 1, y + 1);
            if (mode == EmulationDeinterlacingMode.BobEvenLines
                || mode == EmulationDeinterlacingMode.BobOddLines)
            {
                var parity = mode == EmulationDeinterlacingMode.BobEvenLines ? 0 : 1;
                if ((firstY & 1) != parity) firstY = secondY;
                if ((secondY & 1) != parity) secondY = firstY;
            }

            for (var x = 0; x < width; x++)
            for (var channel = 0; channel < 3; channel++)
            {
                var offset = (y * width + x) * 3 + channel;
                var first = source[(firstY * width + x) * 3 + channel];
                var second = source[(secondY * width + x) * 3 + channel];
                colors[offset] = mode == EmulationDeinterlacingMode.Blend
                    ? first * 0.25f + source[offset] * 0.5f + second * 0.25f
                    : (first + second) * 0.5f;
            }
        }
    }

    // Adapted from Hyllian's MIT-licensed checkerboard-dedither passes (2011-2022).
    internal static void ApplyDedithering(float[] colors, int width, int height, int intensity)
    {
        if (intensity <= 0 || width < 2 || height < 2) return;
        var source = colors.ToArray();
        var strength = Math.Clamp(intensity / 100f, 0f, 1f);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = Read(x, y);
            var left = Read(x - 1, y); var right = Read(x + 1, y);
            var up = Read(x, y - 1); var down = Read(x, y + 1);
            var axial = Average(left, right, up, down);
            var diagonalMatches = 0;
            if (Similar(center, Read(x - 1, y - 1))) diagonalMatches++;
            if (Similar(center, Read(x + 1, y - 1))) diagonalMatches++;
            if (Similar(center, Read(x - 1, y + 1))) diagonalMatches++;
            if (Similar(center, Read(x + 1, y + 1))) diagonalMatches++;
            var axialMatches = 0;
            if (Similar(axial, left)) axialMatches++;
            if (Similar(axial, right)) axialMatches++;
            if (Similar(axial, up)) axialMatches++;
            if (Similar(axial, down)) axialMatches++;
            var contrast = Distance(center, axial);
            if (diagonalMatches < 3 || axialMatches < 3
                || contrast < MinimumPatternContrast || contrast > MaximumPatternContrast) continue;

            var candidate = Average(center, axial);
            var offset = (y * width + x) * 3;
            colors[offset] = Lerp(center.Red, candidate.Red, strength);
            colors[offset + 1] = Lerp(center.Green, candidate.Green, strength);
            colors[offset + 2] = Lerp(center.Blue, candidate.Blue, strength);
        }

        Color Read(int x, int y)
        {
            x = Math.Clamp(x, 0, width - 1);
            y = Math.Clamp(y, 0, height - 1);
            var offset = (y * width + x) * 3;
            return new Color(source[offset], source[offset + 1], source[offset + 2]);
        }
    }

    // Original edge-preserving bilateral pass; no third-party shader code is copied.
    internal static void ApplyDenoising(float[] colors, int width, int height, int intensity)
    {
        if (intensity <= 0 || width < 2 || height < 2) return;
        var source = colors.ToArray();
        var strength = Math.Clamp(intensity / 100f, 0f, 1f);
        var rangeSigma = 0.02f + 0.16f * strength;
        var inverseRangeVariance = 1f / (2f * rangeSigma * rangeSigma);

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = Read(x, y);
            var red = 0f; var green = 0f; var blue = 0f; var weightSum = 0f;
            for (var offsetY = -1; offsetY <= 1; offsetY++)
            for (var offsetX = -1; offsetX <= 1; offsetX++)
            {
                var sample = Read(x + offsetX, y + offsetY);
                var spatialWeight = offsetX == 0 && offsetY == 0
                    ? 4f
                    : offsetX == 0 || offsetY == 0 ? 2f : 1f;
                var distance = DistanceSquared(center, sample);
                var weight = spatialWeight * MathF.Exp(-distance * inverseRangeVariance);
                red += sample.Red * weight;
                green += sample.Green * weight;
                blue += sample.Blue * weight;
                weightSum += weight;
            }

            var filtered = new Color(red / weightSum, green / weightSum, blue / weightSum);
            var offset = (y * width + x) * 3;
            colors[offset] = Lerp(center.Red, filtered.Red, strength);
            colors[offset + 1] = Lerp(center.Green, filtered.Green, strength);
            colors[offset + 2] = Lerp(center.Blue, filtered.Blue, strength);
        }

        Color Read(int x, int y)
        {
            x = Math.Clamp(x, 0, width - 1);
            y = Math.Clamp(y, 0, height - 1);
            var offset = (y * width + x) * 3;
            return new Color(source[offset], source[offset + 1], source[offset + 2]);
        }
    }

    // Original deterministic gradient-step reconstruction; no third-party shader code is copied.
    internal static void ApplyDebanding(float[] colors, int width, int height, int intensity)
    {
        if (intensity <= 0 || width < 3 || height < 3) return;
        var source = colors.ToArray();
        var strength = Math.Clamp(intensity / 100f, 0f, 1f);
        var threshold = 0.01f + 0.05f * strength;

        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = Read(x, y);
            var horizontal = Direction(Read(x - 1, y), center, Read(x + 1, y));
            var vertical = Direction(Read(x, y - 1), center, Read(x, y + 1));
            var selected = horizontal.Valid && (!vertical.Valid || horizontal.Score <= vertical.Score)
                ? horizontal
                : vertical;
            if (!selected.Valid) continue;
            var offset = (y * width + x) * 3;
            colors[offset] = Lerp(center.Red, selected.Candidate.Red, strength);
            colors[offset + 1] = Lerp(center.Green, selected.Candidate.Green, strength);
            colors[offset + 2] = Lerp(center.Blue, selected.Candidate.Blue, strength);
        }

        (bool Valid, float Score, Color Candidate) Direction(Color before, Color center, Color after)
        {
            var beforeDelta = Luminance(before) - Luminance(center);
            var afterDelta = Luminance(after) - Luminance(center);
            var score = MathF.Max(Distance(before, center), Distance(after, center));
            var hasStep = score > 0.0005f;
            var isGradient = beforeDelta * afterDelta <= 0.000001f;
            return (hasStep && isGradient && score <= threshold, score,
                Average(before, center, after));
        }

        Color Read(int x, int y)
        {
            x = Math.Clamp(x, 0, width - 1);
            y = Math.Clamp(y, 0, height - 1);
            var offset = (y * width + x) * 3;
            return new Color(source[offset], source[offset + 1], source[offset + 2]);
        }
    }

    // Original low-contrast detail recovery pass; no third-party shader code is copied.
    internal static void ApplyDetailRecovery(float[] colors, int width, int height, int intensity)
    {
        if (intensity <= 0 || width < 3 || height < 3) return;
        var source = colors.ToArray();
        var strength = Math.Clamp(intensity / 100f, 0f, 1f);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = Read(x, y);
            var sum = new Color(0f, 0f, 0f);
            var minimum = center;
            var maximum = center;
            var localContrast = 0f;
            for (var offsetY = -1; offsetY <= 1; offsetY++)
            for (var offsetX = -1; offsetX <= 1; offsetX++)
            {
                if (offsetX == 0 && offsetY == 0) continue;
                var sample = Read(x + offsetX, y + offsetY);
                sum = new Color(sum.Red + sample.Red, sum.Green + sample.Green,
                    sum.Blue + sample.Blue);
                minimum = new Color(MathF.Min(minimum.Red, sample.Red),
                    MathF.Min(minimum.Green, sample.Green), MathF.Min(minimum.Blue, sample.Blue));
                maximum = new Color(MathF.Max(maximum.Red, sample.Red),
                    MathF.Max(maximum.Green, sample.Green), MathF.Max(maximum.Blue, sample.Blue));
                localContrast = MathF.Max(localContrast, Distance(center, sample));
            }

            if (localContrast <= 0.001f) continue;
            var average = new Color(sum.Red / 8f, sum.Green / 8f, sum.Blue / 8f);
            var edgeProtection = Math.Clamp((0.35f - localContrast) / 0.30f, 0f, 1f);
            var amount = strength * edgeProtection;
            if (amount <= 0f) continue;
            var extension = localContrast * 0.25f * strength;
            var candidate = new Color(
                Math.Clamp(center.Red + (center.Red - average.Red) * amount,
                    minimum.Red - extension, maximum.Red + extension),
                Math.Clamp(center.Green + (center.Green - average.Green) * amount,
                    minimum.Green - extension, maximum.Green + extension),
                Math.Clamp(center.Blue + (center.Blue - average.Blue) * amount,
                    minimum.Blue - extension, maximum.Blue + extension));
            var offset = (y * width + x) * 3;
            colors[offset] = Math.Clamp(candidate.Red, 0f, 1f);
            colors[offset + 1] = Math.Clamp(candidate.Green, 0f, 1f);
            colors[offset + 2] = Math.Clamp(candidate.Blue, 0f, 1f);
        }

        Color Read(int x, int y)
        {
            x = Math.Clamp(x, 0, width - 1);
            y = Math.Clamp(y, 0, height - 1);
            var offset = (y * width + x) * 3;
            return new Color(source[offset], source[offset + 1], source[offset + 2]);
        }
    }

    private static bool Similar(Color first, Color second) =>
        Distance(first, second) <= SimilarityThreshold;

    private static float Distance(Color first, Color second)
    {
        var red = first.Red - second.Red;
        var green = first.Green - second.Green;
        var blue = first.Blue - second.Blue;
        return MathF.Sqrt(red * red + green * green + blue * blue);
    }

    private static float DistanceSquared(Color first, Color second)
    {
        var red = first.Red - second.Red;
        var green = first.Green - second.Green;
        var blue = first.Blue - second.Blue;
        return red * red + green * green + blue * blue;
    }

    private static Color Average(Color first, Color second) => new(
        (first.Red + second.Red) * 0.5f,
        (first.Green + second.Green) * 0.5f,
        (first.Blue + second.Blue) * 0.5f);

    private static Color Average(Color first, Color second, Color third) => new(
        (first.Red + second.Red + third.Red) / 3f,
        (first.Green + second.Green + third.Green) / 3f,
        (first.Blue + second.Blue + third.Blue) / 3f);

    private static Color Average(Color first, Color second, Color third, Color fourth) => new(
        (first.Red + second.Red + third.Red + fourth.Red) * 0.25f,
        (first.Green + second.Green + third.Green + fourth.Green) * 0.25f,
        (first.Blue + second.Blue + third.Blue + fourth.Blue) * 0.25f);

    private static float Lerp(float first, float second, float weight) =>
        first + (second - first) * weight;

    private static float Luminance(Color color) =>
        color.Red * 0.2126f + color.Green * 0.7152f + color.Blue * 0.0722f;

    private readonly record struct Color(float Red, float Green, float Blue);
}
