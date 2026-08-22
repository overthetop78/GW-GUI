using GWGUI.App.Enums.Rendering.Scp;
namespace GWGUI.App.Functions.Rendering.Scp;

public static class ScpMediaGeometryFunctions
{
    public static float FluxRadius(int width, int height, float zoom, DiskMediaCategory mediaCategory) =>
        Math.Min(width, height) * (mediaCategory == DiskMediaCategory.Unknown ? .47f : .43f) * zoom;
}
