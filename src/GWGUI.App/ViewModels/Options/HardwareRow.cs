using GWGUI.App.Localization.Extensions;
namespace GWGUI.App.ViewModels.Options;

public sealed class HardwareRow(string? driveId, string port, string usbId, string readerLabel,
    string size, string density, string rpm, bool available, bool configured, string configurationState)
{
    public string? DriveId { get; } = driveId;
    public string Port { get; } = port;
    public string UsbId { get; } = usbId;
    public string ReaderLabel { get; } = readerLabel;
    public string Size { get; set; } = size;
    public string Density { get; set; } = density;
    public string Rpm { get; set; } = rpm;
    public bool Available { get; } = available;
    public string AvailabilityState => LocExtension.Get(Available
        ? "Hardware.AvailableState"
        : "Hardware.UnavailableState");
    public bool Configured { get; } = configured;
    public string ConfigurationState { get; } = configurationState;
}
