using GWGUI.Emulation.Enums;
namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterEPaperColorMode
{
    internal const string Shader = """
        vec3 filterEPaperColorMode(vec3 color,int mode,float dither)
        {
            float y=dot(color,vec3(.2126,.7152,.0722));
            if(mode==0){float level=y+dither>=.5?1.0:0.0;return vec3(level);}
            if(mode==1){float level=floor(clamp(y+dither,0.0,1.0)*15.0+.5)/15.0;return vec3(level);}
            return floor(clamp(color+vec3(dither),vec3(0.0),vec3(1.0))*15.0+vec3(.5))/15.0;
        }
        """;
    internal static (float Red,float Green,float Blue) Apply(float red,float green,float blue,EmulationEPaperColorMode mode,float dither)
    {
        var y=red*.2126f+green*.7152f+blue*.0722f;
        if(mode==EmulationEPaperColorMode.Monochrome){var v=y+dither>=.5f?1f:0f;return(v,v,v);}
        if(mode==EmulationEPaperColorMode.Grayscale16){var v=Quantize(y+dither);return(v,v,v);}
        return(Quantize(red+dither),Quantize(green+dither),Quantize(blue+dither));
    }
    private static float Quantize(float value)=>MathF.Round(Math.Clamp(value,0f,1f)*15f)/15f;
}
