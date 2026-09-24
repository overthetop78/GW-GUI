using GWGUI.App.Constants.Rendering.Emulation;
using GWGUI.App.Rendering.Emulation.Processing.OpenGl;
using GWGUI.App.Rendering.Emulation.Processing.OpenGl.Shaders;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed class OpenGlVideoProcessingProgram : IDisposable
{
    private readonly OpenGlNativeApi _api = new();
    private readonly uint _program;
    private readonly OpenGlVideoProgramUniforms _uniforms;
    private bool _disposed;

    internal OpenGlVideoProcessingProgram(EmulationVideoSampling sampling,
        EmulationVideoDisplayTechnology displayTechnology)
    {
        _program = _api.CreateProgram(
            OpenGlVideoShaderSource.VertexSource,
            OpenGlVideoShaderSource.Fragment(sampling, displayTechnology));
        _uniforms = new(_api, _program);
        _api.UseProgram(_program);
        _api.SetUniform(
            _api.GetUniformLocation(_program, OpenGlVideoUniformNames.Source),
            OpenGlVideoConstants.SourceTextureUnit);
        _api.SetUniform(
            _api.GetUniformLocation(_program, OpenGlVideoUniformNames.History),
            OpenGlVideoConstants.HistoryTextureUnit);
        _api.UseProgram(0);
    }

    internal void Use(EmulationVideoProcessingConfiguration configuration,
        int sourceWidth, int sourceHeight, int outputWidth, int outputHeight,
        bool hasHistory = false, double elapsedMilliseconds = 0, long sequence = 0,
        float averageLuminance = 0f)
    {
        _api.UseProgram(_program);
        _uniforms.Apply(configuration, sourceWidth, sourceHeight, outputWidth, outputHeight,
            hasHistory, elapsedMilliseconds, sequence, averageLuminance);
    }

    internal void Stop() => _api.UseProgram(0);

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _api.DeleteProgram(_program);
    }
}
