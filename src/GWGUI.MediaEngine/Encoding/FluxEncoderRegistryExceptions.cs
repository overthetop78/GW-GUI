namespace GWGUI.MediaEngine.Encoding;

/// <summary>Construit les erreurs propres à l'enregistrement et à la recherche des encodeurs de pistes.</summary>
internal static class FluxEncoderRegistryExceptions
{
    /// <summary>Crée l'erreur signalant une entrée nulle dans la collection d'encodeurs.</summary>
    /// <param name="index">Position de l'entrée nulle.</param>
    /// <returns>Erreur contenant la position invalide.</returns>
    public static ArgumentException NullEncoder(int index) => new($"The flux encoder at index {index} is null.", "encoders");

    /// <summary>Crée l'erreur signalant un identifiant d'encodeur vide.</summary>
    /// <param name="index">Position de l'encodeur portant l'identifiant vide.</param>
    /// <returns>Erreur contenant la position invalide.</returns>
    public static ArgumentException EmptyEncoderId(int index) => new($"The flux encoder at index {index} has an empty identifier.", "encoders");

    /// <summary>Crée l'erreur signalant un identifiant d'encodeur enregistré plusieurs fois.</summary>
    /// <param name="id">Identifiant technique dupliqué.</param>
    /// <returns>Erreur décrivant le doublon.</returns>
    public static ArgumentException DuplicateEncoder(string id) => new($"The flux encoder identifier '{id}' is registered more than once.", nameof(id));

    /// <summary>Crée l'erreur signalant qu'aucun encodeur ne correspond à l'identifiant demandé.</summary>
    /// <param name="id">Identifiant technique demandé.</param>
    /// <returns>Erreur décrivant l'identifiant absent.</returns>
    public static KeyNotFoundException EncoderNotFound(string id) => new($"No flux encoder is registered with the identifier '{id}'.");
}
