using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Reconstruit une chaîne de secteurs Atari DOS.</summary>
public static class AtariDosFileReader
{
    /// <summary>Lit la chaîne attendue et conserve le contenu partiel en cas d'erreur.</summary>
    public static AtariDosFileData Read(SectorImage image, int first, int expectedSectors, int fileNumber, ICollection<string> warnings, string name)
    {
        var result = new List<byte>();
        var current = first;
        var visited = new HashSet<int>();
        var count = 0;
        var valid = true;
        var limit = expectedSectors == 0 ? 1 : expectedSectors;
        while (current != 0 && count < limit)
        {
            if (!visited.Add(current))
            {
                warnings.Add(AtariDosFileSystemExceptions.CyclicDataChain(name, current));
                valid = false;
                break;
            }
            if (!AtariDosVtocReader.TrySector(image, current, out var sector))
            {
                warnings.Add(AtariDosFileSystemExceptions.MissingDataSector(name, current));
                valid = false;
                break;
            }
            if (sector.Length < AtariDosFileSystemLayout.LinkByteCount)
            {
                warnings.Add(AtariDosWarnings.TruncatedSector(name, current));
                valid = false;
                break;
            }
            var link = sector.Length - AtariDosFileSystemLayout.LinkByteCount;
            var storedFile = sector[link] >> AtariDosFileSystemLayout.FileOwnerShift;
            var next = (sector[link] & AtariDosFileSystemLayout.NextSectorHighMask) << BitPrimitives.BitsPerByte | sector[link + 1];
            var used = (int)sector[link + 2];
            if (used > link)
            {
                warnings.Add(AtariDosWarnings.InvalidUsedLength(name, current, used, link));
                used = link;
                valid = false;
            }
            if (storedFile != fileNumber && storedFile != 0)
            {
                warnings.Add(AtariDosFileSystemExceptions.InconsistentOwner(name, current, fileNumber, storedFile));
                valid = false;
            }
            result.AddRange(sector.AsSpan(0, used).ToArray());
            current = next;
            count++;
        }
        if (count != expectedSectors || current != 0)
        {
            warnings.Add(AtariDosWarnings.InconsistentCount(name, expectedSectors, count, current));
            valid = false;
        }
        return new(result, count, valid);
    }
}
