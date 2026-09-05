namespace GWGUI.App.Rendering.Emulation.Processing;
internal static class FilterEPaperSurfaceTexture
{
    internal const string Shader="float filterEPaperTextureNoise(vec2 p){return fract(sin(dot(p,vec2(12.9898,78.233)))*43758.5453);}vec3 filterEPaperSurfaceTexture(vec3 color,float noise,float setting){return clamp(color+vec3((noise-.5)*clamp(setting,0.0,1.0)*.055),vec3(0.0),vec3(1.0));}";
    internal static float Apply(float value,int x,int y,int setting)
    {var v=unchecked((uint)(x*374761393+y*668265263));v=(v^(v>>13))*1274126177u;var hash=(v^(v>>16))/(float)uint.MaxValue;return Math.Clamp(value+(hash-.5f)*Math.Clamp(setting,0,100)/100f*.055f,0f,1f);}
}
