using GWGUI.MediaFileSystems.Constants;

using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.Dos;

/// <summary>Reconstruit une chaîne de secteurs Atari DOS.</summary>
public static class AtariDosFileReader
{
    /// <summary>Lit la portion de chaîne délimitée par le nombre de secteurs de l'entrée de répertoire.</summary>
    public static AtariDosFileData Read(
        IMediaSectorImage image,
        int first,
        int expectedSectors,
        bool usesExtendedLinks,
        ICollection<string> warnings,
        string name)
    {
        var result = new List<byte>();
        var sectors = new List<int>();
        var storedFileNumbers = new List<int?>();
        var current = first;
        var visited = new HashSet<int>();
        var valid = true;
        while (sectors.Count < expectedSectors)
        {
            if (current == 0)
            {
                warnings.Add(AtariDosWarnings.PrematureEnd(name, expectedSectors, sectors.Count));
                valid = false;
                break;
            }
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
            var nextHigh = usesExtendedLinks ? sector[link] : sector[link] & AtariDosFileSystemLayout.NextSectorHighMask;
            var next = nextHigh << BitConstants.BitsPerByte | sector[link + 1];
            var used = (int)sector[link + 2];
            if (used > link)
            {
                warnings.Add(AtariDosWarnings.InvalidUsedLength(name, current, used, link));
                used = link;
                valid = false;
            }
            sectors.Add(current);
            storedFileNumbers.Add(usesExtendedLinks
                ? null
                : sector[link] >> AtariDosFileSystemLayout.FileOwnerShift);
            result.AddRange(sector.AsSpan(0, used).ToArray());
            current = next;
        }
        if (expectedSectors == 0 && current != 0)
        {
            warnings.Add(AtariDosWarnings.UnexpectedDataForEmptyFile(name, current));
            valid = false;
        }
        var chain = new AtariDosSectorChain(
            Array.AsReadOnly(sectors.ToArray()),
            Array.AsReadOnly(storedFileNumbers.ToArray()),
            current,
            sectors.Count == expectedSectors);
        return new(
            Array.AsReadOnly(result.ToArray()),
            chain,
            valid && chain.IsComplete,
            [],
            []);
    }
}
