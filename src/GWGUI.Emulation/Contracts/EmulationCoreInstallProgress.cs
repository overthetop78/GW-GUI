namespace GWGUI.Emulation.Contracts;

public sealed record EmulationCoreInstallProgress(long DownloadedBytes, long? TotalBytes)
{
    public double? Fraction => TotalBytes is > 0 ? DownloadedBytes / (double)TotalBytes.Value : null;
}
