namespace GWGUI.MediaEngine.Interfaces;

/// <summary>Calculs d'intégrité de données fournis par MediaEngine sans accès aux fichiers.</summary>
public interface IMediaDataIntegrity
{
    /// <summary>Calcule le CRC32 réfléchi standard, avec le polynôme 0xEDB88320 et un complément final.</summary>
    /// <param name="data">Octets empruntés, consultés sans modification ni copie.</param>
    /// <returns>Valeur numérique du CRC32 ; zéro pour une étendue vide.</returns>
    /// <remarks>Le calcul reprend celui des conteneurs WOZ. L'appelant conserve le formatage, la sélection
    /// des octets couverts et les règles de reconnaissance. Ce CRC ne remplace pas une empreinte cryptographique.</remarks>
    uint ComputeCrc32(ReadOnlySpan<byte> data);
}
