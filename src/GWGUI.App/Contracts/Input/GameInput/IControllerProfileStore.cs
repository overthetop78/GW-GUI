namespace GWGUI.App.Services.Input.GameInput;

internal interface IControllerProfileStore
{
    IReadOnlyDictionary<string, ControllerVisualModel> GetModels();
    ControllerVisualProfile? GetVisual(string deviceId);
    void SetVisual(string deviceId, ControllerVisualModel model, string displayName);
    void RemoveVisual(string deviceId);
    ControllerAnalogDeadZoneProfile GetAnalog(string deviceId);
    void PreviewAnalog(string deviceId, ControllerAnalogDeadZoneProfile profile);
    void SaveAnalog(string deviceId, ControllerAnalogDeadZoneProfile profile);
}

internal sealed class ControllerProfileStore : IControllerProfileStore
{
    public IReadOnlyDictionary<string, ControllerVisualModel> GetModels() => ControllerVisualProfileStore.GetModels();
    public ControllerVisualProfile? GetVisual(string deviceId) => ControllerVisualProfileStore.TryGet(deviceId, out var profile) ? profile : null;
    public void SetVisual(string deviceId, ControllerVisualModel model, string displayName) => ControllerVisualProfileStore.Set(deviceId, model, displayName);
    public void RemoveVisual(string deviceId) => ControllerVisualProfileStore.Remove(deviceId);
    public ControllerAnalogDeadZoneProfile GetAnalog(string deviceId) => ControllerAnalogDeadZoneProfileStore.Get(deviceId);
    public void PreviewAnalog(string deviceId, ControllerAnalogDeadZoneProfile profile) => ControllerAnalogDeadZoneProfileStore.Preview(deviceId, profile);
    public void SaveAnalog(string deviceId, ControllerAnalogDeadZoneProfile profile) => ControllerAnalogDeadZoneProfileStore.Save(deviceId, profile);
}
