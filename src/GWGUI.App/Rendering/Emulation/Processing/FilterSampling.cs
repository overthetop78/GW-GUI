namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSampling
{
    internal static float Read(float[] source, int width, int height, int x, int y, int channel)
    {
        x = Math.Clamp(x, 0, width - 1);
        y = Math.Clamp(y, 0, height - 1);
        return source[(y * width + x) * 3 + channel];
    }

    internal static FilterColor ReadColor(float[] source, int width, int height, int x, int y)
    {
        x = Math.Clamp(x, 0, width - 1);
        y = Math.Clamp(y, 0, height - 1);
        var offset = (y * width + x) * 3;
        return new FilterColor(source[offset], source[offset + 1], source[offset + 2]);
    }

    internal static bool Similar(FilterColor first, FilterColor second) =>
        MathF.Abs(first.Red - second.Red) * .299f
        + MathF.Abs(first.Green - second.Green) * .587f
        + MathF.Abs(first.Blue - second.Blue) * .114f < .035f;

    internal static float Sinc(float value) => MathF.Abs(value) < .0001f
        ? 1f : MathF.Sin(MathF.PI * value) / (MathF.PI * value);

    internal static float BesselJ1(float value)
    {
        var half = value * .5f;
        var term = half;
        var sum = term;
        for (var index = 1; index < 12; index++)
        {
            term *= -(half * half) / (index * (index + 1f));
            sum += term;
        }
        return sum;
    }

    internal static float Jinc(float value) => MathF.Abs(value) < .0001f
        ? .5f : BesselJ1(value) / value;
}