using System.Collections.Frozen;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Reconstruit les fichiers seedling, sapling et tree sans déplacer leurs blocs creux.</summary>
internal static class ProDosFileContentReader
{
    /// <summary>Lit un fichier selon son type de stockage jusqu'à sa longueur EOF.</summary>
    public static ProDosFileContent Read(SectorImage image, ProDosStorageType storageType, int keyBlock, int length, string fileName, List<string> warnings)
    {
        var requiredBlocks = (length + ProDosFileSystemLayout.BlockSize - 1) / ProDosFileSystemLayout.BlockSize;
        var dataPointers = new List<int>(requiredBlocks);
        var indexBlocks = new HashSet<int>();
        var valid = true;
        if (storageType == ProDosStorageType.Seedling) dataPointers.Add(keyBlock);
        else if (storageType == ProDosStorageType.Sapling) valid &= ReadIndex(image, keyBlock, requiredBlocks, fileName, storageType, dataPointers, indexBlocks, warnings);
        else if (storageType == ProDosStorageType.Tree) valid &= ReadTree(image, keyBlock, requiredBlocks, fileName, storageType, dataPointers, indexBlocks, warnings);
        using var output = new MemoryStream();
        var dataBlocks = new HashSet<int>();
        foreach (var pointer in dataPointers.Take(requiredBlocks))
        {
            if (pointer == 0) output.Write(new byte[ProDosFileSystemLayout.BlockSize]);
            else if (pointer < 0 || pointer >= image.BlockCount || !image.TryGetBlock(pointer, out var block) || block.Data.Count != ProDosFileSystemLayout.BlockSize)
            {
                warnings.Add(ProDosFileSystemExceptions.MissingDataBlock(fileName, storageType, pointer));
                output.Write(new byte[ProDosFileSystemLayout.BlockSize]);
                valid = false;
            }
            else
            {
                dataBlocks.Add(pointer);
                output.Write(block.Data.ToArray());
            }
        }
        if (output.Length < length)
        {
            warnings.Add(ProDosFileSystemExceptions.TruncatedContent(fileName, storageType, output.Length, length));
            valid = false;
        }
        return new(Array.AsReadOnly(output.ToArray().Take(length).ToArray()), valid, dataBlocks.ToFrozenSet(), indexBlocks.ToFrozenSet());
    }

    /// <summary>Lit les pointeurs d'un index sapling en conservant les emplacements nuls.</summary>
    private static bool ReadIndex(SectorImage image, int blockNumber, int pointerCount, string fileName, ProDosStorageType storageType, ICollection<int> output, ISet<int> indexBlocks, ICollection<string> warnings)
    {
        if (!indexBlocks.Add(blockNumber))
        {
            warnings.Add(ProDosFileSystemExceptions.InvalidIndexBlock(fileName, storageType, blockNumber, true));
            AddSparsePointers(output, pointerCount);
            return false;
        }
        if (blockNumber <= 0 || blockNumber >= image.BlockCount || !image.TryGetBlock(blockNumber, out var index) || index.Data.Count != ProDosFileSystemLayout.BlockSize)
        {
            warnings.Add(ProDosFileSystemExceptions.InvalidIndexBlock(fileName, storageType, blockNumber, false));
            AddSparsePointers(output, pointerCount);
            return false;
        }
        for (var pointer = 0; pointer < Math.Min(pointerCount, ProDosFileSystemLayout.IndexPointerCount); pointer++) output.Add(ProDosPrimitives.ReadIndexPointer(index.Data, pointer));
        return true;
    }

    /// <summary>Lit un index maître tree et la plage logique couverte par chacun de ses pointeurs.</summary>
    private static bool ReadTree(SectorImage image, int masterBlock, int requiredBlocks, string fileName, ProDosStorageType storageType, ICollection<int> output, ISet<int> indexBlocks, ICollection<string> warnings)
    {
        if (!indexBlocks.Add(masterBlock) || masterBlock <= 0 || masterBlock >= image.BlockCount || !image.TryGetBlock(masterBlock, out var master) || master.Data.Count != ProDosFileSystemLayout.BlockSize)
        {
            warnings.Add(ProDosFileSystemExceptions.InvalidMasterIndexBlock(fileName, storageType, masterBlock));
            AddSparsePointers(output, requiredBlocks);
            return false;
        }
        var valid = true;
        for (var index = 0; index < ProDosFileSystemLayout.IndexPointerCount && output.Count < requiredBlocks; index++)
        {
            var childCount = Math.Min(ProDosFileSystemLayout.IndexPointerCount, requiredBlocks - output.Count);
            var child = ProDosPrimitives.ReadIndexPointer(master.Data, index);
            if (child == 0) AddSparsePointers(output, childCount);
            else valid &= ReadIndex(image, child, childCount, fileName, storageType, output, indexBlocks, warnings);
        }
        return valid;
    }

    /// <summary>Ajoute les pointeurs nuls représentant une plage creuse valide.</summary>
    private static void AddSparsePointers(ICollection<int> output, int count)
    {
        for (var index = 0; index < count; index++) output.Add(0);
    }
}
