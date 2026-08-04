namespace GWGUI.Domain.Hardware;

public sealed record SerialDevice(string Port, string StableId, string DisplayName);

public interface ISerialDeviceDiscovery
{
    IReadOnlyList<SerialDevice> FindSerialDevices();
}
