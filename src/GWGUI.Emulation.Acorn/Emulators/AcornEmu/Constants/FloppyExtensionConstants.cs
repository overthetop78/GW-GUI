namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Options et ressources des extensions disquette, communes aux deux lecteurs de la machine.</summary>
internal static class FloppyExtensionConstants
{
    /// <summary>Champ persistant de la ROM destinée au contrôleur 8271.</summary>
    internal const string IntelRom = StorageConstants.FloppyPrefix + "rom8271";
    /// <summary>Champ persistant de la ROM destinée au contrôleur WD1770.</summary>
    internal const string WdRom = StorageConstants.FloppyPrefix + "rom1770";
    /// <summary>Activation de la carte double Solidisk.</summary>
    internal const string Solidisk = StorageConstants.FloppyPrefix + "solidisk";
    /// <summary>Identifiant et titre traduit du cadre d'extension.</summary>
    internal const string Group = "Emulation.Acorn.Extensions.Floppy";
    /// <summary>Libellé invariant du champ ROM 8271.</summary>
    internal const string IntelRomLabel = "Emulation.Acorn.Extensions.Rom8271";
    /// <summary>Libellé invariant du champ ROM WD1770.</summary>
    internal const string WdRomLabel = "Emulation.Acorn.Extensions.Rom1770";
    /// <summary>Libellé invariant du nom de la carte double.</summary>
    internal const string SolidiskLabel = "Emulation.Acorn.Extensions.Solidisk";
    /// <summary>Nom invariant utilisé dans le diagnostic de ROM 8271 manquante.</summary>
    internal const string IntelRomName = "8271";
    /// <summary>Nom invariant utilisé dans le diagnostic de ROM WD1770 manquante.</summary>
    internal const string WdRomName = "WD1770";
}
