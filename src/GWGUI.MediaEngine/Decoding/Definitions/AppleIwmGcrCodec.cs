namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Encode et décode les blocs 6-and-2 employés par les contrôleurs Apple IWM.</summary>
internal static class AppleIwmGcrCodec
{
    /// <summary>Décode les symboles 6-and-2 en données taguées et reconstruit les quatre symboles de checksum.</summary>
    /// <param name="symbols">Symboles décodés de la table GCR, sans identifiant de secteur ni checksum final.</param><param name="checksum">Reçoit les quatre valeurs de checksum reconstruites.</param><returns>Douze octets de tags suivis des 512 octets sectoriels.</returns>
    public static byte[] Decode(ReadOnlySpan<byte> symbols, out byte[] checksum)
    {
        var b1 = new byte[AppleIwmGcrFormat.GroupByteCount];
        var b2 = new byte[AppleIwmGcrFormat.GroupByteCount];
        var b3 = new byte[AppleIwmGcrFormat.GroupByteCount];
        var source = 0;
        for (var index = 0; index <= AppleIwmGcrFormat.LastGroupIndex; index++)
        {
            var w4 = symbols[source++];
            var w1 = symbols[source++];
            var w2 = symbols[source++];
            var w3 = index == AppleIwmGcrFormat.LastGroupIndex ? (byte)0 : symbols[source++];
            b1[index] = (byte)((w1 & AppleIwmGcrFormat.SixBitMask) | ((w4 << AppleIwmGcrFormat.ThirdChecksumShift) & AppleIwmGcrFormat.EncodedHighBitsMask));
            b2[index] = (byte)((w2 & AppleIwmGcrFormat.SixBitMask) | ((w4 << AppleIwmGcrFormat.SecondChecksumShift) & AppleIwmGcrFormat.EncodedHighBitsMask));
            b3[index] = (byte)((w3 & AppleIwmGcrFormat.SixBitMask) | ((w4 << AppleIwmGcrFormat.FirstChecksumShift) & AppleIwmGcrFormat.EncodedHighBitsMask));
        }
        var output = new byte[AppleIwmGcrFormat.TaggedSectorByteCount];
        uint c1 = 0;
        uint c2 = 0;
        uint c3 = 0;
        var destination = 0;
        for (var index = 0; ; index++)
        {
            c1 = (c1 & AppleIwmGcrFormat.ChecksumByteMask) << 1;
            if ((c1 & AppleIwmGcrFormat.ChecksumCarryBit) != 0) c1++;
            var value = (byte)(b1[index] ^ c1);
            c3 += value;
            if ((c1 & AppleIwmGcrFormat.ChecksumCarryBit) != 0)
            {
                c3++;
                c1 &= AppleIwmGcrFormat.ChecksumByteMask;
            }
            output[destination++] = value;
            value = (byte)(b2[index] ^ c3);
            c2 += value;
            if (c3 > AppleIwmGcrFormat.ChecksumByteMask)
            {
                c2++;
                c3 &= AppleIwmGcrFormat.ChecksumByteMask;
            }
            output[destination++] = value;
            if (destination == AppleIwmGcrFormat.TaggedSectorByteCount) break;
            value = (byte)(b3[index] ^ c2);
            c1 += value;
            if (c2 > AppleIwmGcrFormat.ChecksumByteMask)
            {
                c1++;
                c2 &= AppleIwmGcrFormat.ChecksumByteMask;
            }
            output[destination++] = value;
        }
        checksum = [(byte)(c1 & AppleIwmGcrFormat.SixBitMask), (byte)(c2 & AppleIwmGcrFormat.SixBitMask), (byte)(c3 & AppleIwmGcrFormat.SixBitMask), (byte)(((c1 & AppleIwmGcrFormat.ChecksumHighBitsMask) >> AppleIwmGcrFormat.FirstChecksumShift) | ((c2 & AppleIwmGcrFormat.ChecksumHighBitsMask) >> AppleIwmGcrFormat.SecondChecksumShift) | ((c3 & AppleIwmGcrFormat.ChecksumHighBitsMask) >> AppleIwmGcrFormat.ThirdChecksumShift))];
        return output;
    }

