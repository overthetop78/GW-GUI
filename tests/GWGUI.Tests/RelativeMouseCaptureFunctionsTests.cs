using System.Windows;
using GWGUI.App.Input;

namespace GWGUI.Tests;

public sealed class RelativeMouseCaptureFunctionsTests
{
    [Fact]
    public void CenterAndDelta_HandleFractionalDisplayCoordinates()
    {
        var center = RelativeMouseCaptureFunctions.Center(
            RelativeMouseCaptureTestConstants.DisplayWidth, RelativeMouseCaptureTestConstants.DisplayHeight);
        Assert.Equal(RelativeMouseCaptureTestConstants.ExpectedCenterX, center.X);
        Assert.Equal(RelativeMouseCaptureTestConstants.ExpectedCenterY, center.Y);
        Assert.Equal((RelativeMouseCaptureTestConstants.ExpectedDeltaX,
                RelativeMouseCaptureTestConstants.ExpectedDeltaY),
            RelativeMouseCaptureFunctions.Delta(new Point(RelativeMouseCaptureTestConstants.PointerX,
                RelativeMouseCaptureTestConstants.PointerY), center));
    }

    [Fact]
    public void MovementAndNativeButtonState_AreDeterministic()
    {
        Assert.False(RelativeMouseCaptureFunctions.HasMovement(RelativeMouseCaptureConstants.NoMovement,
            RelativeMouseCaptureConstants.NoMovement));
        Assert.True(RelativeMouseCaptureFunctions.HasMovement(RelativeMouseCaptureTestConstants.Movement,
            RelativeMouseCaptureConstants.NoMovement));
        Assert.True(RelativeMouseCaptureFunctions.IsPressed(unchecked((short)RelativeMouseCaptureConstants.PressedKeyMask)));
        Assert.False(RelativeMouseCaptureFunctions.IsPressed(RelativeMouseCaptureTestConstants.ReleasedKeyState));
    }

    [Fact]
    public void CaptureState_ReleasesOnFocusPauseFullscreenOrForcedCloseWithoutRemainingStuck()
    {
        var state = new RelativeMouseCaptureState();
        state.Capture();
        Assert.True(state.IsCaptured);
        Assert.True(state.Release());
        Assert.False(state.IsCaptured);
        Assert.False(state.Release());

        state.Capture();
        Assert.True(state.Release());
        Assert.False(state.IsCaptured);
    }
}

internal static class RelativeMouseCaptureTestConstants
{
    internal const double DisplayWidth = 101d;
    internal const double DisplayHeight = 51d;
    internal const double ExpectedCenterX = 50.5d;
    internal const double ExpectedCenterY = 25.5d;
    internal const double PointerX = 61d;
    internal const double PointerY = 20d;
    internal const int ExpectedDeltaX = 11;
    internal const int ExpectedDeltaY = -6;
    internal const int Movement = 1;
    internal const short ReleasedKeyState = 0;
}
