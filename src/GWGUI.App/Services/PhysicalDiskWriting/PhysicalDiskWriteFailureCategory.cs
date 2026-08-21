namespace GWGUI.App.Services.PhysicalDiskWriting;

public enum PhysicalDiskWriteFailureCategory
{
    Validation,
    Device,
    WriteProtected,
    Verification,
    Cancelled,
    Unexpected
}
