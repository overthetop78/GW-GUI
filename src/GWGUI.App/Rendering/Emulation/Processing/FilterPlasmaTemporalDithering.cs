namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterPlasmaTemporalDithering
{
    private static readonly int[] Bayer4X4 =
    [
        0, 8, 2, 10,
        12, 4, 14, 6,
        3, 11, 1, 9,
        15, 7, 13, 5
    ];

    internal const string Shader = """
        float plasmaBayer(vec2 pixel,float phase)
        {
            int x=int(mod(pixel.x+phase,4.0)),y=int(mod(pixel.y+phase,4.0));
            if(y==0)return x==0?0.0:(x==1?8.0:(x==2?2.0:10.0));
            if(y==1)return x==0?12.0:(x==1?4.0:(x==2?14.0:6.0));
            if(y==2)return x==0?3.0:(x==1?11.0:(x==2?1.0:9.0));
            return x==0?15.0:(x==1?7.0:(x==2?13.0:5.0));
        }
        vec3 filterPlasmaTemporalDithering(vec3 color,vec2 pixel,float intensity,float phase)
        {
            if(intensity<=0.0)return color;
            float levels=mix(255.0,31.0,intensity);
            float threshold=(plasmaBayer(pixel,phase)+.5)/16.0;
            vec3 quantized=floor(color*levels+threshold)/levels;
            return clamp(mix(color,quantized,intensity),0.0,1.0);
        }
        """;

    internal static void Apply(float[] colors, int width, int height,
        long sequence, int setting)
    {
        if (setting <= 0) return;
        var intensity = setting / 100f;
        var levels = 255f + (31f - 255f) * intensity;
        var phase = (int)(Math.Abs(sequence) % 4);
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var threshold = (Bayer4X4[((y + phase) & 3) * 4 + ((x + phase) & 3)] + 0.5f) / 16f;
            var index = (y * width + x) * 3;
            for (var channel = 0; channel < 3; channel++)
            {
                var original = colors[index + channel];
                var quantized = MathF.Floor(original * levels + threshold) / levels;
                colors[index + channel] = Math.Clamp(
                    original + (quantized - original) * intensity, 0f, 1f);
            }
        }
    }
}
