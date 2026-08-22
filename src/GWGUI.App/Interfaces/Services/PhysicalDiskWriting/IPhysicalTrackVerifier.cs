namespace GWGUI.App.Interfaces.Services.PhysicalDiskWriting;

public interface IPhysicalTrackVerifier
{
    ValueTask<bool> VerifyAsync(
        int cylinder,
        int head,
        ReadOnlyMemory<uint> expectedDeviceTicks,
        CancellationToken cancellationToken = default);
}
