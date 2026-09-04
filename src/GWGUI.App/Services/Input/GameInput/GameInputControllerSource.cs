namespace GWGUI.App.Services.Input.GameInput;
internal sealed class GameInputControllerSource : IGameInputControllerSource
{
    internal static GameInputControllerSource Instance { get; } = new();

    private GameInputControllerSource() { }

    public void StartMonitoring() => GameInputControllerReader.StartMonitoring();

    internal void StopMonitoring() => GameInputControllerReader.StopMonitoring();

    public IReadOnlyList<GameInputDeviceDescriptor> GetConnectedDevices() =>
        GameInputControllerReader.GetConnectedControllerDetailsCached();

    public GameInputLiveState ReadState(string deviceId) =>
        GameInputControllerReader.ReadDetailedState(deviceId);

    public void Refresh() => GameInputControllerReader.RefreshConnectedDevices();

    public bool SetRumble(
        string deviceId,
        float lowFrequency,
        float highFrequency,
        float leftTrigger,
        float rightTrigger) =>
        GameInputControllerReader.SetRumble(
            deviceId, lowFrequency, highFrequency, leftTrigger, rightTrigger);
}
