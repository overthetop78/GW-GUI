namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayDecimalPoint
{
    internal static void AddTo(ICollection<SegmentDisplayElement> elements, bool enabled)
    {
        if (enabled) elements.Add(new(.91f, .9f, .91f, .9f, true));
    }
}
