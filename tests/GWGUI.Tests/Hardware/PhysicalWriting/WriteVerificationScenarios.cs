using GWGUI.App.Enums.Services.PhysicalDiskWriting;
using GWGUI.App.Interfaces.Services.PhysicalDiskWriting;
using GWGUI.App.Services.PhysicalDiskWriting;
namespace GWGUI.Tests.Hardware.PhysicalWriting;
internal static class WriteVerificationScenarios
{
    public static async Task Verify(bool success)
    {
        var device = new WritePlanningScenarios.Device();
        var verifier = new Verifier(device, success);
        var result = await new PhysicalDiskWriteService(device, verifier).WriteAsync(WritePlanningScenarios.Image, WritePlanningScenarios.Options with { Verify = true });
        Assert.Equal(success, result.IsSuccess);
        Assert.Equal(success ? 2 : 1, result.WrittenTracks);
        Assert.Equal(result.WrittenTracks, verifier.Count);
        if (!success) { var failure = Assert.Single(result.Failures); Assert.Equal(PhysicalDiskWriteFailureCategory.Verification, failure.Category); Assert.Equal(0, failure.Cylinder); Assert.Equal(0, failure.Head); }
        Assert.Equal("close", device.Calls.Last());
    }
    private sealed class Verifier(WritePlanningScenarios.Device device, bool success) : IPhysicalTrackVerifier
    {
        public int Count { get; private set; }
        public ValueTask<bool> VerifyAsync(int cylinder, int head, ReadOnlyMemory<uint> expectedDeviceTicks, CancellationToken cancellationToken = default)
        {
            Assert.Equal(Count, cylinder); Assert.Equal(Count, head);
            Assert.Equal(device.Writes[Count++], expectedDeviceTicks.ToArray());
            return ValueTask.FromResult(success);
        }
    }
}
