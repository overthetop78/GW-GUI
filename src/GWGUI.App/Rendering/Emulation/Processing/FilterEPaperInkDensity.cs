namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterEPaperInkDensity
{
    internal const string Shader="vec3 filterEPaperInkDensity(vec3 level,vec3 paper,float setting){float d=clamp(setting,0.0,1.0);vec3 dark=mix(paper*.35,vec3(.015,.018,.015),d);return mix(dark,paper,level);}";
    internal static (float Red,float Green,float Blue) Apply((float Red,float Green,float Blue) level,(float Red,float Green,float Blue) paper,int setting)
    {var d=Math.Clamp(setting,0,100)/100f;float L(float a,float b,float t)=>a+(b-a)*t;var dark=(L(paper.Red*.35f,.015f,d),L(paper.Green*.35f,.018f,d),L(paper.Blue*.35f,.015f,d));return(L(dark.Item1,paper.Red,level.Red),L(dark.Item2,paper.Green,level.Green),L(dark.Item3,paper.Blue,level.Blue));}
}
