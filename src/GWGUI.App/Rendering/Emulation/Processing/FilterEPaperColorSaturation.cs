namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterEPaperColorSaturation
{
    internal const string Shader="vec3 filterEPaperSaturation(vec3 color,float setting){float y=dot(color,vec3(.2126,.7152,.0722));return mix(vec3(y),color,clamp(setting,0.0,1.0));}";
    internal static (float Red,float Green,float Blue) Apply((float Red,float Green,float Blue) color,int setting)
    {
        var a=Math.Clamp(setting,0,100)/100f;var y=color.Red*.2126f+color.Green*.7152f+color.Blue*.0722f;
        return(y+(color.Red-y)*a,y+(color.Green-y)*a,y+(color.Blue-y)*a);
    }
}
