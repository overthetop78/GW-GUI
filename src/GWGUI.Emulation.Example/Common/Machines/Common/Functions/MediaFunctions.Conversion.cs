namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Conversion des descriptions de montage entre les contrats du module et ceux de l’hôte.</summary>
/// <remarks>Ces projections manipulent des chemins et des valeurs ; elles n’ouvrent aucun fichier média.</remarks>
internal static class EmulationMediaConversionFunctions
{
    /// <summary>Projette un montage interne vers le contrat commun de média.</summary>
    /// <param name="media">Montage emprunté dont sont repris le chemin, le slot et les protections.</param>
    /// <returns>Nouveau média SDK avec le type correspondant à la catégorie interne.</returns>
    internal static EmulationMedia ToCommon(MediaConfiguration media) => new(media.Path, media.Slot, ToType(media.Category), media.IsReadOnly, media.IsInserted);

    /// <summary>Convertit un montage reçu de l’hôte en conservant les données internes du montage existant.</summary>
    /// <param name="media">Média commun à monter ou à mettre à jour.</param>
    /// <param name="mountedMedia">Liste empruntée dans laquelle est recherché le premier montage du même slot.</param>
    /// <returns>Nouveau montage ; le libellé existant est conservé lorsqu’un montage de ce slot est déjà présent.</returns>
    /// <remarks>Le chemin, la catégorie, la protection et l’insertion sont remplacés par les valeurs reçues.</remarks>
    internal static MediaConfiguration ToAcorn(EmulationMedia media, IReadOnlyList<MediaConfiguration> mountedMedia)
    {
        var category = ToAcornCategory(media.Type);
        var existing = mountedMedia.FirstOrDefault(item => item.Slot == media.Slot);
        return existing is null
            ? new MediaConfiguration(media.Path, category, media.Slot, IsReadOnly: media.IsReadOnly, IsInserted: media.IsInserted)
            : existing with
            {
                Path = media.Path,
                Category = category,
                IsReadOnly = media.IsReadOnly,
                IsInserted = media.IsInserted
            };
    }

    /// <summary>Convertit une catégorie interne de média vers le type public correspondant.</summary>
    /// <param name="category">Catégorie disquette, cassette ou cartouche.</param>
    /// <returns>Type SDK de même fonction.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La catégorie n’a pas de correspondance déclarée.</exception>
    private static EmulationMediaType ToType(MediaCategory category) => category switch
    {
        MediaCategory.Floppy => EmulationMediaType.Floppy,
        MediaCategory.Cassette => EmulationMediaType.Cassette,
        MediaCategory.Cartridge => EmulationMediaType.Cartridge,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
    };

    /// <summary>Convertit un type public de média vers sa catégorie interne.</summary>
    /// <param name="type">Type disquette, cassette ou cartouche reçu de l’hôte.</param>
    /// <returns>Catégorie interne utilisée par la configuration.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Le type SDK n’est pas représenté par ce parcours.</exception>
    private static MediaCategory ToAcornCategory(EmulationMediaType type) => type switch
    {
        EmulationMediaType.Floppy => MediaCategory.Floppy,
        EmulationMediaType.Cassette => MediaCategory.Cassette,
        EmulationMediaType.Cartridge => MediaCategory.Cartridge,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
