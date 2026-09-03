namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterSuperTwoXSai
{
    internal static void Sample(float[] source,int width,int height,float x,float y,Span<float> result)=>FilterTwoXSai.SampleCore(source,width,height,x,y,result,.62f,1);
    internal const string OpenGlShader="""vec4 superTwoXSaiSample(vec2 uv){vec4 a=twoXSaiSample(uv);return mix(a,linearSample(uv),.18);}""";
    internal const string VeldridShader="""vec3 superTwoXSaiSampleCompact(vec2 uv){return mix(twoXSaiSampleCompact(uv),linearSampleCompact(uv),.18);}""";
}