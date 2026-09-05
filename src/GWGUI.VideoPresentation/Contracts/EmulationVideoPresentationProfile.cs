namespace GWGUI.VideoPresentation.Contracts;

public sealed record EmulationVideoPresentationProfile(
    EmulationVideoRenderer Renderer = EmulationVideoRenderer.Direct3D11,
    EmulationVideoProcessingConfiguration? Processing = null)
{
    public EmulationVideoPresentationProfile Normalize() => new(
        Enum.IsDefined(Renderer) ? Renderer : EmulationVideoRenderer.Direct3D11,
        EmulationVideoProcessingConfigurationFunctions.Normalize(Processing));
}
