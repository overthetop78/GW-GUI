namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSepia
{
    public static void Apply(float[] colors, int intensity)
    {
        if (intensity <= 0) return;
        var amount = intensity / 100f;
        for (var index = 0; index < colors.Length; index += 3)
        {
            var red = colors[index];
            var green = colors[index + 1];
            var blue = colors[index + 2];
            var luminance = red * 0.2126f + green * 0.7152f + blue * 0.0722f;
            colors[index] = Mix(red, Math.Min(1f, luminance * 1.07f), amount);
            colors[index + 1] = Mix(green, luminance * 0.93f, amount);
            colors[index + 2] = Mix(blue, luminance * 0.74f, amount);
        }
    }

    private static float Mix(float source, float target, float amount) =>
        source + (target - source) * amount;
}
