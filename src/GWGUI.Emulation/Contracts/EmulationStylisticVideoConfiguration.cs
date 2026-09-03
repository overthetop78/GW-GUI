namespace GWGUI.Emulation.Contracts;

public sealed record EmulationStylisticVideoConfiguration(
    int Grain = 0,
    int Vhs = 0,
    int ChromaticAberration = 0,
    int Bloom = 0,
    bool Sepia = false);
