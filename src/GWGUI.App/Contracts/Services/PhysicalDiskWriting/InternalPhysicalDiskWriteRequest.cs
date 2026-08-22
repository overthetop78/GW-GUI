namespace GWGUI.App.Contracts.Services.PhysicalDiskWriting;

public sealed record InternalPhysicalDiskWriteRequest(
    string SourcePath,
    string FormatId,
    PhysicalDiskWriteOptions Options);
