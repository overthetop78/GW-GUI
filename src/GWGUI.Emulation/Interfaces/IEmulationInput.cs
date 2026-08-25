namespace GWGUI.Emulation.Interfaces;

public interface IEmulationInput
{
    bool SupportsPointerCapture { get; }
    bool CapturePointerOnClick { get; }
    IReadOnlyDictionary<string, string> KeyboardBindings { get; }
    bool SupportsControllerPointerSwitch { get; }
    bool ControllerPointerMode { get; }
    void SetInput(EmulationInputSnapshot snapshot);
    void SetControllerPortDevice(int port, EmulationPeripheralCategory peripheral);
    ValueTask<bool> SwitchControllerPointerAsync(CancellationToken cancellationToken = default);
}
