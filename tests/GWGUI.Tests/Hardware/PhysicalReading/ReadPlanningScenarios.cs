using GWGUI.App.Services.PhysicalDiskReading;

namespace GWGUI.Tests.Hardware.PhysicalReading;

internal static class ReadPlanningScenarios
{
    public static async Task EngineValidation(int variant)
    {
        using var context = new GWGUI.Tests.Interface.ReadViews.ReadOperationScenarios.Context();
        context.Settings.Engines.PhysicalRead = GWGUI.Domain.Settings.Engines.OperationEngine.Internal;
        context.Hardware = new(new(){ControllerUsbId="virtual",Selection="B"},"virtual-port",true,"synthetic");
        context.Model.Read.Tracks.Enabled = true; context.Model.Read.Tracks.Value = "c=2-3:h=1";
        context.Model.Read.Revs.Enabled = true; context.Model.Read.Revs.Value = "4";
        var plan = context.Controller.CreateInternalOptions(context.Hardware);
        Assert.Equal("virtual-port",plan.PortName); Assert.Equal(4,plan.Revolutions); Assert.Equal(2,plan.Tracks.Count);
        Assert.Equal(1,plan.DriveUnit);
        if(variant==0) { context.Hardware=null; context.ValidationKey="Hardware.NotConfigured"; }
        if(variant==1) context.ValidationKey="Read.InternalRawScpOnly";
        if(variant==2)
        {
            context.View.ImageBlock.RawScpRadio.IsChecked=true; context.View.ImageBlock.KnownFormatRadio.IsChecked=false;
            context.Model.Read.Reverse.Enabled=true; context.ValidationKey="Read.InternalUnsupportedOptions";
        }
        await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ContextIdle);
        await context.Controller.ExecuteAsync();
        Assert.Single(context.ValidationMessages); Assert.Equal(0,context.Calls); Assert.False(context.Operation.IsRunning);
        Assert.Equal("1",context.Model.Read.SequenceValue);
    }
    public static async Task Invalid(int variant)
    {
        var options = ReadAcquisitionScenarios.Options;
        options = variant switch {
            0 => options with { PortName = " " },
            1 => options with { Tracks = [] },
            2 => options with { Revolutions = 0 },
            3 => options with { Revolutions = 256 },
            4 => options with { SeekRetries = -1 },
            5 => options with { FakeIndexPeriod = TimeSpan.FromMilliseconds(200), HardSectors = true },
            6 => options with { Tracks = [new(84, 0)] },
            _ => options with { Tracks = [new(0, 0), new(0, 0, 1, 1)] }
        };
        var device = new ReadAcquisitionScenarios.Device();
        await Assert.ThrowsAnyAsync<ArgumentException>(() => new PhysicalDiskFluxAcquisitionService(device).AcquireAsync(options));
        Assert.Empty(device.Calls);
    }
}
