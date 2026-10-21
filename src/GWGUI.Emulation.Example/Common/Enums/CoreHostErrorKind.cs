namespace GWGUI.Emulation.Example.Common.Enums;

/// <summary>Nature d’un échec transporté par l’hôte de cœur, codée sur un octet.</summary>
/// <remarks>Le récepteur peut traiter une demande de média séparément d’un échec général.
/// Cette distinction complète le diagnostic et conserve les mêmes valeurs des deux côtés du transport.</remarks>
internal enum CoreHostErrorKind : byte
{
    /// <summary>Échec général à restituer avec son message et sa cause éventuelle.</summary>
    General,
    /// <summary>Le cœur demande un média avant de pouvoir poursuivre son chargement.</summary>
    MediaRequired
}
