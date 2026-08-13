namespace GWGUI.App.Services.PhysicalDiskWriting;

public enum PhysicalDiskWriteFailureKind
{
    Validation,
    Device,
    WriteProtected,
    Verification,
    Cancelled,
    Unexpected
}
