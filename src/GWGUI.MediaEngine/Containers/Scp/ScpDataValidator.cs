namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Valide qu’une section SCP appartient entièrement aux données disponibles.</summary>
public static class ScpDataValidator
{
    /// <summary>Vérifie la position et la longueur d’une section SCP.</summary>
    /// <param name="data">Données complètes du conteneur.</param>
    /// <param name="offset">Position de début de la section, en octets.</param>
    /// <param name="length">Longueur requise de la section, en octets.</param>
    /// <param name="section">Section SCP contrôlée.</param>
    /// <param name="trackNumber">Numéro de piste éventuel.</param>
    /// <param name="revolutionNumber">Numéro de révolution éventuel basé sur un.</param>
    /// <exception cref="InvalidDataException">La position ou la longueur est négative, ou la section dépasse les données disponibles.</exception>
    public static void Require(ReadOnlySpan<byte> data, int offset, int length, ScpSection section, int? trackNumber = null, int? revolutionNumber = null)
    {
        if (offset < 0 || length < 0 || offset > data.Length - length) throw ScpExceptions.IncompleteSection(section, offset, length, trackNumber, revolutionNumber);
    }
}
