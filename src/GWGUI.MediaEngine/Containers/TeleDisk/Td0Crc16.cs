using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Calcule le CRC-16 propre au format TeleDisk.</summary>
internal static class Td0Crc16
{
    /// <summary>Polynôme du CRC-16/TeleDisk.</summary>
    private const ushort Polynomial = 0xA097;

    /// <summary>Calcule un CRC à partir d'une valeur initiale, sans réflexion ni XOR final.</summary>
    /// <param name="data">Octets à intégrer au calcul.</param>
    /// <param name="initial">Valeur initiale ou résultat d'un calcul précédent.</param>
    /// <returns>CRC-16 calculé.</returns>
    public static ushort Compute(ReadOnlySpan<byte> data, ushort initial = 0)
    {
        var crc = initial;
        foreach (var value in data) crc = Crc16Calculator.Update(crc, value, Polynomial);
        return crc;
    }
}
