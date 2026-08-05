namespace GWGUI.Domain.Hardware;

public sealed record SerialDevice(
    string Port,
    string StableId,
    string DisplayName,
    int? VendorId = null,
    int? ProductId = null,
    string? Manufacturer = null,
    string? Product = null,
    string? UsbSerialNumber = null,
    string? PnpDeviceId = null,
    string? UsbLocation = null);

public interface ISerialDeviceDiscovery
{
    IReadOnlyList<SerialDevice> FindSerialDevices();
}
