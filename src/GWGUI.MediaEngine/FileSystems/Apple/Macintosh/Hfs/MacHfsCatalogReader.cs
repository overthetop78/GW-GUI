using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;

/// <summary>Valide les nœuds feuilles et lit les records du catalogue HFS.</summary>
internal static class MacHfsCatalogReader
{
    /// <summary>Lit tous les records de catalogue exploitables.</summary>
    public static IReadOnlyList<MacHfsCatalogRecord> Read(IReadOnlyList<byte> catalog, SectorImage image, int allocationStart, uint allocationSize, List<string> warnings)
    {
        if (catalog.Count < MacHfsFileSystemLayout.MinimumCatalogLength) throw MacFileSystemExceptions.TruncatedCatalog(MacHfsFileSystemLayout.SystemName, catalog.Count, MacHfsFileSystemLayout.MinimumCatalogLength);
        var bytes = catalog.ToArray();
        var nodeSize = MacFileSystemPrimitives.ReadUInt16(bytes, MacHfsFileSystemLayout.NodeSizeOffset);
        if (nodeSize is < MacHfsFileSystemLayout.MinimumNodeSize or > MacHfsFileSystemLayout.MaximumNodeSize || bytes.Length < nodeSize) nodeSize = MacHfsFileSystemLayout.DefaultNodeSize;
        var records = new List<MacHfsCatalogRecord>();
        for (var nodeOffset = 0; nodeOffset + nodeSize <= bytes.Length; nodeOffset += nodeSize) ReadNode(bytes.AsSpan(nodeOffset, nodeSize), image, allocationStart, allocationSize, records, warnings);
        if (records.Count == 0) warnings.Add(MacFileSystemExceptions.NoReadableCatalogRecord(MacHfsFileSystemLayout.SystemName));
        return records.AsReadOnly();
    }

    /// <summary>Lit les records d'un nœud feuille dont la table d'offsets est valide.</summary>
    private static void ReadNode(ReadOnlySpan<byte> node, SectorImage image, int allocationStart, uint allocationSize, ICollection<MacHfsCatalogRecord> records, List<string> warnings)
    {
        if (node.Length < MacHfsFileSystemLayout.NodeDescriptorLength || (sbyte)node[MacHfsFileSystemLayout.NodeKindOffset] != MacHfsFileSystemLayout.LeafNodeKind) return;
        var count = MacFileSystemPrimitives.ReadUInt16(node, MacHfsFileSystemLayout.RecordCountOffset);
        if (count > MacHfsFileSystemLayout.MaximumRecordCount || MacHfsFileSystemLayout.NodeDescriptorLength + (count + 1) * MacHfsFileSystemLayout.RecordOffsetLength > node.Length) return;
        for (var index = 0; index < count; index++)
        {
            var startOffset = node.Length - MacHfsFileSystemLayout.RecordOffsetLength * (index + 1);
            var endOffset = node.Length - MacHfsFileSystemLayout.RecordOffsetLength * (index + 2);
            if (endOffset < 0) continue;
            var start = MacFileSystemPrimitives.ReadUInt16(node, startOffset);
            var end = MacFileSystemPrimitives.ReadUInt16(node, endOffset);
            if (start < MacHfsFileSystemLayout.NodeDescriptorLength || end <= start || end > node.Length) continue;
            ReadRecord(node, start, end, image, allocationStart, allocationSize, records, warnings);
        }
    }

    /// <summary>Décode une clé puis son record dossier ou fichier.</summary>
    private static void ReadRecord(ReadOnlySpan<byte> node, int start, int end, SectorImage image, int allocationStart, uint allocationSize, ICollection<MacHfsCatalogRecord> records, List<string> warnings)
    {
        var keyLength = node[start];
        if (keyLength < MacHfsFileSystemLayout.MinimumKeyLength || start + 1 + keyLength > end) return;
        var keyOffset = start + 1;
        var parentId = MacFileSystemPrimitives.ReadUInt32(node, keyOffset + MacHfsFileSystemLayout.ParentIdOffset);
        var nameLength = node[keyOffset + MacHfsFileSystemLayout.NameLengthOffset];
        if (nameLength > MacHfsFileSystemLayout.MaximumNameLength || keyOffset + MacHfsFileSystemLayout.NameOffset + nameLength > end) return;
        var name = MacFileSystemPrimitives.DecodeName(node.Slice(keyOffset + MacHfsFileSystemLayout.NameOffset, nameLength));
        var dataOffset = start + 1 + keyLength;
        if (dataOffset % MacHfsFileSystemLayout.RecordAlignment != 0) dataOffset++;
        if (dataOffset >= end) return;
        if (node[dataOffset] == MacHfsFileSystemLayout.DirectoryRecordType && dataOffset + MacHfsFileSystemLayout.MinimumDirectoryRecordLength <= end) ReadDirectory(node, dataOffset, parentId, name, records);
        else if (node[dataOffset] == MacHfsFileSystemLayout.FileRecordType && dataOffset + MacHfsFileSystemLayout.MinimumFileRecordLength <= end) ReadFile(node, dataOffset, parentId, name, image, allocationStart, allocationSize, records, warnings);
    }

