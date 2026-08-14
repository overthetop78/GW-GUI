using System.Security.Cryptography;

namespace GWGUI.Emulation.Amiga;

public sealed record AmigaFirmware(string Path, long Size, string Sha256, DateTime LastWriteTimeUtc);

public sealed class AmigaFirmwareCatalog
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { ".rom", ".bin", ".key" };
    private readonly string _directory;

    public AmigaFirmwareCatalog(string directory) => _directory = Path.GetFullPath(directory);

    public IReadOnlyList<AmigaFirmware> Scan()
    {
        Directory.CreateDirectory(_directory);
        return Directory.EnumerateFiles(_directory, "*", SearchOption.AllDirectories)
            .Where(path => Extensions.Contains(Path.GetExtension(path)))
            .Select(CreateEntry)
            .OrderBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static AmigaFirmware CreateEntry(string path)
    {
        var file = new FileInfo(path);
        using var stream = file.OpenRead();
        return new AmigaFirmware(file.FullName, file.Length, Convert.ToHexString(SHA256.HashData(stream)), file.LastWriteTimeUtc);
    }
}
