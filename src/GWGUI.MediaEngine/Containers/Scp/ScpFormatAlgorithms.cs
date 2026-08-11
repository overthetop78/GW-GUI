using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Regroupe les calculs définis par le format de conteneur SCP.</summary>
public static class ScpFormatAlgorithms
{
    /// <summary>Ajoute des octets à une somme de contrôle SCP existante avec un cumul non signé sur 32 bits.</summary>
    /// <param name="checksum">Somme de contrôle calculée avant les octets fournis.</param>
    /// <param name="data">Octets SCP à ajouter au cumul.</param>
    /// <returns>Somme des octets modulo 2<sup>32</sup>.</returns>
    public static uint UpdateChecksum(uint checksum, ReadOnlySpan<byte> data)
    {
        foreach (var value in data)
        {
            checksum = unchecked(checksum + value);
        }

        return checksum;
    }

    /// <summary>Calcule la somme de contrôle SCP des octets fournis avec un cumul non signé sur 32 bits.</summary>
    /// <param name="data">Octets couverts par la somme de contrôle SCP.</param>
    /// <returns>Somme des octets modulo 2<sup>32</sup>.</returns>
    public static uint ComputeChecksum(ReadOnlySpan<byte> data) => UpdateChecksum(ScpFormatConstants.InitialChecksum, data);

    /// <summary>Indique si une somme de contrôle calculée respecte la valeur déclarée dans l’en-tête SCP.</summary>
    /// <param name="declaredChecksum">Somme de contrôle enregistrée dans l’en-tête SCP.</param>
    /// <param name="flags">Drapeaux de l’en-tête SCP.</param>
    /// <param name="computedChecksum">Somme de contrôle calculée sur les octets couverts.</param>
    /// <returns><see langword="true"/> lorsque les deux sommes sont identiques, ou lorsque la somme déclarée indique son absence pour une capture réinscriptible ; sinon <see langword="false"/>.</returns>
    public static bool IsChecksumValid(uint declaredChecksum, ScpFlags flags, uint computedChecksum)
    {
        var checksumIsOmitted = declaredChecksum == ScpFormatConstants.MissingChecksum && (flags & ScpFlags.Writable) != ScpFlags.None;
        return checksumIsOmitted || declaredChecksum == computedChecksum;
    }

    /// <summary>Convertit un numéro d’entrée de piste SCP en cylindre et face physiques.</summary>
    /// <param name="trackNumber">Numéro compris entre zéro et <see cref="ScpFormatConstants.FloppyTrackSlots"/> moins un.</param>
    /// <returns>Couple formé du cylindre et de la face correspondant au numéro SCP.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="trackNumber"/> est hors de la table des pistes SCP.</exception>
    public static (int Cylinder, int Head) ToTrackAddress(int trackNumber)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(trackNumber);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(trackNumber, ScpFormatConstants.FloppyTrackSlots);
        return (trackNumber / DiskGeometryConstants.DoubleSidedHeadCount, trackNumber % DiskGeometryConstants.DoubleSidedHeadCount);
    }
}
