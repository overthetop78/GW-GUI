namespace GWGUI.App.Rendering.Emulation.Processing;

internal readonly record struct FilterColor(float Red, float Green, float Blue)
{
    internal float this[int channel] => channel switch
    {
        0 => Red,
        1 => Green,
        2 => Blue,
        _ => throw new ArgumentOutOfRangeException(nameof(channel))
    };
}
