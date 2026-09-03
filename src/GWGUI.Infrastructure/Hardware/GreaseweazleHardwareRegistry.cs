using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Hardware.Parsing;
using GWGUI.Domain.Settings.Hardware;
using GWGUI.Infrastructure.Functions.Hardware;
namespace GWGUI.Infrastructure.Hardware;

public sealed class GreaseweazleHardwareRegistry(
    ISerialDeviceDiscovery discovery,
    IGreaseweazleRunner runner,
    IGwCommandBuilder? commandBuilder = null) : IHardwareRegistry
{
    private readonly IGwCommandBuilder commandBuilder = commandBuilder ?? new GwCommandBuilder();

    public async Task<HardwareScanResult> ScanAsync(
        string executable,
        IReadOnlyList<ControllerSettings> configuredControllers,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executable);
        var controllers = configuredControllers.Select(CloneUnavailable).ToList();
        var unconfigured = new List<ControllerSettings>();
        var serialDevices = await Task.Run(discovery.FindSerialDevices, cancellationToken);
        foreach (var serial in serialDevices.Where(GreaseweazleDeviceMatcher.IsCandidate))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await runner.RunAsync(commandBuilder.BuildInfo(new(executable, serial.Port)), cancellationToken: cancellationToken);
            var parsed = GwInfoParser.Parse(string.Join(Environment.NewLine, result.Output.Select(line => line.Text)));
            if (!GreaseweazleHardwareScanFunctions.CanUseInfo(result, parsed, serial)) continue;
            var confirmedSerial = NullIfWhiteSpace(parsed.SerialNumber) ?? serial.UsbSerialNumber;
            var usbId = confirmedSerial ?? serial.StableId;
            var controller = controllers.FirstOrDefault(item => Matches(item, serial, confirmedSerial));
            if (controller is null)
            {
                controller = new ControllerSettings { UsbId = usbId };
                unconfigured.Add(controller);
            }
            controller.LastPort = serial.Port;
            controller.Model = parsed.Model ?? serial.DisplayName;
            controller.UsbSerialNumber = confirmedSerial;
            controller.PnpDeviceId = serial.PnpDeviceId;
            controller.LastUsbLocation = serial.UsbLocation;
            controller.VendorId = serial.VendorId;
            controller.ProductId = serial.ProductId;
            controller.IsAvailable = true;
        }
        return new(controllers, unconfigured);
    }

    private static ControllerSettings CloneUnavailable(ControllerSettings source) => new()
    {
        UsbId = source.UsbId,
        UsbSerialNumber = source.UsbSerialNumber,
        PnpDeviceId = source.PnpDeviceId,
        LastUsbLocation = source.LastUsbLocation,
        VendorId = source.VendorId,
        ProductId = source.ProductId,
        LastPort = source.LastPort,
        Model = source.Model,
        IsAvailable = false
    };

    private static bool Matches(ControllerSettings controller, SerialDevice serial, string? confirmedSerial)
    {
        if (!string.IsNullOrWhiteSpace(confirmedSerial))
            return EqualsId(controller.UsbId, confirmedSerial) ||
                   EqualsId(controller.UsbSerialNumber, confirmedSerial) ||
                   EqualsId(controller.PnpDeviceId, serial.PnpDeviceId);

        if (!string.IsNullOrWhiteSpace(serial.PnpDeviceId))
            return EqualsId(controller.UsbId, serial.PnpDeviceId) ||
                   EqualsId(controller.PnpDeviceId, serial.PnpDeviceId);

        return EqualsId(controller.UsbId, serial.StableId) ||
               EqualsId(controller.LastUsbLocation, serial.UsbLocation);
    }

    private static bool EqualsId(string? left, string? right) =>
        !string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right) &&
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
