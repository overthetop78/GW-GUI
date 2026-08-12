using GWGUI.MediaEngine.FileSystems.Acorn.FileCore;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Acorn.Adfs;

/// <summary>Lit le contenu d'un fichier ADFS à travers un résolveur FileCore.</summary>
public static class AcornAdfsFileReader
{
    /// <summary>Lit un fichier, ou retourne un contenu absent lorsque la plage n'est pas intégralement disponible.</summary>
    public static IReadOnlyList<byte>? Read(SectorImage image, int address, uint length, IFileCoreAddressResolver resolver, string name, List<string> warnings, ref bool metadataValid)
    {
        if (length == 0) return [];
        if (length > int.MaxValue || address <= 0 || !resolver.TryResolveByteOffset(address, 0, out _))
        {
            warnings.Add(AcornAdfsWarnings.InvalidDataRange(name, address, 0, length));
            metadataValid = false;
            return null;
        }
        var output = new byte[(int)length];
        var copied = 0;
        while (copied < output.Length)
        {
            if (!resolver.TryResolveByteOffset(address, copied, out var byteOffset))
            {
                warnings.Add(AcornAdfsWarnings.InvalidDataRange(name, address, copied, length));
                metadataValid = false;
                return null;
            }
            var blockNumber = checked((int)(byteOffset / AcornAdfsLayout.BlockSize));
            var offsetInBlock = checked((int)(byteOffset % AcornAdfsLayout.BlockSize));
            if (!image.TryGetBlock(blockNumber, out var block) || offsetInBlock >= block.Data.Count)
            {
                warnings.Add(AcornAdfsWarnings.MissingBlock(name, address, copied, blockNumber));
                metadataValid = false;
                return null;
            }
            var count = Math.Min(block.Data.Count - offsetInBlock, output.Length - copied);
            block.Data.ToArray().AsSpan(offsetInBlock, count).CopyTo(output.AsSpan(copied));
            copied += count;
        }
        return output;
    }
}
