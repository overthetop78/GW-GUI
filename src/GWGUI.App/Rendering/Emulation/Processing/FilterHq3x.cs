namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterHq3x
{
    internal static void Sample(float[] source,int width,int height,float x,float y,Span<float> result){FilterHqx.Sample(source,width,height,x,y,result);for(var c=0;c<3;c++)result[c]=Math.Clamp(result[c]*.82f+FilterBilinear.Sample(source,width,height,x-.5f,y-.5f,c)*.18f,0f,1f);}
    internal const string OpenGlShader="""vec4 hq3xSample(vec2 uv){return mix(hqxSample(uv),linearSample(uv),.18);}""";
    internal const string VeldridShader="""vec3 hq3xSampleCompact(vec2 uv){return mix(hqxCompactSample(uv),linearSampleCompact(uv),.18);}""";
}