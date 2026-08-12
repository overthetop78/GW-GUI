using GWGUI.MediaEngine.Definitions;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;


namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Lit les volumes AmigaDOS OFS et FFS ainsi que leurs variantes.</summary>
public sealed class AmigaDosFileSystemReader : IFileSystemReader
{
    /// <summary>Identifiant technique du lecteur AmigaDOS.</summary>
    public string Id => Definitions.FileSystemIds.AmigaDos;
    /// <summary>Formats d'images sectorielles pris en charge.</summary>
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { DiskImageFormatIds.AmigaDos, DiskImageFormatIds.AmigaDosHighDensity }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Indique si l'image contient un volume AmigaDOS plausible.</summary>
    /// <param name="image">Image sectorielle à examiner.</param>
    /// <returns><see langword="true"/> si un volume AmigaDOS est reconnu.</returns>
    public bool CanRead(SectorImage image) => AmigaDosRootBlockReader.TryRead(image, out _);

    /// <summary>Lit le volume AmigaDOS contenu dans l'image.</summary>
    /// <param name="image">Image sectorielle à lire.</param>
    /// <returns>Volume et entrées reconstruits.</returns>
    /// <exception cref="InvalidDataException">Le boot, la racine ou un bloc indispensable est invalide.</exception>
    public FileSystemVolume Read(SectorImage image)
    {
        if (image.TryGetBlock(AmigaDosLayout.BootBlock, out var bootBlock) && AmigaDosRootBlockReader.HasDosPrefix(bootBlock.Data.ToArray()) && bootBlock.Data[AmigaDosLayout.DosVariantOffset] > (byte)AmigaDosLayout.MaximumVariant) throw AmigaDosExceptions.UnsupportedBootVariant(bootBlock.Data[AmigaDosLayout.DosVariantOffset]);
        if (!AmigaDosRootBlockReader.TryRead(image, out var rootResult) || rootResult is null)
        {
            if (image.TryGetBlock(AmigaDosLayout.BootBlock, out bootBlock) && AmigaDosRootBlockReader.HasDosPrefix(bootBlock.Data.ToArray()))
            {
                var declaredRoot = BigEndianInt32.Read(bootBlock.Data.ToArray(), AmigaDosLayout.BootRootPointerOffset);
                throw AmigaDosExceptions.InvalidRootBlock(declaredRoot > 0 ? declaredRoot : image.BlockCount / 2);
            }
            throw AmigaDosExceptions.UnsupportedBoot();
        }
        var warnings = new List<string>();
        var root = rootResult.Data;
        if (!AmigaDosChecksum.IsValid(root)) warnings.Add(AmigaDosWarnings.InvalidRootChecksum(rootResult.BlockNumber));
        var visited = new HashSet<int> { rootResult.BlockNumber };
        var entries = AmigaDosDirectoryReader.Read(image, root, rootResult.HashTableSize, rootResult.Variant, visited, warnings, 0);
        var freeBlocks = AmigaDosBitmapReader.CountFreeBlocks(image, root, warnings);
        return new(AmigaDosNameCodec.Read(root, AmigaDosLayout.OrdinaryNameOffset, AmigaDosLayout.OrdinaryNameMaximumLength), rootResult.Variant.FileSystemId(), image.Capacity, (long)freeBlocks * AmigaDosLayout.BlockSize, AmigaDosTime.Read(root, AmigaDosLayout.DateOffset), AmigaDosTime.Read(root, AmigaDosLayout.VolumeModifiedDateOffset), entries, warnings);
    }

}
