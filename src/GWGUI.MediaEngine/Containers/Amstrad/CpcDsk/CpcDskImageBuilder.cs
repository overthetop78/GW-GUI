using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;

/// <summary>Construit un modèle CPCEMU réinscriptible depuis une image sectorielle reconstruite.</summary>
public static class CpcDskImageBuilder
{
    /// <summary>Regroupe les secteurs par adresse physique et synthétise uniquement les champs absents du flux sectoriel.</summary>
    public static CpcDskImage Build(SectorImage image, CpcDskContainerKind kind)
    {
        if (image.Cylinders > CpcDskLayout.MaximumCylinderCount || image.Heads > CpcDskLayout.MaximumHeadCount) throw CpcDskExceptions.InvalidContainer("the geometry exceeds CPCEMU limits");
        var tracks = new List<CpcDskTrack>(checked(image.Cylinders * image.Heads));
        for (var cylinder = 0; cylinder < image.Cylinders; cylinder++)
        {
            for (var head = 0; head < image.Heads; head++)
            {
                var blocks = image.AvailableBlocks.Where(block => block.Address.Cylinder == cylinder && block.Address.Head == head).OrderBy(block => block.Address.Number).ToArray();
                var sectors = blocks.Select(block => BuildSector(block, cylinder, head)).ToArray();
                var defaultSizeCode = sectors.Length == 0 ? (byte)0 : sectors.GroupBy(sector => sector.SizeCode).OrderByDescending(group => group.Count()).First().Key;
                tracks.Add(new(tracks.Count, sectors.Length > 0, checked((byte)cylinder), checked((byte)head), defaultSizeCode, CpcDskFormat.DefaultGap3Length, CpcDskFormat.DefaultFillerByte, sectors));
            }
        }
        return new(kind, checked((byte)image.Cylinders), checked((byte)image.Heads), tracks, image);
    }

    private static CpcDskSector BuildSector(SectorBlock block, int cylinder, int head)
    {
        var sizeCode = block.FormatCode is { } declared && NominalSize(declared) == block.Data.Count ? declared : GetSizeCode(block.Data.Count);
        var status1 = block.IntegrityValid == false ? checked((byte)CpcDskLayout.DataErrorMask) : (byte)0;
        return new(checked((byte)cylinder), checked((byte)head), checked((byte)block.Address.Number), sizeCode, status1, 0, block.Data.ToArray());
    }

    private static byte GetSizeCode(int size)
    {
        for (byte code = 0; code <= CpcDskLayout.SectorSizeCodeMask; code++) if (NominalSize(code) == size) return code;
        throw CpcDskExceptions.InvalidContainer($"sector size {size} is not representable by a CPCEMU size code");
    }

    private static int NominalSize(byte sizeCode) => CpcDskLayout.MinimumSectorSize << (sizeCode & CpcDskLayout.SectorSizeCodeMask);
}
