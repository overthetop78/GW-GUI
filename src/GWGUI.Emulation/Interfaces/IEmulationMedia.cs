namespace GWGUI.Emulation;

public interface IEmulationMedia
{
    IReadOnlyList<EmulationMedia> MountedMedia { get; }
    ValueTask InsertAsync(EmulationMedia media, CancellationToken cancellationToken = default);
    ValueTask EjectAsync(EmulationMediaSlot slot, CancellationToken cancellationToken = default);
    ValueTask SelectDiskAsync(EmulationMediaSlot slot, int index,
        CancellationToken cancellationToken = default);
}
