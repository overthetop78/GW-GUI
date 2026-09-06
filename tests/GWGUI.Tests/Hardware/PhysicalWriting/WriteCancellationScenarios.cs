using GWGUI.App.Enums.Services.PhysicalDiskWriting;
using GWGUI.App.Services.PhysicalDiskWriting;
using GWGUI.Infrastructure.Hardware.Greaseweazle;
namespace GWGUI.Tests.Hardware.PhysicalWriting;
internal static class WriteCancellationScenarios
{
    public static async Task Disconnected()
    {
        var error=new IOException("synthetic disconnected drive");
        var device=new WritePlanningScenarios.Device { OnWrite=(count,_)=>{if(count==2) throw error;} };
        var result=await new PhysicalDiskWriteService(device).WriteAsync(WritePlanningScenarios.Image,WritePlanningScenarios.Options);
        Assert.Equal(1,result.WrittenTracks); Assert.False(result.IsSuccess); Assert.False(result.Cancelled);
        var failure=Assert.Single(result.Failures); Assert.Same(error,failure.Exception); Assert.Equal(1,failure.Cylinder); Assert.Equal(1,failure.Head);
        Assert.Equal(2,device.Writes.Count); Assert.Equal("close",device.Calls[^1]);
    }
    public static async Task Cancel()
    {
        using var source = new CancellationTokenSource();
        var device = new WritePlanningScenarios.Device { OnWrite = (count, token) => { if (count == 2) { source.Cancel(); token.ThrowIfCancellationRequested(); } } };
        var result = await new PhysicalDiskWriteService(device).WriteAsync(WritePlanningScenarios.Image, WritePlanningScenarios.Options, cancellationToken: source.Token);
        Assert.True(result.Cancelled); Assert.False(result.IsSuccess);
        Assert.Equal(1, result.WrittenTracks); Assert.Empty(result.Failures);
        Assert.Equal("close", device.Calls.Last());
    }
    public static async Task Protected()
    {
        var error = new GreaseweazleProtocolException((GreaseweazleCommand)7, GreaseweazleAcknowledgement.WriteProtected);
        var device = new WritePlanningScenarios.Device { OnWrite = (_, _) => throw error };
        var result = await new PhysicalDiskWriteService(device).WriteAsync(WritePlanningScenarios.Image, WritePlanningScenarios.Options);
        Assert.Equal(0, result.WrittenTracks); Assert.False(result.Cancelled);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(PhysicalDiskWriteFailureCategory.WriteProtected, failure.Category);
        Assert.Same(error, failure.Exception); Assert.Single(device.Writes);
        Assert.Equal("close", device.Calls.Last());
    }
}
