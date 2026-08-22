using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.MediaEngine.Containers.Scp;
using SkiaSharp;

namespace GWGUI.App.Contracts.Rendering.Scp;

public sealed record ScpRenderRequest(
    ScpImage? Image,
    int Head,
    ScpTrack? SelectedTrack,
    int Width,
    int Height,
    SKPoint Center,
    float Zoom,
    string EmptySideText,
    string SideText,
    DiskMediaCategory MediaCategory = DiskMediaCategory.Unknown);
