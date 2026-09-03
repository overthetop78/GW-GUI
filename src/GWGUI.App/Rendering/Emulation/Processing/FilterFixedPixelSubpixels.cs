using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterFixedPixelSubpixels
{
    internal const string Shader = """
        vec3 filterFixedPixelSubpixels(vec3 color,vec2 fraction,float subpixelLayout,vec3 tint,float intensity,float pixelScale)
        {
            int mode=int(subpixelLayout+.5);
            if(mode==0)return dot(color,vec3(.2126,.7152,.0722))*tint;
            float strength=intensity*smoothstep(2.25,3.0,pixelScale);
            if(strength<=0.0)return color;
            int selected=int(floor(min(2.0,fraction.x*3.0)));
            if(mode==2)selected=2-selected;
            for(int channel=0;channel<3;channel++)if(channel!=selected)color[channel]*=1.0-.42*strength;
            return color;
        }
        """;

    private const float RedLuminance = 0.2126f;
    private const float GreenLuminance = 0.7152f;
    private const float BlueLuminance = 0.0722f;

    internal static void Apply(float[] colors, int sourceWidth, int outputWidth, int outputHeight,
        EmulationSubpixelLayout layout, EmulationMonochromePalette palette, int intensitySetting)
    {
        if (layout == EmulationSubpixelLayout.Monochrome)
        {
            var tint = Tint(palette);
            for (var index = 0; index < colors.Length; index += 3)
            {
                var luminance = colors[index] * RedLuminance
                    + colors[index + 1] * GreenLuminance + colors[index + 2] * BlueLuminance;
                colors[index] = luminance * tint.Red;
                colors[index + 1] = luminance * tint.Green;
                colors[index + 2] = luminance * tint.Blue;
            }
            return;
        }

        var outputScale = outputWidth / (float)sourceWidth;
        var strength = intensitySetting / 100f * SmoothStep(2.25f, 3f, outputScale);
        if (strength <= 0f) return;
        var scaleX = sourceWidth / (float)outputWidth;
        var attenuation = 0.42f * strength;
        for (var y = 0; y < outputHeight; y++)
        for (var x = 0; x < outputWidth; x++)
        {
            var sourceX = (x + 0.5f) * scaleX;
            var selected = Math.Min(2, (int)((sourceX - MathF.Floor(sourceX)) * 3f));
            if (layout == EmulationSubpixelLayout.Bgr) selected = 2 - selected;
            var index = (y * outputWidth + x) * 3;
            for (var channel = 0; channel < 3; channel++)
                if (channel != selected) colors[index + channel] *= 1f - attenuation;
        }
    }

    private static float SmoothStep(float start, float end, float value)
    {
        var amount = Math.Clamp((value - start) / (end - start), 0f, 1f);
        return amount * amount * (3f - 2f * amount);
    }

    internal static (float Red, float Green, float Blue) Tint(EmulationMonochromePalette palette)
    {
        var srgb = palette switch
        {
            EmulationMonochromePalette.Gray => (0.78f, 0.80f, 0.77f),
            EmulationMonochromePalette.Amber => (1f, 0.46f, 0.035f),
            EmulationMonochromePalette.Blue => (0.42f, 0.74f, 1f),
            EmulationMonochromePalette.White => (1f, 1f, 1f),
            _ => (0.56f, 0.78f, 0.32f)
        };
        return (Linear(srgb.Item1), Linear(srgb.Item2), Linear(srgb.Item3));
    }

    private static float Linear(float value) =>
        SoftwareEmulationVideoProcessingPipeline.SrgbToLinear(value);
}
