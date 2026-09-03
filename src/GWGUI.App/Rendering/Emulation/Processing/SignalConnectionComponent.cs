namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class SignalConnectionComponent
{
    internal const string Shader = """
        vec3 signalConnectionComponent(vec3 color,vec3 left,vec3 right,float amount)
        {
            float y=dot(color,vec3(.299,.587,.114));
            vec2 c=vec2(dot(color,vec3(.596,-.274,-.322)),dot(color,vec3(.211,-.523,.312)));
            vec2 lc=vec2(dot(left,vec3(.596,-.274,-.322)),dot(left,vec3(.211,-.523,.312)));
            vec2 rc=vec2(dot(right,vec3(.596,-.274,-.322)),dot(right,vec3(.211,-.523,.312)));
            c=mix(c,(lc+rc)*.5,amount*.34);
            return clamp(vec3(y+.956*c.x+.621*c.y,y-.272*c.x-.647*c.y,y-1.106*c.x+1.703*c.y),0.0,1.0);
        }
        """;

    internal static void Apply(float[] colors, int width, int height, int intensity)
    {
        if (intensity <= 0 || width < 2) return;
        var source = colors.ToArray();
        var amount = intensity / 100f * 0.34f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = (y * width + x) * 3;
            var left = (y * width + Math.Max(0, x - 1)) * 3;
            var right = (y * width + Math.Min(width - 1, x + 1)) * 3;
            ApplyChromaBlur(colors, source, center, left, right, amount);
        }
    }

    internal static void ApplyChromaBlur(float[] target, float[] source, int center, int left,
        int right, float amount)
    {
        Components(source, center, out var luminance, out var inPhase, out var quadrature);
        Components(source, left, out _, out var leftI, out var leftQ);
        Components(source, right, out _, out var rightI, out var rightQ);
        inPhase += ((leftI + rightI) * 0.5f - inPhase) * amount;
        quadrature += ((leftQ + rightQ) * 0.5f - quadrature) * amount;
        target[center] = Math.Clamp(luminance + 0.956f * inPhase + 0.621f * quadrature, 0f, 1f);
        target[center + 1] = Math.Clamp(luminance - 0.272f * inPhase - 0.647f * quadrature, 0f, 1f);
        target[center + 2] = Math.Clamp(luminance - 1.106f * inPhase + 1.703f * quadrature, 0f, 1f);
    }

    private static void Components(float[] source, int index, out float luminance,
        out float inPhase, out float quadrature)
    {
        var red = source[index]; var green = source[index + 1]; var blue = source[index + 2];
        luminance = red * 0.299f + green * 0.587f + blue * 0.114f;
        inPhase = red * 0.596f - green * 0.274f - blue * 0.322f;
        quadrature = red * 0.211f - green * 0.523f + blue * 0.312f;
    }
}
