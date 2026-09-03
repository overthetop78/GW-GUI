using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Settings.Hardware;
using GWGUI.Infrastructure.Hardware;

namespace GWGUI.Tests;

public sealed class GreaseweazleHardwareRegistryTests
{
    private const string SerialNumber = "GW0CF19C9E7592000007E0941B";

    [Fact]
    public async Task ScanKeepsIdentifiedDeviceWhenInfoEndsWithGithubWarning()
    {
        var registry = CreateRegistry(InfoResult(1,
            "Host Tools: 1.23",
            "Port: COM3",
            "Model: Greaseweazle V4.1",
            "Firmware: 1.6",
            $"Serial: {SerialNumber}",
            "** FATAL ERROR: HTTPSConnectionPool(host='api.github.com'): Connection timed out"));

        var scan = await registry.ScanAsync("gw.exe", [ConfiguredController()]);

        var controller = Assert.Single(scan.ConfiguredControllers);
        Assert.True(controller.IsAvailable);
        Assert.Equal("COM3", controller.LastPort);
        Assert.Equal("Greaseweazle V4.1", controller.Model);
        Assert.Equal(SerialNumber, controller.UsbSerialNumber);
    }

    [Fact]
    public async Task ScanRejectsGithubFailureWithoutDeviceIdentity()
    {
        var registry = CreateRegistry(InfoResult(1,
            "** FATAL ERROR: HTTPSConnectionPool(host='api.github.com'): Connection timed out"));

        var scan = await registry.ScanAsync("gw.exe", [ConfiguredController()]);

        Assert.False(Assert.Single(scan.ConfiguredControllers).IsAvailable);
    }

    [Fact]
    public async Task ScanRejectsNonNetworkFailureEvenWithDeviceIdentity()
    {
        var registry = CreateRegistry(InfoResult(1,
            "Port: COM3",
            "Model: Greaseweazle V4.1",
            "Firmware: 1.6",
            $"Serial: {SerialNumber}",
            "** FATAL ERROR: Device communication failed"));

        var scan = await registry.ScanAsync("gw.exe", [ConfiguredController()]);

        Assert.False(Assert.Single(scan.ConfiguredControllers).IsAvailable);
    }

    private static GreaseweazleHardwareRegistry CreateRegistry(GwExecutionResult result) => new(
        new TestDiscovery([new SerialDevice("COM3", SerialNumber, "USB Serial Device (COM3)",
            0x1209, 0x4d69, UsbSerialNumber: SerialNumber)]),
        new TestRunner(result));

    private static ControllerSettings ConfiguredController() => new()
    {
        UsbId = SerialNumber,
        UsbSerialNumber = SerialNumber,
        LastPort = "COM3",
        Model = "Greaseweazle V4.1"
    };

    private static GwExecutionResult InfoResult(int exitCode, params string[] lines) => new(
        exitCode, false, TimeSpan.Zero, lines.Select(line =>
            new GwOutputLine(DateTimeOffset.UnixEpoch, GwOutputStream.Standard, line)).ToArray());

    private sealed class TestDiscovery(IReadOnlyList<SerialDevice> devices) : ISerialDeviceDiscovery
    {
        public IReadOnlyList<SerialDevice> FindSerialDevices() => devices;
    }

    private sealed class TestRunner(GwExecutionResult result) : IGreaseweazleRunner
    {
        public bool IsRunning => false;

        public Task<GwExecutionResult> RunAsync(
            GwCommand command,
            IProgress<GwOutputLine>? output = null,
            CancellationToken cancellationToken = default) => Task.FromResult(result);
    }
}
