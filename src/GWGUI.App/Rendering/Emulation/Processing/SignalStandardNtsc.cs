namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class SignalStandardNtsc
{
    internal const string Shader = """
        vec3 signalStandardNtsc(vec3 color,vec3 left,float amount)
        {
            vec3 mixed=signalConnectionComponent(color,left,left,min(1.0,amount*3.0));
            float y=dot(mixed,vec3(.299,.587,.114));
            vec2 c=vec2(dot(mixed,vec3(.596,-.274,-.322)),dot(mixed,vec3(.211,-.523,.312)));
            float error=amount*.12; c=vec2(c.x-c.y*error,c.y+c.x*error);
            return clamp(vec3(y+.956*c.x+.621*c.y,y-.272*c.x-.647*c.y,y-1.106*c.x+1.703*c.y),0.0,1.0);
        }
        """;

    public static void Apply(float[] colors, int width, int height, int intensity)
    {
        if (intensity <= 0 || width < 2) return;
        var source = colors.ToArray();
        var amount = intensity / 100f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = (y * width + x) * 3;
            var delayed = (y * width + Math.Max(0, x - 1)) * 3;
            Components(source, center, out var luminance, out var inPhase, out var quadrature);
            Components(source, delayed, out var delayedY, out var delayedI, out var delayedQ);
            luminance += (delayedY - luminance) * amount * 0.24f;
            inPhase += (delayedI - inPhase) * amount * 0.72f;
            quadrature += (delayedQ - quadrature) * amount * 0.84f;
            var hueError = amount * 0.12f;
            var rotatedI = inPhase - quadrature * hueError;
            quadrature += inPhase * hueError;
            inPhase = rotatedI;
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
