namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterHq2x
{
    internal static void Sample(float[] source,int width,int height,float x,float y,Span<float> result){FilterHqx.Sample(source,width,height,x,y,result);for(var c=0;c<3;c++)result[c]=Math.Clamp(result[c]*.94f+FilterBilinear.Sample(source,width,height,x-.5f,y-.5f,c)*.06f,0f,1f);}
    internal const string OpenGlShader="""vec4 hq2xSample(vec2 uv){return mix(hqxSample(uv),linearSample(uv),.06);}""";
    internal const string VeldridShader="""vec3 hq2xSampleCompact(vec2 uv){return mix(hqxCompactSample(uv),linearSampleCompact(uv),.06);}""";
}