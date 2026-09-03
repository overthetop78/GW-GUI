namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterPal
{
    public static void Apply(float[] colors, int width, int height, int intensity)
    {
        if (intensity <= 0 || height < 2) return;
        var source = colors.ToArray();
        var amount = intensity / 100f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = (y * width + x) * 3;
            var neighborY = y + (((y & 1) == 0) ? 1 : -1);
            neighborY = Math.Clamp(neighborY, 0, height - 1);
            var neighbor = (neighborY * width + x) * 3;
            Components(source, center, out var luminance, out var inPhase, out var quadrature);
            Components(source, neighbor, out _, out var neighborI, out var neighborQ);
            inPhase += (neighborI - inPhase) * amount * 0.28f;
            quadrature += (neighborQ - quadrature) * amount * 0.36f;
            quadrature += ((y & 1) == 0 ? 1f : -1f) * amount * 0.025f;
            colors[center] = Math.Clamp(luminance + 0.956f * inPhase + 0.621f * quadrature, 0f, 1f);
            colors[center + 1] = Math.Clamp(luminance - 0.272f * inPhase - 0.647f * quadrature, 0f, 1f);
            colors[center + 2] = Math.Clamp(luminance - 1.106f * inPhase + 1.703f * quadrature, 0f, 1f);
        }
    }

    private static void Components(float[] source, int index, out float luminance,
        out float inPhase, out float quadrature)
    {
        var red = source[index];
        var green = source[index + 1];
        var blue = source[index + 2];
        luminance = red * 0.299f + green * 0.587f + blue * 0.114f;
        inPhase = red * 0.596f - green * 0.274f - blue * 0.322f;
        quadrature = red * 0.211f - green * 0.523f + blue * 0.312f;
    }
}
