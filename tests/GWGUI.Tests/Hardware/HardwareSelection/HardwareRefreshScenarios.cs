using GWGUI.App.Services.Hardware;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Hardware;
using GWGUI.Tests.Application.TestInfrastructure;
namespace GWGUI.Tests.Hardware.HardwareSelection;
internal static class HardwareRefreshScenarios
{
    public static async Task Refresh(bool cancel)
    {
        var original = new ControllerSettings { UsbId = "configured",LastPort = "old",IsAvailable = true };
        var known = new ControllerSettings { UsbId = "remembered",UsbSerialNumber = "serial",LastPort = "old-known" };
        var settings = new AppSettings { GwExecutablePath = "virtual-tool",Controllers = [original],UnconfiguredControllers = [known] };
        var pending = new TaskCompletionSource<HardwareScanResult>(); using var cancellation = new CancellationTokenSource();
        var registry = ControlledDependencies.Simulate<IHardwareRegistry>((method,args) =>
        {
            Assert.Equal("ScanAsync",method.Name); Assert.Equal("virtual-tool",args[0]);
            Assert.Same(original,Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<ControllerSettings>>(args[1])));
            Assert.Equal(cancellation.Token,args[2]); return pending.Task.WaitAsync(cancellation.Token);
        });
        var saved = 0;
        var store = ControlledDependencies.Simulate<ISettingsStore>((method,args) =>
        { Assert.Equal("SaveAsync",method.Name); Assert.Same(settings,args[0]); Assert.Equal(cancellation.Token,args[1]); saved++; return Task.CompletedTask; });
        var monitor = new StartupHardwareMonitor(registry,store,path => { Assert.Equal("virtual-tool",path); return true; });
        var running = monitor.CheckAsync(settings,cancellation.Token);
        Assert.False(running.IsCompleted); Assert.Equal(0,saved); Assert.Same(original,Assert.Single(settings.Controllers));
        if(cancel)
        {
            cancellation.Cancel(); await Assert.ThrowsAnyAsync<OperationCanceledException>(() => running);
            Assert.Same(original,Assert.Single(settings.Controllers)); Assert.Same(known,Assert.Single(settings.UnconfiguredControllers)); Assert.Equal(0,saved); return;
        }
        var missing = new ControllerSettings { UsbId = "configured",IsAvailable = false };
        var discovered = new ControllerSettings { UsbId = "new",LastPort = "new-port",IsAvailable = true };
        pending.SetResult(new([missing],[new() { UsbId = "different-id",UsbSerialNumber = "SERIAL",LastPort = "updated",IsAvailable = true },discovered]));
        var result = await running; Assert.True(result.Performed); Assert.Equal(1,saved);
        Assert.Same(missing,Assert.Single(result.MissingControllers)); Assert.Same(discovered,Assert.Single(result.NewControllers));
        var remembered = Assert.Single(settings.UnconfiguredControllers); Assert.Equal("remembered",remembered.UsbId); Assert.Equal("updated",remembered.LastPort); Assert.True(remembered.IsAvailable);
        Assert.NotSame(known,remembered); Assert.Equal("old-known",known.LastPort); Assert.Equal("old",original.LastPort);
    }
    public static async Task Unavailable(bool configured)
    {
        var settings = new AppSettings { GwExecutablePath = "virtual-missing" };
        if(configured) settings.Controllers.Add(new() { UsbId = "known",IsAvailable = true });
        var saved = 0; var store = ControlledDependencies.Simulate<ISettingsStore>((method,_) => { Assert.Equal("SaveAsync",method.Name); saved++; return Task.CompletedTask; });
        var result = await new StartupHardwareMonitor(ControlledDependencies.Reject<IHardwareRegistry>(),store,_ => false).CheckAsync(settings);
        Assert.Equal(configured,result.Performed); Assert.Equal(configured?1:0,saved); Assert.Empty(result.NewControllers);
        if(configured) Assert.False(Assert.Single(result.MissingControllers).IsAvailable); else Assert.Empty(result.MissingControllers);
    }
}
