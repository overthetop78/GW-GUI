namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterEPaperPaperBrightness
{
    internal const string Shader="float filterEPaperPaperBrightness(float setting){return .55+clamp(setting,0.0,1.0)*.4;}";
    internal static float Apply(int setting)=>.55f+Math.Clamp(setting,0,100)/100f*.4f;
}
