namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Identités stables des trois voies de simulation du cœur.</summary>
public enum Emulator
{
    /// <summary>Aucun émulateur encore sélectionné dans la configuration.</summary>
    None = -1,
    /// <summary>Cœur simulant les échanges de callbacks et de données libretro.</summary>
    Libretro = 0,
    /// <summary>Émulateur simulant les exports d’une DLL, sans créer ni charger de DLL externe.</summary>
    External = 1,
    /// <summary>Cœur exécutant sa simulation directement dans le module.</summary>
    Internal = 2,
}
