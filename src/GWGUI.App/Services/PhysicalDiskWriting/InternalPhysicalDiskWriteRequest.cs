namespace GWGUI.App.Services.PhysicalDiskWriting;

public sealed record InternalPhysicalDiskWriteRequest(
    string SourcePath,
    string FormatId,
    PhysicalDiskWriteOptions Options);
