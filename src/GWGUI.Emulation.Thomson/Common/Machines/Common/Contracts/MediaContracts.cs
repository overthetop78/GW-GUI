using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Enums;

namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Contracts;

public sealed record MediaConfiguration(string Path, MediaCategory Category, EmulationMediaSlot Slot, string? Label = null, bool IsReadOnly = true, bool IsInserted = true);

internal sealed record DiskImageStatus(int Index, string? Path, string? Label);
internal sealed record DiskStatus(int ImageCount, int CurrentIndex, bool IsEjected, IReadOnlyList<DiskImageStatus> Images);
