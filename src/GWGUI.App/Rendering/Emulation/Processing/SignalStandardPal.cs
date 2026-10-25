namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class SignalStandardPal
{
    internal const string Shader = """
        vec3 signalStandardPal(vec3 color,vec3 vertical,float amount)
        {
            float y=dot(color,vec3(.299,.587,.114));
            vec2 c=vec2(dot(color,vec3(.596,-.274,-.322)),dot(color,vec3(.211,-.523,.312)));
            vec2 n=vec2(dot(vertical,vec3(.596,-.274,-.322)),dot(vertical,vec3(.211,-.523,.312)));
            c=mix(c,n,amount*vec2(.58,.72));
            return clamp(vec3(y+.956*c.x+.621*c.y,y-.272*c.x-.647*c.y,y-1.106*c.x+1.703*c.y),0.0,1.0);
        }
        """;

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
            SignalStandardNtsc.Components(source, center, out var luminance, out var inPhase, out var quadrature);
            SignalStandardNtsc.Components(source, neighbor, out _, out var neighborI, out var neighborQ);
            inPhase += (neighborI - inPhase) * amount * 0.58f;
            quadrature += (neighborQ - quadrature) * amount * 0.72f;
            colors[center] = Math.Clamp(luminance + 0.956f * inPhase + 0.621f * quadrature, 0f, 1f);
            colors[center + 1] = Math.Clamp(luminance - 0.272f * inPhase - 0.647f * quadrature, 0f, 1f);
            colors[center + 2] = Math.Clamp(luminance - 1.106f * inPhase + 1.703f * quadrature, 0f, 1f);
        }
    }


}
