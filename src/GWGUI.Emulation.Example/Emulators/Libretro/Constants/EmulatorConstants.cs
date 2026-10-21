namespace GWGUI.Emulation.Example.Emulators.Libretro.Constants;

/// <summary>Identité de l’émulateur fictif ExampleLibretro, distincte des deux modèles qu’il peut exécuter.</summary>
internal static class EmulatorConstants
{
    /// <summary>Identifiant stable enregistré lors du choix de l’émulateur.</summary>
    internal const string Id = "example-libretro";
    /// <summary>Nom technique invariant de ce cœur de démonstration.</summary>
    internal const string DisplayName = "ExampleLibretro";
    /// <summary>Clé de description résolue par les ressources communes du module.</summary>
    internal const string DescriptionResourceKey = "Emulation.Family.Example";
}
