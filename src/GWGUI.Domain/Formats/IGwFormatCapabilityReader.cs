namespace GWGUI.Domain.Formats;

public interface IGwFormatCapabilityReader
{
    Task<GwFormatCapabilities> ReadAsync(
        string executablePath,
        CancellationToken cancellationToken = default);
}
