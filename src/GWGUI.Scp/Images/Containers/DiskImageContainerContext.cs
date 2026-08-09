namespace GWGUI.Scp.Images.Containers;

internal sealed class DiskImageContainerContext(string path, string? formatId)
{
    private byte[]? bytes;

    public string Path { get; } = path;
    public string Extension { get; } = System.IO.Path.GetExtension(path).ToLowerInvariant();
    public string? FormatId { get; } = formatId;

    public async Task<byte[]> ReadBytesAsync(CancellationToken cancellationToken)
    {
        bytes ??= await File.ReadAllBytesAsync(Path, cancellationToken).ConfigureAwait(false);
        return bytes;
    }
}
