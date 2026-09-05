namespace GWGUI.App.Rendering.Emulation.Processing;

internal static class FilterSegmentDisplayColon
{
    internal static void AddTo(ICollection<SegmentDisplayElement> elements, bool enabled)
    {
        if (!enabled) return;
        elements.Add(new(.91f, .35f, .91f, .35f, true));
        elements.Add(new(.91f, .65f, .91f, .65f, true));
    }
}
