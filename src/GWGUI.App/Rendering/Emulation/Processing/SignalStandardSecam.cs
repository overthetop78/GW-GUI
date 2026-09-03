namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class SignalStandardSecam
{
    internal const string Shader = """
        vec3 signalStandardSecam(vec3 color,vec3 previousLine,float amount,float line)
        {
            float y=dot(color,vec3(.299,.587,.114));
            float r=color.r-y,b=color.b-y,pr=previousLine.r-dot(previousLine,vec3(.299,.587,.114));
            float pb=previousLine.b-dot(previousLine,vec3(.299,.587,.114));
            if(mod(line,2.0)<.5)r=mix(r,pr,amount*.88);else b=mix(b,pb,amount*.88);
            float red=clamp(y+r,0.0,1.0),blue=clamp(y+b,0.0,1.0);
            return vec3(red,clamp((y-.299*red-.114*blue)/.587,0.0,1.0),blue);
        }
        """;

    internal static void Apply(float[] colors, int width, int height, int intensity)
    {
        if (intensity <= 0 || height < 2) return;
        var source = colors.ToArray();
        var amount = intensity / 100f * 0.88f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = (y * width + x) * 3;
            var previous = (Math.Max(0, y - 1) * width + x) * 3;
            Components(source, center, out var luminance, out var redDifference,
                out var blueDifference);
            Components(source, previous, out _, out var previousRed, out var previousBlue);
            if ((y & 1) == 0) redDifference += (previousRed - redDifference) * amount;
            else blueDifference += (previousBlue - blueDifference) * amount;
            colors[center] = Math.Clamp(luminance + redDifference, 0f, 1f);
            colors[center + 2] = Math.Clamp(luminance + blueDifference, 0f, 1f);
            colors[center + 1] = Math.Clamp((luminance - 0.299f * colors[center]
                - 0.114f * colors[center + 2]) / 0.587f, 0f, 1f);
        }
    }

    private static void Components(float[] source, int index, out float luminance,
        out float redDifference, out float blueDifference)
    {
        var red = source[index]; var green = source[index + 1]; var blue = source[index + 2];
        luminance = red * 0.299f + green * 0.587f + blue * 0.114f;
        redDifference = red - luminance;
        blueDifference = blue - luminance;
    }
}
