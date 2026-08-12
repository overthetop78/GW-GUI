namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Reconstruit positionnellement les données d'un inode COHERENT depuis ses pointeurs directs et indirects.</summary>
internal static class CoherentFileDataReader
{
    /// <summary>Lit le contenu logique et indique si tous les blocs nécessaires étaient valides et présents.</summary>
    public static CoherentFileData Read(CoherentImageData image, CoherentInode inode, List<string> warnings, string name)
    {
        if (inode.Size > int.MaxValue) throw CoherentExceptions.FileTooLarge(inode.Size);
        if (inode.Size == 0) return new([], true);
        var requiredBlocks = checked(((int)inode.Size + CoherentFileSystemLayout.BlockSize - 1) / CoherentFileSystemLayout.BlockSize);
        var blocks = new List<int>(requiredBlocks);
        var valid = true;
        for (var index = 0; index < CoherentFileSystemLayout.DirectPointerCount && blocks.Count < requiredBlocks; index++) blocks.Add(inode.Blocks[index]);
        var indirectBlocks = new HashSet<int>();
        AddIndirect(image, inode.Blocks[CoherentFileSystemLayout.SingleIndirectPointerIndex], 1, blocks, requiredBlocks, indirectBlocks, warnings, name, ref valid);
        AddIndirect(image, inode.Blocks[CoherentFileSystemLayout.DoubleIndirectPointerIndex], 2, blocks, requiredBlocks, indirectBlocks, warnings, name, ref valid);
        AddIndirect(image, inode.Blocks[CoherentFileSystemLayout.TripleIndirectPointerIndex], 3, blocks, requiredBlocks, indirectBlocks, warnings, name, ref valid);
        var result = new byte[checked((int)inode.Size)];
        var destination = 0;
        var missingBytes = 0;
        foreach (var block in blocks)
        {
            if (destination >= result.Length) break;
            var count = Math.Min(CoherentFileSystemLayout.BlockSize, result.Length - destination);
            if (block != 0)
            {
                if (block < 0 || block >= image.BlockCount || !image.IsBlockPresent(block))
                {
                    warnings.Add(CoherentWarnings.DirectBlockUnavailable(name, block));
                    missingBytes += count;
                    valid = false;
                }
                else image.Bytes.AsSpan(block * CoherentFileSystemLayout.BlockSize, count).CopyTo(result.AsSpan(destination));
            }
            destination += count;
        }
        if (destination < result.Length)
        {
            missingBytes += result.Length - destination;
            valid = false;
        }
        if (missingBytes > 0) warnings.Add(CoherentWarnings.MissingBytes(name, missingBytes));
        return new(result, valid);
    }

    /// <summary>Ajoute les positions logiques décrites par un pointeur indirect en conservant les trous.</summary>
    private static void AddIndirect(CoherentImageData image, int block, int level, List<int> result, int requiredBlocks, HashSet<int> visited, List<string> warnings, string name, ref bool valid)
    {
        if (result.Count >= requiredBlocks) return;
        if (block == 0)
        {
            AddHoles(result, requiredBlocks, Capacity(level));
            return;
        }
        if (block < 0 || block >= image.BlockCount || !image.IsBlockPresent(block))
        {
            warnings.Add(CoherentWarnings.IndirectBlockUnavailable(name, block, level));
            AddHoles(result, requiredBlocks, Capacity(level));
            valid = false;
            return;
        }
        if (!visited.Add(block))
        {
            warnings.Add(CoherentWarnings.IndirectBlockRepeated(name, block, level));
            AddHoles(result, requiredBlocks, Capacity(level));
            valid = false;
            return;
        }
        var offset = block * CoherentFileSystemLayout.BlockSize;
        for (var index = 0; index < CoherentFileSystemLayout.IndirectPointersPerBlock && result.Count < requiredBlocks; index++)
        {
            var rawChild = CoherentFormat.ReadCanonicalUInt32(image.Bytes.AsSpan(offset + index * CoherentFormat.UInt32Length, CoherentFormat.UInt32Length));
            if (rawChild > int.MaxValue)
            {
                warnings.Add(CoherentWarnings.IndirectBlockUnavailable(name, rawChild, level));
                AddHoles(result, requiredBlocks, Capacity(level - 1));
                valid = false;
                continue;
            }
            var child = (int)rawChild;
            if (level == 1) result.Add(child); else AddIndirect(image, child, level - 1, result, requiredBlocks, visited, warnings, name, ref valid);
        }
    }

    /// <summary>Calcule le nombre de blocs de données couverts par un pointeur indirect du niveau donné.</summary>
    private static int Capacity(int level)
    {
        var capacity = 1;
        for (var index = 0; index < level; index++) capacity = checked(capacity * CoherentFileSystemLayout.IndirectPointersPerBlock);
        return capacity;
    }

    /// <summary>Ajoute le nombre demandé de positions vides sans dépasser la taille logique du fichier.</summary>
    private static void AddHoles(List<int> result, int requiredBlocks, int count)
    {
        while (count-- > 0 && result.Count < requiredBlocks) result.Add(0);
    }
}
