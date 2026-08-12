using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Lisa;

/// <summary>Lit les noms des catalogues Lisa tabulaires, hachés et B-tree.</summary>
public static class LisaCatalogReader
{
    /// <summary>Lit les noms associés aux identifiants de fichiers.</summary>
    /// <param name="image">Image sectorielle taguée.</param>
    /// <param name="version">Version du catalogue.</param>
    /// <returns>Noms trouvés et avertissements produits.</returns>
    public static LisaCatalogResult Read(SectorImage image, ushort version)
    {
        var result = new Dictionary<ushort, string>();
        var pages = image.AvailableBlocks.Select(block => (Block: block, HasTag: LisaPageTagReader.TryRead(block, out var tag), Tag: tag)).Where(item => item.HasTag && item.Tag.FileId == LisaFileSystemLayout.CatalogFileId).OrderBy(item => item.Tag.PageNumber).Select(item => item.Block).ToArray();
        if (pages.Length == 0) return new(result, [LisaFileSystemExceptions.MissingCatalog(version)]);
        var bytes = pages.SelectMany(block => block.Data).ToArray();
        if ((LisaCatalogVersion)version == LisaCatalogVersion.Table) ReadTableEntries(bytes, result);
        else ReadLaterEntries(bytes, result);
        return new(result.ToFrozenDictionary(), []);
    }

    /// <summary>Lit les entrées de la disposition tabulaire.</summary>
    private static void ReadTableEntries(byte[] bytes, IDictionary<ushort, string> result)
    {
        for (var offset = 0; offset + LisaFileSystemLayout.TableEntrySize <= bytes.Length; offset += LisaFileSystemLayout.TableEntrySize) ReadEntry(bytes, offset, bytes[offset], result);
    }

    /// <summary>Lit les entrées des dispositions hachée et B-tree.</summary>
    private static void ReadLaterEntries(byte[] bytes, IDictionary<ushort, string> result)
    {
        for (var offset = LisaFileSystemLayout.TreeEntriesOffset; offset + LisaFileSystemLayout.TreeEntrySize <= bytes.Length; offset += LisaFileSystemLayout.TreeEntrySize)
        {
            if (bytes[offset] != LisaFileSystemLayout.UnusedCatalogEntryMarker || bytes[offset + LisaFileSystemLayout.CatalogNameOffset] < LisaVolumeHeader.MinimumPrintableCharacter) continue;
            ReadEntry(bytes, offset, LisaFileSystemLayout.CatalogNameLength, result);
        }
    }

    /// <summary>Décode une entrée de catalogue validée.</summary>
    private static void ReadEntry(byte[] bytes, int offset, int length, IDictionary<ushort, string> result)
    {
        if (length is 0 or > LisaFileSystemLayout.CatalogNameLength || offset + LisaFileSystemLayout.CatalogNameOffset + length > bytes.Length) return;
        var name = LisaVolumeHeader.DecodeName(bytes.AsSpan(offset + LisaFileSystemLayout.CatalogNameOffset, length));
        var fileId = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset + LisaFileSystemLayout.CatalogFileIdOffset, sizeof(ushort)));
        if (LisaPageTagReader.IsUserFile(fileId) && !string.IsNullOrWhiteSpace(name)) result.TryAdd(fileId, name);
    }
}

/// <summary>Résultat de lecture d'un catalogue Lisa.</summary>
/// <param name="Names">Noms indexés par identifiant de fichier.</param>
/// <param name="Warnings">Avertissements produits pendant la lecture.</param>
public sealed record LisaCatalogResult
{
    /// <summary>Crée un résultat de lecture du catalogue.</summary>
    public LisaCatalogResult(IReadOnlyDictionary<ushort, string> names, IReadOnlyList<string> warnings)
    {
        Names = names;
        Warnings = warnings;
    }

    /// <summary>Noms indexés par identifiant de fichier.</summary>
    public IReadOnlyDictionary<ushort, string> Names { get; }

    /// <summary>Avertissements produits pendant la lecture.</summary>
    public IReadOnlyList<string> Warnings { get; }
}
