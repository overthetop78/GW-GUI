namespace GWGUI.Emulation.HardDisks.Partitioning;

/// <summary>RDB geometry in 512-byte sectors; reserved cylinders are computed from the partition count.</summary>
public sealed record RdbGeometry(int Heads = 1, int SectorsPerTrack = 32)
{
    internal long CylinderBytes
    {
        get
        {
            if (Heads is < 1 or > 65535 || SectorsPerTrack is < 1 or > 65535)
                throw new ArgumentOutOfRangeException(nameof(Heads), "Invalid RDB geometry.");
            return checked((long)Heads * SectorsPerTrack * 512);
        }
    }
}
