using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Reconnaît le boot et sélectionne le bloc racine AmigaDOS.</summary>
public static class AmigaDosRootBlockReader
{
    /// <summary>Tente de reconnaître le volume puis retourne sa variante et sa racine.</summary>
    public static bool TryRead(SectorImage image, out AmigaDosRootBlock? root)
    {
        root = null;
        if (image.BlockSize != AmigaDosLayout.BlockSize || !image.TryGetBlock(AmigaDosLayout.BootBlock, out var bootBlock) || bootBlock.Data.Count <= AmigaDosLayout.DosVariantOffset) return false;
        var boot = bootBlock.Data.ToArray();
        var signedBoot = HasDosPrefix(boot) && boot[AmigaDosLayout.DosVariantOffset] <= (byte)AmigaDosLayout.MaximumVariant;
        var variant = signedBoot ? (AmigaDosVariant)boot[AmigaDosLayout.DosVariantOffset] : AmigaDosVariant.Ofs;
        var conventionalRoot = image.BlockCount / 2;
        if (!signedBoot)
        {
            if (!TryGetRoot(image, conventionalRoot, out var protectedRoot) || !AmigaDosChecksum.IsValid(protectedRoot)) return false;
            if (HasEmptyHashTable(protectedRoot)) return false;
            root = Create(variant, conventionalRoot, protectedRoot);
            return true;
        }
        var declaredRoot = BigEndianInt32.Read(boot, AmigaDosLayout.BootRootPointerOffset);
        if (TryGetRoot(image, declaredRoot, out var declaredData))
        {
            root = Create(variant, declaredRoot, declaredData);
            return true;
        }
        if (!TryGetRoot(image, conventionalRoot, out var conventionalData)) return false;
        root = Create(variant, conventionalRoot, conventionalData);
        return true;
    }

    /// <summary>Indique si les trois premiers octets portent la signature DOS.</summary>
    public static bool HasDosPrefix(ReadOnlySpan<byte> boot) => boot.Length > AmigaDosLayout.DosVariantOffset && boot[0] == AmigaDosLayout.DosSignatureD && boot[1] == AmigaDosLayout.DosSignatureO && boot[2] == AmigaDosLayout.DosSignatureS;

    private static bool TryGetRoot(SectorImage image, int blockNumber, out byte[] data)
    {
        data = [];
        if (blockNumber <= 0 || blockNumber >= image.BlockCount || !image.TryGetBlock(blockNumber, out var block) || block.Data.Count != AmigaDosLayout.BlockSize) return false;
        data = block.Data.ToArray();
        return BigEndianInt32.Read(data, AmigaDosLayout.PrimaryTypeOffset) == AmigaDosLayout.HeaderPrimaryType && BigEndianInt32.Read(data, AmigaDosLayout.SecondaryTypeOffset) == AmigaDosLayout.RootSecondaryType;
    }

    private static AmigaDosRootBlock Create(AmigaDosVariant variant, int blockNumber, byte[] data)
    {
        var hashTableSize = Math.Clamp(BigEndianInt32.Read(data, AmigaDosLayout.HashTableSizeOffset), 0, AmigaDosLayout.RootHashTableEntryCount);
        return new(variant, blockNumber, data, hashTableSize == 0 ? AmigaDosLayout.RootHashTableEntryCount : hashTableSize);
    }

    /// <summary>Indique que la racine ne référence aucune entrée de répertoire.</summary>
    private static bool HasEmptyHashTable(ReadOnlySpan<byte> root)
    {
        for (var index = 0; index < AmigaDosLayout.RootHashTableEntryCount; index++)
            if (BigEndianInt32.Read(root, AmigaDosLayout.DataPointersOffset + index * AmigaDosLayout.WordSize) != 0) return false;
        return true;
    }
}
