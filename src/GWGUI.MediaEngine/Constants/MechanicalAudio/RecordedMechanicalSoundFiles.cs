using System.Collections.Generic;
using System.Linq;
using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Constants;

/// <summary>Catalogues typés des enregistrements mécaniques, regroupés par type et matériel.</summary>
internal static partial class RecordedMechanicalSoundFiles
{
    /// <summary>Catalogues des trois catégories de lecteurs enregistrées.</summary>
    internal static IReadOnlyList<IReadOnlyList<RecordedMechanicalSoundProfile>> Catalogs { get; } =
    [
        Floppy.Catalogs,
        HardDisk.Catalogs,
        Optical.Catalogs,
    ];

    /// <summary>Définitions audio utilisées directement par le lecteur, sans catalogue externe.</summary>
    internal static RecordedMechanicalSoundCatalog All { get; } = new(Catalogs.SelectMany(profiles => profiles).ToArray());

}
