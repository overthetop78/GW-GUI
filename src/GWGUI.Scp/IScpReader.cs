namespace GWGUI.Scp;

public interface IScpReader
{
    Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default);
}
