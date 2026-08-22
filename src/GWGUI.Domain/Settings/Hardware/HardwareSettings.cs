namespace GWGUI.Domain.Settings.Hardware;

public sealed class ControllerSettings
{
    public string UsbId { get; set; } = "";
    public string? UsbSerialNumber { get; set; }
    public string? PnpDeviceId { get; set; }
    public string? LastUsbLocation { get; set; }
    public int? VendorId { get; set; }
    public int? ProductId { get; set; }
    public string LastPort { get; set; } = "";
    public string Model { get; set; } = "";
    public bool IsAvailable { get; set; }
}

public sealed class DriveSettings
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string ControllerUsbId { get; set; } = "";
    public string Selection { get; set; } = "";
    public string Size { get; set; } = "3.5";
    public string Density { get; set; } = "Unknown";
    public int? NominalRpm { get; set; }
}
