namespace GWGUI.App.Input;

internal sealed class RelativeMouseCaptureState
{
    internal bool IsCaptured { get; private set; }

    internal void Capture() => IsCaptured = true;

    internal bool Release()
    {
        if (!IsCaptured) return false;
        IsCaptured = false;
        return true;
    }
}
