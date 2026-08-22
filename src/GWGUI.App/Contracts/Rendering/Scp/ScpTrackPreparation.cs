using GWGUI.App.Enums.Rendering.Scp;
namespace GWGUI.App.Contracts.Rendering.Scp;

public sealed record ScpTrackPreparation(
    int Cylinder,
    int Head,
    ScpTrackVisualState State,
    int ValidSectors = 0,
    int InvalidSectors = 0,
    int UnverifiedSectors = 0,
    bool HasFlux = true);
