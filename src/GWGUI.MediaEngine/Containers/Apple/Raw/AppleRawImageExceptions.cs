namespace GWGUI.MediaEngine.Containers.Apple.Raw;

/// <summary>Construit les erreurs des représentations Apple brutes.</summary>
internal static class AppleRawImageExceptions
{
    /// <summary>Crée l'erreur signalant une capacité absente du catalogue.</summary>
    public static InvalidDataException UnsupportedLayout(int length, string extension, IEnumerable<string> probes) => new($"Apple raw image length {length} with extension '{extension}' is unsupported after probes: {string.Join(", ", probes)}.");
    /// <summary>Crée l'erreur signalant une capacité connue qu'aucune structure ne permet d'interpréter.</summary>
    public static InvalidDataException KnownCapacityWithoutStructure(int length, string extension, IEnumerable<string> probes) => new($"Apple raw image length {length} is known, but extension '{extension}' and probes [{string.Join(", ", probes)}] do not identify its structure.");
}
