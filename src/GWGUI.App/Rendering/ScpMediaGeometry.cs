namespace GWGUI.App.Rendering;

public static class ScpMediaGeometry
{
    public static float FluxRadius(int width, int height, float zoom, DiskMediaCategory mediaCategory) =>
        Math.Min(width, height) * (mediaCategory == DiskMediaCategory.Unknown ? .47f : .43f) * zoom;
}
