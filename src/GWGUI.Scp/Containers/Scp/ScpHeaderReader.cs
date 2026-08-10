using GWGUI.Scp;

namespace GWGUI.Scp.Containers.Scp;

/// <summary>
/// Fournit un point d'entrée spécialisé pour lire uniquement l'en-tête fixe d'un conteneur SCP.
/// </summary>
public static class ScpHeaderReader
{
    /// <summary>
    /// Valide et interprète les seize octets de l'en-tête fixe SCP.
    /// </summary>
    /// <param name="data">Données commençant au premier octet du conteneur SCP.</param>
    /// <returns>En-tête SCP interprété.</returns>
    /// <exception cref="InvalidDataException">L'en-tête est incomplet ou contient une valeur invalide.</exception>
    /// <exception cref="NotSupportedException">La largeur de cellule de bit déclarée n'est pas prise en charge.</exception>
    public static ScpHeader Read(ReadOnlySpan<byte> data) => ScpReader.ReadHeader(data);
}
