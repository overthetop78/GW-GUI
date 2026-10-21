namespace GWGUI.Emulation.Example.Common.Machines.ExampleMachineA.Constants;

/// <summary>Rassemble les identités et valeurs matérielles utilisées pour décrire les profils d’exemple A à E.</summary>
/// <remarks>Les identifiants stables correspondent aux membres de MachineModel ; les clés de ressources servent uniquement à leur présentation.
/// Les valeurs mémoire sont exprimées en octets, même lorsque leur nom indique une quantité en Kio.</remarks>
internal static class ExampleMachineAModelConstants
{
    /// <summary>Identifiant stable du profil A à 16 Kio.</summary>
    internal const string ModelAId = nameof(MachineModel.ExampleMachineA);
    /// <summary>Identifiant stable du profil B à 32 Kio.</summary>
    internal const string ModelBId = nameof(MachineModel.ExampleMachineB);
    /// <summary>Identifiant stable du profil D à 64 Kio.</summary>
    internal const string ModelDId = nameof(MachineModel.ExampleMachineD);
    /// <summary>Identifiant stable du profil E à 128 Kio et processeur 6512.</summary>
    internal const string ModelEId = nameof(MachineModel.ExampleMachineE);
    /// <summary>Identifiant stable du profil C à 128 Kio et processeur 65C12.</summary>
    internal const string ModelCId = nameof(MachineModel.ExampleMachineC);
    /// <summary>Clé de traduction du nom affiché pour A.</summary>
    internal const string ModelADisplayNameResource = "Emulation.Example.Model.ExampleMachineA";
    /// <summary>Clé de traduction du nom affiché pour B.</summary>
    internal const string ModelBDisplayNameResource = "Emulation.Example.Model.ExampleMachineB";
    /// <summary>Clé de traduction du nom affiché pour D.</summary>
    internal const string ModelDDisplayNameResource = "Emulation.Example.Model.ExampleMachineD";
    /// <summary>Clé de traduction du nom affiché pour E.</summary>
    internal const string ModelEDisplayNameResource = "Emulation.Example.Model.ExampleMachineE";
    /// <summary>Clé de traduction du nom affiché pour C.</summary>
    internal const string ModelCDisplayNameResource = "Emulation.Example.Model.ExampleMachineC";
    /// <summary>Fréquence CPU nominale commune à ces profils, en hertz : 2 MHz.</summary>
    internal const long CpuFrequencyHz = 2_000_000;
    /// <summary>Capacité de 16 Kio exprimée en octets.</summary>
    internal const long SixteenKibibytes = 16 * 1024;
    /// <summary>Capacité de 32 Kio exprimée en octets.</summary>
    internal const long ThirtyTwoKibibytes = 32 * 1024;
    /// <summary>Capacité de 64 Kio exprimée en octets.</summary>
    internal const long SixtyFourKibibytes = 64 * 1024;
    /// <summary>Capacité de 128 Kio exprimée en octets.</summary>
    internal const long OneHundredTwentyEightKibibytes = 128 * 1024;
    /// <summary>Nombre de ports pour la connexion du clavier.</summary>
    internal const int OnePort = 1;
    /// <summary>Nombre de ports de joystick analogique des profils équipés.</summary>
    internal const int TwoPorts = 2;
}
