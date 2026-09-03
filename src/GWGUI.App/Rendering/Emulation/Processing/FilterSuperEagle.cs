namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterSuperEagle
{
    internal static void Sample(float[] source,int width,int height,float x,float y,Span<float> result)=>FilterTwoXSai.SampleCore(source,width,height,x,y,result,.78f,2);
    internal const string OpenGlShader="""vec4 superEagleSample(vec2 uv){vec4 a=twoXSaiSample(uv),b=linearSample(uv);return vec4(clamp(a.rgb+(a.rgb-b.rgb)*.22,0.0,1.0),1.0);}""";
    internal const string VeldridShader="""vec3 superEagleSampleCompact(vec2 uv){vec3 a=twoXSaiSampleCompact(uv),b=linearSampleCompact(uv);return clamp(a+(a-b)*.22,0.0,1.0);}""";
}