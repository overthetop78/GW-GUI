using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding.Apple;

/// <summary>Lit des octets Apple dont le bit de poids fort marque la synchronisation dans un flux de bits.</summary>
internal static class AppleBitLatch
{
    /// <summary>Masque du bit de poids fort signalant qu'un octet Apple est synchronisé.</summary>
    private const byte SynchronizedByteMask = 0x80;

    /// <summary>Lit une suite d'octets Apple synchronisés depuis la position courante du flux.</summary>
    /// <param name="bits">Bits à lire dans leur ordre logique.</param>
    /// <param name="offset">Position de lecture, avancée après chaque bit consommé.</param>
    /// <param name="count">Nombre d'octets à lire.</param>
    /// <returns>Octets lus, ou <see langword="null"/> lorsque le flux ne contient pas assez de bits.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> est négatif, ou <paramref name="offset"/> est hors des limites de <paramref name="bits"/>.</exception>
    public static byte[]? TryReadBytes(IReadOnlyList<bool> bits, ref int offset, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if ((uint)offset > (uint)bits.Count) throw new ArgumentOutOfRangeException(nameof(offset));
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            if (offset + BitPrimitives.BitsPerByte > bits.Count) return null;
            byte value = 0;
            for (var bit = 0; bit < BitPrimitives.BitsPerByte; bit++)
                value = (byte)((value << 1) | (bits[offset++] ? 1 : 0));

            while ((value & SynchronizedByteMask) == 0)
            {
                if (offset >= bits.Count) return null;
                value = (byte)((value << 1) | (bits[offset++] ? 1 : 0));
            }

            result[index] = value;
        }

        return result;
    }
}
