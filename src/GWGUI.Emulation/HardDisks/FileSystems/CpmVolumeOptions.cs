namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>Linear CP/M 2.2 volume parameters. Sector skew/translation belongs to the block adapter.</summary>
public sealed record CpmVolumeOptions(int AllocationBlockBytes, int DirectoryEntries, long ReservedBytes = 0);
