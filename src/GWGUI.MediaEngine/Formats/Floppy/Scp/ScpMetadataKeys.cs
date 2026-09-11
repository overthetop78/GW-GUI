namespace GWGUI.MediaEngine.Formats.Floppy.Scp;

/// <summary>Defines SCP-specific metadata keys shared by its reader and writer adapter.</summary>
internal static class ScpMetadataKeys
{
    public const string Version = "scp.version.raw";
    public const string VersionText = "scp.version";
    public const string DiskType = "scp.diskType";
    public const string Flags = "scp.flags";
    public const string BitCellEncoding = "scp.bitCellEncoding";
    public const string Heads = "scp.heads";
    public const string Resolution = "scp.resolution";
    public const string ResolutionNanoseconds = "scp.resolutionNanoseconds";
    public const string ChecksumValid = "scp.checksumValid";
}
