namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterEPaperRefreshTime
{
    internal static float BlendFactor(double elapsedMilliseconds,int setting)=>setting<=0?1f:(float)(1d-Math.Exp(-Math.Max(.001,elapsedMilliseconds)/Math.Clamp(setting,0,1000)));
}
