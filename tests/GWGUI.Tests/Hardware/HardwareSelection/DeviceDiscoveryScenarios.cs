using GWGUI.Domain.Hardware;
using GWGUI.Infrastructure.Hardware;
namespace GWGUI.Tests.Hardware.HardwareSelection;
internal static class DeviceDiscoveryScenarios
{
    public static async Task DuplicatesAndFailedInformation(bool warning)
    {
        SerialDevice Device(string port,string serial)=>new(port,serial,"Greaseweazle",0x1209,0x4d69,UsbSerialNumber:serial);
        var found=new[]{Device("ONE","one"),Device("ONE","one"),Device("TWO","two"),Device("BAD","bad")};
        var discovery=GWGUI.Tests.Application.TestInfrastructure.ControlledDependencies.Simulate<ISerialDeviceDiscovery>((method,_)=>method.Name=="FindSerialDevices"?found:throw new InvalidOperationException(method.Name));
        var runner=GWGUI.Tests.Application.TestInfrastructure.ControlledDependencies.Simulate<GWGUI.Domain.Commands.Execution.IGreaseweazleRunner>((method,args)=>
        {
            Assert.Equal("RunAsync",method.Name); var command=Assert.IsType<GWGUI.Domain.Commands.GwCommand>(args[0]); var port=command.Arguments[^1];
            var text=port=="BAD"?"partial output":$"Model: GW\nFirmware: 1.0\nSerial: {port.ToLowerInvariant()}\nPort: {port}"+(warning?"\nGitHub check failed":"");
            return Task.FromResult(new GWGUI.Domain.Commands.Execution.GwExecutionResult(warning||port=="BAD"?1:0,false,TimeSpan.Zero,[new(DateTimeOffset.UnixEpoch,GWGUI.Domain.Commands.Execution.GwOutputStream.Standard,text)]));
        });
        var registry=new GreaseweazleHardwareRegistry(discovery,runner); var result=await registry.ScanAsync("virtual",[]);
        Assert.Empty(result.ConfiguredControllers); Assert.Equal(new[]{"one","two"},result.UnconfiguredControllers.Select(controller=>controller.UsbId));
        Assert.All(result.UnconfiguredControllers,controller=>Assert.True(controller.IsAvailable));
        var next=await registry.ScanAsync("virtual",result.UnconfiguredControllers); Assert.Empty(next.UnconfiguredControllers); Assert.Equal(2,next.ConfiguredControllers.Count);
    }
    public static async Task Scan(bool present)
    {
        var configured = new GWGUI.Domain.Settings.Hardware.ControllerSettings { UsbId = "SERIAL", LastPort = "OLD", IsAvailable = true };
        var discovery = GWGUI.Tests.Application.TestInfrastructure.ControlledDependencies.Simulate<ISerialDeviceDiscovery>((method, _) => {
            Assert.Equal("FindSerialDevices", method.Name);
            return present ? new SerialDevice[] { new("VIRTUAL", "id", "Greaseweazle", 0x1209, 0x4d69, UsbSerialNumber: "serial"), new("OTHER", "unknown", "unrelated") } : [];
        });
        var calls = 0;
        var runner = GWGUI.Tests.Application.TestInfrastructure.ControlledDependencies.Simulate<GWGUI.Domain.Commands.Execution.IGreaseweazleRunner>((method, args) => {
            Assert.Equal("RunAsync", method.Name); calls++;
            var command = Assert.IsType<GWGUI.Domain.Commands.GwCommand>(args[0]);
            Assert.Equal("info", command.Verb); Assert.Equal(new[] { "--device", "VIRTUAL" }, command.Arguments);
            return Task.FromResult(new GWGUI.Domain.Commands.Execution.GwExecutionResult(0, false, TimeSpan.Zero,
                [new(DateTimeOffset.UnixEpoch, GWGUI.Domain.Commands.Execution.GwOutputStream.Standard, "Model: GW\nFirmware: 1.0\nSerial: serial\nPort: VIRTUAL")]));
        });
        var result = await new GreaseweazleHardwareRegistry(discovery, runner).ScanAsync("virtual-tool", [configured]);
        var controller = Assert.Single(result.ConfiguredControllers);
        Assert.NotSame(configured, controller); Assert.Equal("SERIAL", controller.UsbId);
        Assert.Equal(present, controller.IsAvailable); Assert.Equal(present ? "VIRTUAL" : "OLD", controller.LastPort);
        Assert.Empty(result.UnconfiguredControllers); Assert.Equal(present ? 1 : 0, calls);
        Assert.Equal("OLD", configured.LastPort); Assert.True(configured.IsAvailable);
    }
    public static void Identity()
    {
        var unknown=new SerialDevice("virtual","id","unknown");
        Assert.False(GreaseweazleDeviceMatcher.IsCandidate(unknown));
        Assert.Equal(20,GreaseweazleDeviceMatcher.Score(unknown with{VendorId=0x1209,ProductId=0x4d69}));
        Assert.Equal(19,GreaseweazleDeviceMatcher.Score(unknown with{Product="GW-COMPAT"}));
        Assert.Equal(10,GreaseweazleDeviceMatcher.Score(unknown with{UsbSerialNumber="gw123"}));
        Assert.Equal(20,GreaseweazleDeviceMatcher.Score(unknown with{Manufacturer="Keir Fraser",Product="Greaseweazle"}));
        Assert.Equal(0,GreaseweazleDeviceMatcher.Score(unknown with{Manufacturer="other",Product="Greaseweazle"}));
    }
}
