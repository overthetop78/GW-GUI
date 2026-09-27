using System.IO;
using System.Security.Cryptography;

namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;

public sealed class FirmwareCatalog
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        FirmwareCatalogConstants.RomExtension,
        FirmwareCatalogConstants.BinaryExtension
    };

    private readonly string _directory;

    public FirmwareCatalog(string directory) => _directory = Path.GetFullPath(directory);

    public IReadOnlyList<Firmware> Scan()
    {
        Directory.CreateDirectory(_directory);
        return Directory.EnumerateFiles(_directory, FirmwareCatalogConstants.SearchPattern,
                SearchOption.AllDirectories)
            .Where(path => Extensions.Contains(Path.GetExtension(path)))
            .Select(Inspect)
            .OrderBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static Firmware Inspect(string path)
    {
        var file = new FileInfo(Path.GetFullPath(path));
        using var stream = file.OpenRead();
        var md5 = Convert.ToHexString(MD5.HashData(stream));
        stream.Position = 0;
        var sha256 = Convert.ToHexString(SHA256.HashData(stream));
        return new Firmware(file.FullName, file.Length, md5, sha256, file.LastWriteTimeUtc,
            FirmwareType.SystemRom, IsKnown: false, IsOfficial: false, Name: null, Version: null,
            CompatibleModels: []);
    }
}
