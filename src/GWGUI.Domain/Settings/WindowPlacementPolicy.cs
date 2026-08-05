namespace GWGUI.Domain.Settings;

public sealed record NormalizedWindowPlacement(double Width, double Height, double? Left, double? Top);

public static class WindowPlacementPolicy
{
    public static NormalizedWindowPlacement Normalize(WindowPlacementSettings value, double minimumWidth, double minimumHeight,
        double virtualLeft, double virtualTop, double virtualWidth, double virtualHeight, double visibleMargin = 80)
    {
        var width = Math.Clamp(value.Width, minimumWidth, Math.Max(minimumWidth, virtualWidth));
        var height = Math.Clamp(value.Height, minimumHeight, Math.Max(minimumHeight, virtualHeight));
        if (value.Left is not double left || value.Top is not double top || !double.IsFinite(left) || !double.IsFinite(top))
            return new(width, height, null, null);
        var right = virtualLeft + virtualWidth;
        var bottom = virtualTop + virtualHeight;
        var intersects = left + width >= virtualLeft + visibleMargin && left <= right - visibleMargin &&
                         top + height >= virtualTop + visibleMargin && top <= bottom - visibleMargin;
        if (!intersects) return new(width, height, null, null);
        return new(width, height,
            Math.Clamp(left, virtualLeft - width + visibleMargin, right - visibleMargin),
            Math.Clamp(top, virtualTop, bottom - visibleMargin));
    }
}
