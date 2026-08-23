namespace GWGUI.App.Services.Input.GameInput;

internal interface IGameInputControllerSource
{
    void StartMonitoring() { }
    IReadOnlyList<GameInputDeviceDescriptor> GetConnectedDevices();
    GameInputLiveState ReadState(string deviceId);
    void Refresh();
    bool SetRumble(string deviceId, float lowFrequency, float highFrequency, float leftTrigger, float rightTrigger);
}
