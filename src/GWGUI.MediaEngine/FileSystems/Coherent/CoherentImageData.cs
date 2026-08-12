using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Conserve les octets aux positions logiques de l'image et la présence réelle de chaque bloc.</summary>
internal sealed class CoherentImageData
{
    private readonly bool[] presentBlocks;

    private CoherentImageData(byte[] bytes, bool[] presentBlocks)
    {
        Bytes = bytes;
        this.presentBlocks = presentBlocks;
    }

    /// <summary>Octets positionnés selon les blocs logiques de l'image.</summary>
    public byte[] Bytes { get; }
    /// <summary>Nombre de blocs logiques.</summary>
    public int BlockCount => presentBlocks.Length;

    /// <summary>Construit la représentation sans confondre un bloc absent avec un bloc de zéros présent.</summary>
    public static CoherentImageData Create(SectorImage image)
    {
        var bytes = new byte[checked(image.BlockCount * image.BlockSize)];
        var present = new bool[image.BlockCount];
        for (var block = 0; block < image.BlockCount; block++)
        {
            if (!image.TryGetBlock(block, out var sector) || sector.Data.Count != image.BlockSize) continue;
            for (var index = 0; index < sector.Data.Count; index++) bytes[block * image.BlockSize + index] = sector.Data[index];
            present[block] = true;
        }
        return new(bytes, present);
    }

    /// <summary>Indique si un bloc logique est réellement présent.</summary>
    public bool IsBlockPresent(int block) => block >= 0 && block < presentBlocks.Length && presentBlocks[block];

    /// <summary>Indique si tous les blocs traversés par une plage sont présents.</summary>
    public bool IsRangePresent(int offset, int length)
    {
        if (offset < 0 || length < 0 || offset > Bytes.Length - length) return false;
        if (length == 0) return true;
        var first = offset / CoherentFileSystemLayout.BlockSize;
        var last = (offset + length - 1) / CoherentFileSystemLayout.BlockSize;
        for (var block = first; block <= last; block++) if (!IsBlockPresent(block)) return false;
        return true;
    }
}