    /// <summary>Crée un record dossier.</summary>
    private static void ReadDirectory(ReadOnlySpan<byte> node, int offset, uint parentId, string name, ICollection<MacHfsCatalogRecord> records)
    {
        var id = MacFileSystemPrimitives.ReadUInt32(node, offset + MacHfsFileSystemLayout.DirectoryIdOffset);
        var modified = MacFileSystemTime.FromSeconds(MacFileSystemPrimitives.ReadUInt32(node, offset + MacHfsFileSystemLayout.DirectoryModifiedOffset));
        records.Add(new(parentId, id, name, true, 0, modified, MacHfsFileSystemLayout.DirectoryDescription, [], [], true));
    }

    /// <summary>Crée un record fichier et lit distinctement ses deux forks.</summary>
    private static void ReadFile(ReadOnlySpan<byte> node, int offset, uint parentId, string name, SectorImage image, int allocationStart, uint allocationSize, ICollection<MacHfsCatalogRecord> records, List<string> warnings)
    {
        var dataLength = MacFileSystemPrimitives.ReadUInt32(node, offset + MacHfsFileSystemLayout.DataForkLengthOffset);
        var resourceLength = MacFileSystemPrimitives.ReadUInt32(node, offset + MacHfsFileSystemLayout.ResourceForkLengthOffset);
        var dataFork = MacHfsExtentReader.Read(image, node.Slice(offset + MacHfsFileSystemLayout.DataForkExtentsOffset, MacHfsFileSystemLayout.EmbeddedExtentsLength), allocationStart, allocationSize, dataLength);
        var resourceFork = MacHfsExtentReader.Read(image, node.Slice(offset + MacHfsFileSystemLayout.ResourceForkExtentsOffset, MacHfsFileSystemLayout.EmbeddedExtentsLength), allocationStart, allocationSize, resourceLength);
        AddExtentWarnings(name, MacHfsFileSystemLayout.DataForkName, dataLength, dataFork, warnings);
        AddExtentWarnings(name, MacHfsFileSystemLayout.ResourceForkName, resourceLength, resourceFork, warnings);
        var finderType = System.Text.Encoding.ASCII.GetString(node.Slice(offset + MacHfsFileSystemLayout.FinderTypeOffset, MacHfsFileSystemLayout.FinderTypeLength)).Trim('\0', ' ');
        var type = string.IsNullOrWhiteSpace(finderType) ? MacHfsFileSystemLayout.DefaultFileDescription : finderType;
        var id = MacFileSystemPrimitives.ReadUInt32(node, offset + MacHfsFileSystemLayout.FileIdOffset);
        var modified = MacFileSystemTime.FromSeconds(MacFileSystemPrimitives.ReadUInt32(node, offset + MacHfsFileSystemLayout.FileModifiedOffset));
        records.Add(new(parentId, id, name, false, (long)dataLength + resourceLength, modified, type, dataFork.Content, resourceFork.Content, dataFork.IsValid && resourceFork.IsValid));
    }

    /// <summary>Ajoute les avertissements distinguant blocs absents et extents supplémentaires.</summary>
    private static void AddExtentWarnings(string file, string fork, uint expectedLength, MacHfsExtentResult result, ICollection<string> warnings)
    {
        foreach (var block in result.MissingBlocks) warnings.Add(MacFileSystemExceptions.MissingBlock(file, fork, block));
        if (result.RemainingLength > 0) warnings.Add(MacFileSystemExceptions.IncompleteData(file, fork, expectedLength - result.RemainingLength, expectedLength));
    }
}
