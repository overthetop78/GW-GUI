namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterEPaperGhosting
{
    internal static float BlendFactor(int setting)=>1f-Math.Clamp(setting,0,100)/100f*.4f;
}
