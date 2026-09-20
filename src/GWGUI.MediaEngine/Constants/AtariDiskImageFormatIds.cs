using MediaImageFormatIds = global::GWGUI.MediaFileSystems.Constants.MediaImageFormatIds;

using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Constants;

/// <summary>Définit et construit les identifiants des images de disquettes Atari.</summary>
public static partial class DiskImageFormatIds
{
    /// <summary>Préfixe des formats Atari 8 bits.</summary>
    public const string AtariPrefix = MediaImageFormatIds.AtariPrefix;
    /// <summary>Image Atari 8 bits de 90 Kio.</summary>
    public const string Atari90 = MediaImageFormatIds.Atari90;
    /// <summary>Image Atari 8 bits de 130 Kio.</summary>
    public const string Atari130 = MediaImageFormatIds.Atari130;
    /// <summary>Image Atari 8 bits étendue de 140 Kio.</summary>
    public const string Atari140 = MediaImageFormatIds.Atari140;
    /// <summary>Image Atari 8 bits de 180 Kio.</summary>
    public const string Atari180 = MediaImageFormatIds.Atari180;
    /// <summary>Image protégée Atari 8 bits au format ATX.</summary>
    public const string AtariAtx = MediaImageFormatIds.AtariAtx;
    /// <summary>Image XFD brute Atari 8 bits de 90 Kio.</summary>
    public const string AtariXfd90 = MediaImageFormatIds.AtariXfd90;
    /// <summary>Image XFD brute Atari 8 bits de 130 Kio.</summary>
    public const string AtariXfd130 = MediaImageFormatIds.AtariXfd130;
    /// <summary>Image XFD brute Atari 8 bits de 140 Kio.</summary>
    public const string AtariXfd140 = MediaImageFormatIds.AtariXfd140;
    /// <summary>Image XFD brute Atari 8 bits de 180 Kio.</summary>
    public const string AtariXfd180 = MediaImageFormatIds.AtariXfd180;
    /// <summary>Préfixe des formats Atari ST.</summary>
    public const string AtariStPrefix = "atarist.";
    /// <summary>Image Atari ST de 180 Kio.</summary>
    public const string AtariSt180 = MediaImageFormatIds.AtariSt180;
    /// <summary>Image Atari ST de 360 Kio.</summary>
    public const string AtariSt360 = MediaImageFormatIds.AtariSt360;
    /// <summary>Image Atari ST de 400 Kio.</summary>
    public const string AtariSt400 = MediaImageFormatIds.AtariSt400;
    /// <summary>Image Atari ST de 440 Kio.</summary>
    public const string AtariSt440 = MediaImageFormatIds.AtariSt440;
    /// <summary>Image Atari ST de 720 Kio.</summary>
    public const string AtariSt720 = MediaImageFormatIds.AtariSt720;
    /// <summary>Image Atari ST de 800 Kio.</summary>
    public const string AtariSt800 = MediaImageFormatIds.AtariSt800;
    /// <summary>Image Atari ST de 810 Kio.</summary>
    public const string AtariSt810 = MediaImageFormatIds.AtariSt810;
    /// <summary>Image Atari ST de 880 Kio.</summary>
    public const string AtariSt880 = MediaImageFormatIds.AtariSt880;
    /// <summary>Image Atari ST de 1 440 Kio.</summary>
    public const string AtariSt1440 = MediaImageFormatIds.AtariSt1440;

    /// <summary>Construit l'identifiant Atari ST en tronquant au kibioctet inférieur une capacité non alignée.</summary>
    /// <param name="capacityBytes">Capacité strictement positive de l'image, en octets.</param>
    /// <returns>Identifiant Atari ST contenant la capacité entière en kibioctets.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La capacité est négative.</exception>
    public static string AtariStFromCapacity(long capacityBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacityBytes);
        return $"{AtariStPrefix}{capacityBytes / DataSizeConstants.BytesPerKibibyte}";
    }

    /// <summary>Construit l'identifiant de repli d'un conteneur ATR.</summary>
    /// <param name="sectorSize">Taille strictement positive d'un secteur, en octets.</param>
    /// <param name="sectorCount">Nombre strictement positif de secteurs.</param>
    /// <returns>Identifiant ATR contenant la taille et le nombre de secteurs.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Une valeur n'est pas strictement positive.</exception>
    public static string AtariAtr(int sectorSize, int sectorCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sectorSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sectorCount);
        return $"{AtariPrefix}atr.{sectorSize}.{sectorCount}";
    }

    /// <summary>Construit l'identifiant de repli d'une reconstruction SCP Atari.</summary>
    /// <param name="sectorSize">Taille strictement positive d'un secteur reconstruit, en octets.</param>
    /// <param name="sectorsPerTrack">Nombre strictement positif de secteurs reconstruits par piste.</param>
    /// <returns>Identifiant SCP Atari contenant la taille et le nombre de secteurs par piste.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Une valeur n'est pas strictement positive.</exception>
    public static string AtariScp(int sectorSize, int sectorsPerTrack)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sectorSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sectorsPerTrack);
        return $"{AtariPrefix}scp.{sectorSize}.{sectorsPerTrack}";
    }
}
