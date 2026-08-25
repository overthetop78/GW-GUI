namespace GWGUI.Emulation.Atari.Contracts;

public sealed record AtariCoreInstallProgress(long DownloadedBytes, long? TotalBytes)
{
    public double? Fraction => TotalBytes is > 0 ? DownloadedBytes / (double)TotalBytes.Value : null;
}
