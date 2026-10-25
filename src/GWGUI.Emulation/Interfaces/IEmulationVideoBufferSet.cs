namespace GWGUI.Emulation.Interfaces;

/// <summary>Jeu tournant de tableaux vidéo loués dans un pool emprunté.</summary>
/// <remarks>L’appelant possède le jeu et le libère après la dernière utilisation de ses images.
/// Chaque tableau reste possédé par le jeu : les consommateurs le copient pour conserver durablement
/// ses pixels et ne le rendent pas eux-mêmes au pool.</remarks>
public interface IEmulationVideoBufferSet : IDisposable
{
    /// <summary>Obtient le tableau du prochain emplacement, agrandi par location si nécessaire.</summary>
    /// <param name="length">Longueur minimale demandée, en octets.</param>
    /// <returns>Tableau emprunté dont la longueur peut dépasser la demande.</returns>
    /// <remarks>Le roulement réutilise les emplacements. Le contenu peut être remplacé lorsque le producteur
    /// revient sur le même emplacement ; Dispose rend tous les tableaux encore détenus au pool.</remarks>
    byte[] Rent(int length);

    /// <summary>Copie les lignes d’une image native dans le prochain buffer possédé par le jeu.</summary>
    /// <param name="data">Pointeur emprunté vers les pixels, valide pendant toute la copie.</param>
    /// <param name="height">Nombre de lignes de l’image.</param>
    /// <param name="pitch">Nombre d’octets séparant deux débuts de lignes.</param>
    /// <returns>Tableau emprunté au jeu ; seuls height multiplié par pitch octets contiennent l’image.</returns>
    /// <remarks>Le pointeur source n’est pas libéré par cette opération. Les règles de roulement et de
    /// restitution restent celles de Rent et Dispose ; les calculs utilisent les fonctions communes existantes.</remarks>
    byte[] CopyFramePixels(nint data, uint height, nuint pitch);
}
