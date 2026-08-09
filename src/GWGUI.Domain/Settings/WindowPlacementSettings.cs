namespace GWGUI.Domain.Settings;

public sealed class WindowPlacementSettings
{
    public double Width { get; set; } = 1360;
    public double Height { get; set; } = 820;
    public double? Left { get; set; }
    public double? Top { get; set; }
    public bool Maximized { get; set; }
}
