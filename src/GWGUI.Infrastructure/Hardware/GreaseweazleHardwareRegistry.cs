using GWGUI.Domain.Commands;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Settings;

namespace GWGUI.Infrastructure.Hardware;

public sealed class GreaseweazleHardwareRegistry(
    ISerialDeviceDiscovery discovery,
    IGreaseweazleRunner runner,
    IGwCommandBuilder? commandBuilder = null) : IHardwareRegistry
{
    private readonly IGwCommandBuilder commandBuilder = commandBuilder ?? new GwCommandBuilder();

    public async Task<IReadOnlyList<ControllerSettings>> ScanAsync(
        string executable,
        IReadOnlyList<ControllerSettings> configuredControllers,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executable);
        var controllers = configuredControllers.Select(CloneUnavailable).ToList();
        var serialDevices = await Task.Run(discovery.FindSerialDevices, cancellationToken);
        foreach (var serial in serialDevices)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await runner.RunAsync(commandBuilder.BuildInfo(new(executable, serial.Port)), cancellationToken: cancellationToken);
            if (!result.IsSuccess) continue;
            var parsed = GwInfoParser.Parse(string.Join(Environment.NewLine, result.Output.Select(line => line.Text)));
            var usbId = string.IsNullOrWhiteSpace(parsed.SerialNumber) ? serial.StableId : parsed.SerialNumber;
            var controller = controllers.FirstOrDefault(item => item.UsbId == usbId);
            if (controller is null)
            {
                controller = new ControllerSettings { UsbId = usbId };
                controllers.Add(controller);
            }
            controller.LastPort = serial.Port;
            controller.Model = parsed.Model ?? serial.DisplayName;
            controller.IsAvailable = true;
        }
        return controllers;
    }

    private static ControllerSettings CloneUnavailable(ControllerSettings source) => new()
    {
        UsbId = source.UsbId,
        LastPort = source.LastPort,
        Model = source.Model,
        IsAvailable = false
    };
}
