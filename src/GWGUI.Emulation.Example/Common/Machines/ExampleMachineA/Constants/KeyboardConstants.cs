namespace GWGUI.Emulation.Example.Common.Machines.ExampleMachineA.Constants;

/// <summary>Définit les touches particulières des profils de cette famille d’exemple et leurs libellés traduits.</summary>
/// <remarks>Les identifiants EmulationKey sont ceux du contrat existant : ils décrivent les touches émulées.
/// Leur nom est indépendant du nom générique choisi pour présenter les machines de cet exemple.</remarks>
internal static class ExampleMachineAKeyboardConstants
{
    /// <summary>Touches supplémentaires communes aux profils : fonction F0, copie et verrouillage des majuscules.</summary>
    internal static readonly IReadOnlyList<EmulationKey> SpecialKeys = [EmulationKey.AcornF0, EmulationKey.AcornCopy, EmulationKey.AcornShiftLock];

    /// <summary>Touches du profil C, comprenant les touches communes puis trois touches de son pavé numérique.</summary>
    internal static readonly IReadOnlyList<EmulationKey> ModelCSpecialKeys = [.. SpecialKeys, EmulationKey.AcornKeypadHash, EmulationKey.AcornKeypadDelete, EmulationKey.AcornKeypadComma];

    /// <summary>Clé de traduction de la touche de fonction F0.</summary>
    internal const string F0Resource = "Emulation.Example.Key.F0";
    /// <summary>Clé de traduction de la touche de copie.</summary>
    internal const string CopyResource = "Emulation.Example.Key.Copy";
    /// <summary>Clé de traduction du verrouillage des majuscules.</summary>
    internal const string ShiftLockResource = "Emulation.Example.Key.ShiftLock";
    /// <summary>Clé de traduction du dièse du pavé numérique.</summary>
    internal const string KeypadHashResource = "Emulation.Example.Key.KeypadHash";
    /// <summary>Clé de traduction de la suppression du pavé numérique.</summary>
    internal const string KeypadDeleteResource = "Emulation.Example.Key.KeypadDelete";
    /// <summary>Clé de traduction de la virgule du pavé numérique.</summary>
    internal const string KeypadCommaResource = "Emulation.Example.Key.KeypadComma";
}
