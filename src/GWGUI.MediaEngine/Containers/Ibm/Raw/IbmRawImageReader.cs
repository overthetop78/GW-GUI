using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.MediaEngine.Containers.Ibm.Raw;

/// <summary>Lit une image sectorielle brute IBM IMG ou IMA.</summary>
public sealed class IbmRawImageReader
{
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public IbmRawImageReader() : this(File.ReadAllBytesAsync) { }

    internal IbmRawImageReader(Func<string, CancellationToken, Task<byte[]>> readBytes)
    {
        this.readBytes = readBytes ?? throw new ArgumentNullException(nameof(readBytes));
    }

    /// <summary>Lit le fichier, dÃ©tecte sa gÃ©omÃ©trie puis appelle le constructeur sectoriel IBM commun.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await readBytes(path, cancellationToken).ConfigureAwait(false);
        return IbmRawSectorImageBuilder.Create(data, IbmRawImageGeometryDetector.Detect(data), cancellationToken);
    }
}
