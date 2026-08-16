using System.Windows;

namespace GWGUI.App.Rendering;

internal static class EmulationVideoLayout
{
    internal static Size FitFourThree(double availableWidth, double availableHeight) =>
        Fit(availableWidth, availableHeight, EmulationVideoLayoutConstants.FourThreeAspectRatio);

    internal static Size Fit(double availableWidth, double availableHeight, double aspectRatio)
    {
        if (availableWidth <= EmulationVideoLayoutConstants.EmptyDimension
            || availableHeight <= EmulationVideoLayoutConstants.EmptyDimension
            || !double.IsFinite(aspectRatio)
            || aspectRatio <= EmulationVideoLayoutConstants.EmptyDimension)
            return Size.Empty;
        var width = Math.Min(availableWidth, availableHeight * aspectRatio);
        return new Size(width, width / aspectRatio);
    }
}
