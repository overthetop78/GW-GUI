namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterVhs
{
    internal const string Shader = """
        vec3 filterVhs(vec3 color,vec3 shifted,vec3 left,vec3 right,float amount,
            float noise,float line,float vertical)
        {
            float y=dot(shifted,vec3(.299,.587,.114));
            float ly=dot(left,vec3(.299,.587,.114)),ry=dot(right,vec3(.299,.587,.114));
            y=mix(y,(ly+y*2.0+ry)*.25,amount*.72);
            vec3 chroma=shifted-vec3(dot(shifted,vec3(.299,.587,.114)));
            vec3 delayed=left-vec3(ly);
            chroma=mix(chroma,delayed,amount*.68)*(1.0-amount*.28);
            float head=smoothstep(.88,.99,vertical)*sin(line*.73+noise*18.0)*amount*.16;
            float dropout=step(.965,noise+.5)*amount*.42;
            return clamp((vec3(y)+chroma)*(1.0-dropout)+vec3(head),0.0,1.0);
        }
        """;

    public static void Apply(float[] colors, int width, int height, long sequence, int intensity)
    {
        if (intensity <= 0 || width < 2) return;
        var source = colors.ToArray();
        var amount = intensity / 100f;
        for (var y = 0; y < height; y++)
        {
            var hash = unchecked((uint)(y * 1103515245L + sequence * 12345L));
            hash ^= hash >> 15;
            var wobble = MathF.Sin(y * 0.071f + sequence * 0.31f) * amount * 4f;
            var jitter = (((hash & 15) / 7.5f) - 1f) * amount * 2f;
            var shift = (int)MathF.Round(wobble + jitter);
            var dropout = (hash & 127) > 122 ? amount * 0.42f : 0f;
            var headSwitching = y > height * 0.88f
                ? MathF.Sin(y * 0.73f + sequence * 0.43f) * amount * 0.16f : 0f;
            for (var x = 0; x < width; x++)
            {
                var output = (y * width + x) * 3;
                var centerX = Math.Clamp(x + shift, 0, width - 1);
                var center = (y * width + centerX) * 3;
                var left = (y * width + Math.Max(0, centerX - 2)) * 3;
                var right = (y * width + Math.Min(width - 1, centerX + 2)) * 3;
                var centerY = Luminance(source, center);
                var blurredY = (Luminance(source, left) + centerY * 2f
                    + Luminance(source, right)) * 0.25f;
                var luminance = Lerp(centerY, blurredY, amount * 0.72f);
                for (var channel = 0; channel < 3; channel++)
                {
                    var chroma = source[center + channel] - centerY;
                    var delayedChroma = source[left + channel] - Luminance(source, left);
                    chroma = Lerp(chroma, delayedChroma, amount * 0.68f)
                        * (1f - amount * 0.28f);
                    colors[output + channel] = Math.Clamp((luminance + chroma)
                        * (1f - dropout) + headSwitching, 0f, 1f);
                }
            }
        }
    }

    private static float Lerp(float from, float to, float amount) => from + (to - from) * amount;

    private static float Luminance(float[] source, int index) => source[index] * 0.299f
        + source[index + 1] * 0.587f + source[index + 2] * 0.114f;
}
