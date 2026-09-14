using GWGUI.MediaEngine.Primitives;

using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Reconstruit une chaîne de secteurs Atari DOS.</summary>
public static class AtariDosFileReader
{
    /// <summary>Lit la chaîne attendue et conserve le contenu partiel en cas d'erreur.</summary>
    public static AtariDosFileData Read(SectorImage image, int first, int expectedSectors, int fileNumber, bool usesExtendedLinks, ICollection<string> warnings, string name)
    {
        var result = new List<byte>();
        var current = first;
        var visited = new HashSet<int>();
        var count = 0;
        var valid = true;
        while (current != 0 && count < image.BlockCount)
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
            var nextHigh = usesExtendedLinks ? sector[link] : sector[link] & AtariDosFileSystemLayout.NextSectorHighMask;
            var next = nextHigh << BitPrimitives.BitsPerByte | sector[link + 1];
            var used = (int)sector[link + 2];
            if (used > link)
            {
                warnings.Add(AtariDosWarnings.InvalidUsedLength(name, current, used, link));
                used = link;
                valid = false;
            }
            if (!usesExtendedLinks && storedFile != fileNumber && storedFile != 0)
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
