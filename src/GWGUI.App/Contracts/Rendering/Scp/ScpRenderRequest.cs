using GWGUI.App.Enums.Rendering.Scp;
using SkiaSharp;

using GWGUI.MediaEngine.Formats.Floppy.Scp;

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
