namespace GWGUI.Emulation.Example.Common.Machines.ExampleMachineB.Constants;

/// <summary>Identité et valeurs de démonstration du profil matériel B.</summary>
/// <remarks>Ces valeurs appartiennent à ce profil fictif ; elles ne décrivent aucune machine réelle.</remarks>
internal static class ModelConstants
{
    /// <summary>Identifiant technique enregistré dans les documents et annoncé aux émulateurs.</summary>
    internal const string Id = "example-machine-b";
    /// <summary>Clé du nom de ce profil dans les ressources du module.</summary>
    internal const string DisplayNameResourceKey = "Emulation.Example.Machine.B";
    /// <summary>Fréquence de démonstration du processeur fictif, en hertz.</summary>
    internal const long DefaultCpuFrequencyHz = 1000000;
    /// <summary>Capacité RAM de démonstration, en octets.</summary>
    internal const long MainMemoryBytes = 2097152;
}
