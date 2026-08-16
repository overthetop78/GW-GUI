using System.Windows;

namespace GWGUI.App.Input;

internal static class RelativeMouseCaptureFunctions
{
    internal static Point Center(double width, double height) => new(
        width / RelativeMouseCaptureConstants.CenterDivisor,
        height / RelativeMouseCaptureConstants.CenterDivisor);

    internal static (int X, int Y) Delta(Point current, Point center) =>
        ((int)Math.Round(current.X) - (int)Math.Round(center.X),
            (int)Math.Round(current.Y) - (int)Math.Round(center.Y));

    internal static bool HasMovement(int deltaX, int deltaY) =>
        deltaX != RelativeMouseCaptureConstants.NoMovement || deltaY != RelativeMouseCaptureConstants.NoMovement;

    internal static bool IsPressed(short state) =>
        (state & RelativeMouseCaptureConstants.PressedKeyMask) != RelativeMouseCaptureConstants.NoVirtualKeyState;
}
