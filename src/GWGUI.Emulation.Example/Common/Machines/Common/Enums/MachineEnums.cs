namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Familles regroupant les profils matériels du module.</summary>
public enum MachineFamily
{
    /// <summary>Groupe d’exemple dont le catalogue spécifique fournit les profils ExampleMachineA à ExampleMachineE.</summary>
    ExampleMachineA
}

/// <summary>Identités des profils matériels utilisés pour présenter des configurations distinctes.</summary>
/// <remarks>Les noms deviennent les identifiants publics de modèles ; leurs capacités sont décrites dans le catalogue.
/// Les profils conservent des différences de mémoire, processeur, ports, médias et firmware.</remarks>
public enum MachineModel
{
    /// <summary>Profil à 16 Kio de mémoire, avec clavier et cassette.</summary>
    ExampleMachineA,
    /// <summary>Profil à 32 Kio de mémoire, avec clavier, joysticks analogiques, disquettes et cassette.</summary>
    ExampleMachineB,
    /// <summary>Profil à 128 Kio avec processeur 65C12, clavier étendu et support des cartouches.</summary>
    ExampleMachineC,
    /// <summary>Profil à 64 Kio utilisant le processeur 6512 et les lecteurs de disquettes et cassette.</summary>
    ExampleMachineD,
    /// <summary>Profil à 128 Kio utilisant le processeur 6512 et les lecteurs de disquettes et cassette.</summary>
    ExampleMachineE
}
