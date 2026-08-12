using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.FileSystems.Readers;

namespace GWGUI.MediaEngine.FileSystems.Lisa;

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
        var pages = image.AvailableBlocks.Where(block => LisaFileSystemReader.TagFileId(block) == LisaFileSystemLayout.CatalogFileId).OrderBy(LisaFileSystemReader.TagPageNumber).ToArray();
        if (pages.Length == 0) return new(result, [LisaFileSystemExceptions.MissingCatalog(version)]);
        var bytes = pages.SelectMany(block => block.Data).ToArray();
        var entrySize = version == LisaVolumeHeader.TableCatalogVersion ? LisaFileSystemLayout.TableEntrySize : LisaFileSystemLayout.TreeEntrySize;
        var firstOffset = version == LisaVolumeHeader.TableCatalogVersion ? 0 : LisaFileSystemLayout.TreeEntriesOffset;
        for (var offset = firstOffset; offset + entrySize <= bytes.Length; offset += entrySize)
        {
            var length = version == LisaVolumeHeader.TableCatalogVersion ? bytes[offset] : LisaFileSystemLayout.CatalogNameLength;
            if (version != LisaVolumeHeader.TableCatalogVersion && (bytes[offset] != 0 || bytes[offset + LisaFileSystemLayout.CatalogNameOffset] < LisaVolumeHeader.MinimumPrintableCharacter)) continue;
            if (length is 0 or > LisaFileSystemLayout.CatalogNameLength || offset + LisaFileSystemLayout.CatalogNameOffset + length > bytes.Length) continue;
            var name = LisaVolumeHeader.DecodeName(bytes.AsSpan(offset + LisaFileSystemLayout.CatalogNameOffset, length));
            var fileId = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset + LisaFileSystemLayout.CatalogFileIdOffset, sizeof(ushort)));
            if (LisaFileSystemReader.IsUserFile(fileId) && !string.IsNullOrWhiteSpace(name)) result.TryAdd(fileId, name);
        }
        return new(result, []);
    }

    /// <summary>Construit le nom de secours d'un fichier absent du catalogue.</summary>
    public static string FallbackName(ushort fileId) => $"File {fileId:X4}";
}

/// <summary>Résultat de lecture d'un catalogue Lisa.</summary>
/// <param name="Names">Noms indexés par identifiant de fichier.</param>
/// <param name="Warnings">Avertissements produits pendant la lecture.</param>
public sealed record LisaCatalogResult(IReadOnlyDictionary<ushort, string> Names, IReadOnlyList<string> Warnings);
