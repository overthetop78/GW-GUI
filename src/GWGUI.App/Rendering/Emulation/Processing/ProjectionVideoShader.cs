namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class ProjectionVideoShader
{
    private static readonly string Dependencies = FilterProjectionOpticalBlur.Shader
        + FilterProjectionDiffusion.Shader + FilterProjectionConvergence.Shader
        + FilterProjectionLightOutput.Shader + FilterProjectionAmbientLight.Shader
        + FilterProjectionVignette.Shader + FilterProjectionScreenTexture.Shader;

    internal static string Create(bool veldrid)
    {
        var body = """
            vec3 projectionPixel(vec2 uv)
            {
                vec2 stepSize=1.0/max(OUT_SIZE,vec2(1.0));
                vec3 average=vec3(0.0);
                for(int y=-2;y<=2;y++)for(int x=-2;x<=2;x++)
                    average+=SAMPLE(clamp(uv+vec2(float(x),float(y))*stepSize,vec2(0.0),vec2(1.0)));
                average/=25.0;
                float shift=projectionConvergence(OPTICS.w);
                float nearShift=floor(shift)*stepSize.x;
                float farShift=(floor(shift)+1.0)*stepSize.x;
                vec3 center=SAMPLE(uv);
                center.r=mix(
                    SAMPLE(clamp(uv-vec2(nearShift,0.0),vec2(0.0),vec2(1.0))).r,
                    SAMPLE(clamp(uv-vec2(farShift,0.0),vec2(0.0),vec2(1.0))).r,fract(shift));
                center.b=mix(
                    SAMPLE(clamp(uv+vec2(nearShift,0.0),vec2(0.0),vec2(1.0))).b,
                    SAMPLE(clamp(uv+vec2(farShift,0.0),vec2(0.0),vec2(1.0))).b,fract(shift));
                vec3 color=vec3(0.0);
                for(int channel=0;channel<3;channel++)
                {
                    float value=projectionOpticalBlur(center[channel],average[channel],OPTICS.x);
                    value=projectionDiffusion(value,average[channel],OPTICS.y);
                    value=projectionLightOutput(value,SCREEN.x);
                    value=projectionVignette(value,uv,SCREEN.z);
                    value=projectionAmbientLight(value,SCREEN.y);
                    color[channel]=projectionScreenTexture(value,uv*OUT_SIZE,OPTICS.z);
                }
                return clamp(color,vec3(0.0),vec3(1.0));
            }
            """;
        return Dependencies + body.Replace("OUT_SIZE", veldrid ? "Parameters.Output.xy" : "Output.xy")
            .Replace("OPTICS", veldrid ? "Parameters.Projection" : "Projection")
            .Replace("SCREEN", veldrid ? "Parameters.ProjectionScreen" : "ProjectionScreen")
            .Replace("SAMPLE", veldrid ? "raw" : "extraRaw");
    }
}
