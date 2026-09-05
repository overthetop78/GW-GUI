namespace GWGUI.VideoPresentation.Contracts;

public sealed record EmulationTemporalVideoConfiguration(
    int GeneralPersistence = 0,
    int MotionBlur = 0,
    int Flicker = 0,
    int Interlacing = 0,
    int InterlacingVisibility = 50,
    bool BlackFrameInsertion = false);
