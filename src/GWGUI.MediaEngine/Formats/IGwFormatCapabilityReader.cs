namespace GWGUI.MediaEngine.Formats;

public interface IGwFormatCapabilityReader
{
    Task<GwFormatCapabilities> ReadAsync(
        string executablePath,
        CancellationToken cancellationToken = default);
}
