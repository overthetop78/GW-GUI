namespace GWGUI.App.Rendering;

public sealed record ScpTrackPreparation(
    int Cylinder,
    int Head,
    ScpTrackVisualState State,
    int ValidSectors = 0,
    int InvalidSectors = 0,
    int UnverifiedSectors = 0,
    bool HasFlux = true);
