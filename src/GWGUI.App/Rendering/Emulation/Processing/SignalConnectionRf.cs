using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class SignalConnectionRf
{
    internal const string Shader = """
        vec3 signalConnectionRf(vec3 color,vec3 left,vec3 right,float amount,float noise,float standard,float line)
        {
            vec3 blurred=(left+color*1.5+right)*.285714;
            vec3 result=mix(color,blurred,amount*.92);
            float rfNoise=noise*amount;
            float interference=sin(line*.41+rfNoise*23.0)*amount*.055;
            if(standard<1.5) result+=vec3(rfNoise*.16+interference,rfNoise*.10-interference*.25,rfNoise*.13-interference);
            else if(standard<2.5) result+=vec3(rfNoise*.18+interference,-rfNoise*.04,rfNoise*.10-interference*.55);
            else result+=vec3(rfNoise*.10+interference,rfNoise*.06-interference*.35,-rfNoise*.15-interference);
            return clamp(result,0.0,1.0);
        }
        """;

    public static void Apply(float[] colors, int width, int height, long sequence, int intensity,
        EmulationSignalStandard standard)
    {
        if (intensity <= 0) return;
        var source = colors.ToArray();
        var amount = intensity / 100f;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var center = (y * width + x) * 3;
            var left = (y * width + Math.Max(0, x - 1)) * 3;
            var right = (y * width + Math.Min(width - 1, x + 1)) * 3;
            var hash = unchecked((uint)(x * 374761393 + y * 668265263)
                ^ (uint)(sequence * 2246822519L));
            hash = (hash ^ (hash >> 13)) * 1274126177u;
            var noise = ((hash & 1023) / 511.5f - 1f) * amount;
            var interference = MathF.Sin(y * 0.41f + noise * 23f) * amount * 0.055f;
            for (var channel = 0; channel < 3; channel++)
            {
                var blurred = (source[left + channel] + source[center + channel] * 2f
                    + source[right + channel]) * 0.25f;
                var mixed = source[center + channel]
                    + (blurred - source[center + channel]) * amount * 0.92f;
                var defect = standard switch
                {
                    EmulationSignalStandard.Ntsc => channel switch
                    {
                        0 => noise * 0.18f + interference,
                        1 => -noise * 0.04f,
                        _ => noise * 0.10f - interference * 0.55f
                    },
                    EmulationSignalStandard.Secam => channel switch
                    {
                        0 => noise * 0.10f + interference,
                        1 => noise * 0.06f - interference * 0.35f,
                        _ => -noise * 0.15f - interference
                    },
                    _ => channel switch
                    {
                        0 => noise * 0.16f + interference,
                        1 => noise * 0.10f - interference * 0.25f,
                        _ => noise * 0.13f - interference
                    }
                };
                colors[center + channel] = Math.Clamp(mixed + defect, 0f, 1f);
            }
        }
    }
}
