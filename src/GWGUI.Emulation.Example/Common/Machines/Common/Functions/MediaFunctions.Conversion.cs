namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Conversion des catégories de contenus entre le contrat public et les descriptions du module.</summary>
/// <remarks>Seules les descriptions sont converties. Les images et fichiers ne sont ni écrits ni déplacés.</remarks>
internal static class EmulationMediaConversionFunctions
{
    /// <summary>Projette un contenu du module vers le contrat de l’hôte.</summary>
    /// <param name="media">Description stable contenant chemin, lecteur et protection.</param>
    /// <returns>Description publique conservant le montage et son état d’insertion.</returns>
    internal static EmulationMedia ToCommon(MediaConfiguration media) => new(media.Path, media.Slot, ToType(media.Category), media.IsReadOnly, media.IsInserted);

    /// <summary>Applique une description publique en conservant les métadonnées du montage existant.</summary>
    /// <param name="media">Contenu fourni par l’hôte, dont le fichier reste emprunté.</param>
    /// <param name="mountedMedia">Montages courants dans lesquels rechercher le lecteur.</param>
    /// <returns>Description interne du contenu à monter dans le lecteur.</returns>
    /// <remarks>Le libellé, le bus, le point et l’ordre de montage ainsi que les paramètres de cartouche
    /// et de cassette restent portés par la copie with. Une catégorie interne plus précise est conservée
    /// lorsque sa projection correspond encore au même type public.</remarks>
    internal static MediaConfiguration ToModule(EmulationMedia media, IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        var existing = mountedMedia.FirstOrDefault(item => item.Slot == media.Slot);
        return existing is null ? new(media.Path, ToCategory(media.Type), media.Slot, IsReadOnly: media.IsReadOnly, IsInserted: media.IsInserted)
            : existing with { Path = media.Path, Category = ToType(existing.Category) == media.Type ? existing.Category : ToCategory(media.Type), IsReadOnly = media.IsReadOnly, IsInserted = media.IsInserted };
    }

    /// <summary>Convertit le type interne sans dépendre des valeurs numériques des deux énumérations.</summary>
    /// <param name="category">Type de contenu déclaré par la configuration.</param>
    /// <returns>Type public correspondant.</returns>
    internal static EmulationMediaType ToType(MediaCategory category) => category switch
    {
        MediaCategory.Floppy or MediaCategory.Snapshot => EmulationMediaType.Floppy,
        MediaCategory.Cassette => EmulationMediaType.Cassette,
        MediaCategory.HardDisk or MediaCategory.Directory => EmulationMediaType.HardDisk,
        MediaCategory.Cartridge => EmulationMediaType.Cartridge,
        MediaCategory.MemoryCard => EmulationMediaType.MemoryCard,
        MediaCategory.CompactDisc => EmulationMediaType.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
    };

    /// <summary>Convertit le type public vers le type décrit par les profils fictifs.</summary>
    /// <param name="type">Type annoncé dans le contrat de média.</param>
    /// <returns>Type interne correspondant ; un type absent des profils est refusé.</returns>
    internal static MediaCategory ToCategory(EmulationMediaType type) => type switch
    {
        EmulationMediaType.Floppy => MediaCategory.Floppy,
        EmulationMediaType.Cassette => MediaCategory.Cassette,
        EmulationMediaType.HardDisk => MediaCategory.HardDisk,
        EmulationMediaType.Cartridge => MediaCategory.Cartridge,
        EmulationMediaType.MemoryCard => MediaCategory.MemoryCard,
        EmulationMediaType.CompactDisc => MediaCategory.CompactDisc,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
