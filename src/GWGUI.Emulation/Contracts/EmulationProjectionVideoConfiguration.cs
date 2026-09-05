namespace GWGUI.Emulation.Contracts;

public sealed record EmulationProjectionVideoConfiguration(
    int OpticalBlur = 20,
    int Diffusion = 15,
    int ScreenTexture = 10,
    int Convergence = 5,
    int LightOutput = 50,
    int AmbientLight = 0,
    int Vignette = 0);
