using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterEPaper
{
    internal static readonly string SharedShader = FilterEPaperContrast.Shader
        + FilterEPaperDithering.Shader + FilterEPaperColorMode.Shader
        + FilterEPaperColorSaturation.Shader + FilterEPaperPaperBrightness.Shader
        + FilterEPaperPaperWarmth.Shader + FilterEPaperInkDensity.Shader
        + FilterEPaperSurfaceTexture.Shader + FilterEPaperEdgeSoftness.Shader;

    internal static readonly string VeldridShader = SharedShader + """
        vec3 ePaperSource(vec2 uv,float history)
        {
            if(history>.5)return adjust(texture(sampler2D(History,LinearSampler),clamp(uv,vec2(0.0),vec2(1.0))).rgb);
            return raw(clamp(uv,vec2(0.0),vec2(1.0)));
        }
        vec3 ePaperProcessed(vec2 uv,float history)
        {
            vec2 stepSize=1.0/max(Parameters.Output.xy,vec2(1.0));
            vec3 center=ePaperSource(uv,history);
            vec3 neighbors=(ePaperSource(uv-vec2(stepSize.x,0.0),history)
                +ePaperSource(uv+vec2(stepSize.x,0.0),history)
                +ePaperSource(uv-vec2(0.0,stepSize.y),history)
                +ePaperSource(uv+vec2(0.0,stepSize.y),history))*.25;
            center=filterEPaperEdgeSoftness(center,neighbors,Parameters.EPaperTemporal.x);
            center=vec3(filterEPaperContrast(center.r,Parameters.EPaperInkAndColor.y),
                filterEPaperContrast(center.g,Parameters.EPaperInkAndColor.y),
                filterEPaperContrast(center.b,Parameters.EPaperInkAndColor.y));
            int mode=int(Parameters.EPaperInkAndColor.x+.5);
            float levels=mode==0?1.0:15.0;
            float dither=filterEPaperDither(uv*Parameters.Output.xy,
                Parameters.EPaperInkAndColor.z,levels);
            vec3 level=filterEPaperColorMode(center,mode,dither);
            if(mode==2)level=filterEPaperSaturation(level,Parameters.EPaperInkAndColor.w);
            float brightness=filterEPaperPaperBrightness(Parameters.EPaperSurface.y);
            vec3 paper=filterEPaperPaperWarmth(brightness,Parameters.EPaperSurface.z);
            vec3 color=filterEPaperInkDensity(level,paper,Parameters.EPaperSurface.x);
            return filterEPaperSurfaceTexture(color,
                filterEPaperTextureNoise(floor(uv*Parameters.Output.xy)),Parameters.EPaperSurface.w);
        }
        vec3 ePaperPixel(vec2 uv)
        {
            vec3 current=ePaperProcessed(uv,0.0);
            if(Parameters.EPaperTemporal.w<.5)return current;
            vec3 previous=ePaperProcessed(uv,1.0);
            return mix(previous,current,Parameters.EPaperTemporal.y*Parameters.EPaperTemporal.z);
        }
        """;

    internal static readonly string OpenGlShader = SharedShader + """
        vec3 ePaperSource(vec2 uv,float history)
        {
            if(history>.5)return adjustColor(texture2D(History,clamp(uv,vec2(0.0),vec2(1.0))).rgb);
            return adjustColor(sampleConfigured(clamp(uv,vec2(0.0),vec2(1.0))).rgb);
        }
        vec3 ePaperProcessed(vec2 uv,float history)
        {
            vec2 stepSize=1.0/max(Output.xy,vec2(1.0));
            vec3 center=ePaperSource(uv,history);
            vec3 neighbors=(ePaperSource(uv-vec2(stepSize.x,0.0),history)
                +ePaperSource(uv+vec2(stepSize.x,0.0),history)
                +ePaperSource(uv-vec2(0.0,stepSize.y),history)
                +ePaperSource(uv+vec2(0.0,stepSize.y),history))*.25;
            center=filterEPaperEdgeSoftness(center,neighbors,EPaperTemporal.x);
            center=vec3(filterEPaperContrast(center.r,EPaperInkAndColor.y),
                filterEPaperContrast(center.g,EPaperInkAndColor.y),
                filterEPaperContrast(center.b,EPaperInkAndColor.y));
            int mode=int(EPaperInkAndColor.x+.5);float levels=mode==0?1.0:15.0;
            float dither=filterEPaperDither(uv*Output.xy,EPaperInkAndColor.z,levels);
            vec3 level=filterEPaperColorMode(center,mode,dither);
            if(mode==2)level=filterEPaperSaturation(level,EPaperInkAndColor.w);
            float brightness=filterEPaperPaperBrightness(EPaperSurface.y);
            vec3 paper=filterEPaperPaperWarmth(brightness,EPaperSurface.z);
            vec3 color=filterEPaperInkDensity(level,paper,EPaperSurface.x);
            return filterEPaperSurfaceTexture(color,
                filterEPaperTextureNoise(floor(uv*Output.xy)),EPaperSurface.w);
        }
        vec3 ePaperPixel(vec2 uv)
        {
            vec3 current=ePaperProcessed(uv,0.0);
            if(EPaperTemporal.w<.5)return current;
            vec3 previous=ePaperProcessed(uv,1.0);
            return mix(previous,current,EPaperTemporal.y*EPaperTemporal.z);
        }
        """;

    internal static void Apply(float[] colors, int width, int height,
        EmulationEPaperVideoConfiguration configuration)
    {
        FilterEPaperEdgeSoftness.Apply(colors, width, height, configuration.EdgeSoftness);
        var paperBrightness = FilterEPaperPaperBrightness.Apply(configuration.PaperBrightness);
        var paper = FilterEPaperPaperWarmth.Apply(paperBrightness, configuration.PaperWarmth);
        var levels = configuration.ColorMode == EmulationEPaperColorMode.Monochrome ? 1 : 15;
        for (var y = 0; y < height; y++)
        for (var x = 0; x < width; x++)
        {
            var index = (y * width + x) * 3;
            var dither = FilterEPaperDithering.Offset(x, y, configuration.Dithering, levels);
            var level = FilterEPaperColorMode.Apply(
                FilterEPaperContrast.Apply(colors[index], configuration.Contrast),
                FilterEPaperContrast.Apply(colors[index + 1], configuration.Contrast),
                FilterEPaperContrast.Apply(colors[index + 2], configuration.Contrast),
                configuration.ColorMode, dither);
            if (configuration.ColorMode == EmulationEPaperColorMode.Color4096)
                level = FilterEPaperColorSaturation.Apply(level, configuration.ColorSaturation);
            var pigment = FilterEPaperInkDensity.Apply(level, paper, configuration.InkDensity);
            colors[index] = FilterEPaperSurfaceTexture.Apply(pigment.Red, x, y,
                configuration.SurfaceTexture);
            colors[index + 1] = FilterEPaperSurfaceTexture.Apply(pigment.Green, x, y,
                configuration.SurfaceTexture);
            colors[index + 2] = FilterEPaperSurfaceTexture.Apply(pigment.Blue, x, y,
                configuration.SurfaceTexture);
        }
    }
}
