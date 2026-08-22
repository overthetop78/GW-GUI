using GWGUI.App.Enums.Services.PhysicalDiskWriting;
namespace GWGUI.App.Contracts.Services.PhysicalDiskWriting;

public sealed record PhysicalTrackWriteFailure(
    int? Cylinder,
    int? Head,
    PhysicalDiskWriteFailureCategory Category,
    Exception? Exception = null);