    /// <summary>Encode un bloc tagué Apple IWM en symboles GCR 6-and-2 avec checksum.</summary>
    /// <param name="source">Douze octets de tags suivis des 512 octets sectoriels.</param><returns>Symboles GCR encodés.</returns>
    public static byte[] Encode(IReadOnlyList<byte> source)
    {
        var b1 = new byte[AppleIwmGcrFormat.GroupByteCount];
        var b2 = new byte[AppleIwmGcrFormat.GroupByteCount];
        var b3 = new byte[AppleIwmGcrFormat.GroupByteCount];
        uint c1 = 0;
        uint c2 = 0;
        uint c3 = 0;
        var position = 0;
        for (var index = 0; ; index++)
        {
            c1 = (c1 & AppleIwmGcrFormat.ChecksumByteMask) << 1;
            if ((c1 & AppleIwmGcrFormat.ChecksumCarryBit) != 0) c1++;
            var value = source[position++];
            b1[index] = (byte)(value ^ c1);
            c3 += value;
            if ((c1 & AppleIwmGcrFormat.ChecksumCarryBit) != 0)
            {
                c3++;
                c1 &= AppleIwmGcrFormat.ChecksumByteMask;
            }
            value = source[position++];
            b2[index] = (byte)(value ^ c3);
            c2 += value;
            if (c3 > AppleIwmGcrFormat.ChecksumByteMask)
            {
                c2++;
                c3 &= AppleIwmGcrFormat.ChecksumByteMask;
            }
            if (position == source.Count) break;
            value = source[position++];
            b3[index] = (byte)(value ^ c2);
            c1 += value;
            if (c2 > AppleIwmGcrFormat.ChecksumByteMask)
            {
                c1++;
                c2 &= AppleIwmGcrFormat.ChecksumByteMask;
            }
        }
        var symbols = new List<byte>(AppleIwmGcrFormat.EncodedPayloadSymbolCount + AppleIwmGcrFormat.ChecksumSymbolCount);
        for (var index = 0; index <= AppleIwmGcrFormat.LastGroupIndex; index++)
        {
            symbols.Add((byte)(((b1[index]>>AppleIwmGcrFormat.ThirdChecksumShift)&AppleIwmGcrFormat.FirstPackedChecksumMask)|((b2[index]>>AppleIwmGcrFormat.SecondChecksumShift)&AppleIwmGcrFormat.SecondPackedChecksumMask)|((b3[index]>>AppleIwmGcrFormat.FirstChecksumShift)&AppleIwmGcrFormat.ThirdPackedChecksumMask)));
            symbols.Add((byte)(b1[index] & AppleIwmGcrFormat.SixBitMask));
            symbols.Add((byte)(b2[index] & AppleIwmGcrFormat.SixBitMask));
            if (index != AppleIwmGcrFormat.LastGroupIndex) symbols.Add((byte)(b3[index] & AppleIwmGcrFormat.SixBitMask));
        }
        symbols.Add((byte)(((c1&AppleIwmGcrFormat.ChecksumHighBitsMask)>>AppleIwmGcrFormat.FirstChecksumShift)|((c2&AppleIwmGcrFormat.ChecksumHighBitsMask)>>AppleIwmGcrFormat.SecondChecksumShift)|((c3&AppleIwmGcrFormat.ChecksumHighBitsMask)>>AppleIwmGcrFormat.ThirdChecksumShift)));
        symbols.Add((byte)(c3 & AppleIwmGcrFormat.SixBitMask));
        symbols.Add((byte)(c2 & AppleIwmGcrFormat.SixBitMask));
        symbols.Add((byte)(c1 & AppleIwmGcrFormat.SixBitMask));
        return symbols.Select(value => AppleIwmGcrFormat.SixAndTwoTable[value]).ToArray();
    }
}
