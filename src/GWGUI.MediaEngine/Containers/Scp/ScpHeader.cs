namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>
/// Représente l'en-tête fixe d'un conteneur SCP.
/// </summary>
/// <param name="Version">Version encodée sur un octet, avec le chiffre majeur dans le demi-octet fort et le chiffre mineur dans le demi-octet faible.</param>
/// <param name="DiskType">Identifiant numérique du type de disque déclaré par le conteneur.</param>
/// <param name="Revolutions">Nombre de révolutions enregistrées pour chaque entrée de piste.</param>
/// <param name="StartTrack">Première entrée de piste déclarée dans la table SCP.</param>
/// <param name="EndTrack">Dernière entrée de piste déclarée dans la table SCP, bornes incluses.</param>
/// <param name="Flags">Caractéristiques de capture déclarées par le conteneur.</param>
/// <param name="BitCellEncoding">Largeur des entrées de cellule de bit déclarée par le conteneur.</param>
/// <param name="Heads">Faces dont les pistes sont présentes dans le conteneur.</param>
/// <param name="Resolution">Indice de résolution temporelle SCP ; un pas vaut <see cref="ScpFormatConstants.ResolutionStepNanoseconds"/> nanosecondes multipliées par cet indice augmenté de <see cref="ScpFormatConstants.ResolutionIndexOffset"/>.</param>
/// <param name="Checksum">Somme de contrôle non signée déclarée dans l'en-tête.</param>
public sealed record ScpHeader(byte Version, byte DiskType, byte Revolutions, byte StartTrack, byte EndTrack, ScpFlags Flags, ScpBitCellEncoding BitCellEncoding, ScpHeadSelection Heads, byte Resolution, uint Checksum)
{
    /// <summary>
    /// Obtient le nombre d'entrées comprises entre <see cref="StartTrack"/> et <see cref="EndTrack"/>, bornes incluses.
    /// </summary>
    public int TrackCount => EndTrack - StartTrack + 1;

    /// <summary>
    /// Obtient la durée d'un pas temporel de flux, en nanosecondes.
    /// </summary>
    public int ResolutionNanoseconds => ScpFormatConstants.ResolutionStepNanoseconds * (Resolution + ScpFormatConstants.ResolutionIndexOffset);

    /// <summary>
    /// Obtient la version sous la forme textuelle « majeur.mineur ».
    /// </summary>
    public string VersionText => $"{Version >> ScpFormatConstants.VersionMajorShift}.{Version & ScpFormatConstants.VersionMinorMask}";
}
