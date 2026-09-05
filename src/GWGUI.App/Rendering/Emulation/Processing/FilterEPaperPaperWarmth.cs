namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterEPaperPaperWarmth
{
    internal const string Shader="vec3 filterEPaperPaperWarmth(float brightness,float warmth){float w=clamp(warmth,0.0,1.0);return brightness*vec3(1.0,1.0-w*.05,1.0-w*.16);}";
    internal static (float Red,float Green,float Blue) Apply(float brightness,int setting)
    {var w=Math.Clamp(setting,0,100)/100f;return(brightness,brightness*(1f-w*.05f),brightness*(1f-w*.16f));}
}
