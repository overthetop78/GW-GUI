namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterEPaperEdgeSoftness
{
    internal const string Shader="vec3 filterEPaperEdgeSoftness(vec3 center,vec3 neighbors,float setting){return mix(center,neighbors,clamp(setting,0.0,1.0)*.65);}";
    internal static void Apply(float[] colors,int width,int height,int setting)
    {if(setting<=0||width<2||height<2)return;var source=colors.ToArray();var amount=Math.Clamp(setting,0,100)/100f*.65f;for(var y=0;y<height;y++)for(var x=0;x<width;x++)for(var c=0;c<3;c++){var center=source[(y*width+x)*3+c];var n=(source[(y*width+Math.Max(0,x-1))*3+c]+source[(y*width+Math.Min(width-1,x+1))*3+c]+source[(Math.Max(0,y-1)*width+x)*3+c]+source[(Math.Min(height-1,y+1)*width+x)*3+c])*.25f;colors[(y*width+x)*3+c]=center+(n-center)*amount;}}
}
