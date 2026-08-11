using GWGUI.MediaEngine.Decoding.Apple;

namespace GWGUI.MediaEngine.Decoding.Definitions;

/// <summary>Encode et décode les représentations 4-and-4, 5-and-3 et 6-and-2 des secteurs Apple II.</summary>
internal static class AppleIIGcrCodec
{
    /// <summary>Décode une valeur Apple II stockée sur deux octets 4-and-4.</summary>
    /// <param name="high">Premier octet encodé.</param><param name="low">Second octet encodé.</param><returns>Valeur décodée.</returns>
    public static byte DecodeFourAndFour(byte high, byte low) => (byte)(((high << 1) | 1) & low);

    /// <summary>Encode une valeur Apple II sur deux octets 4-and-4.</summary>
    /// <param name="value">Valeur à encoder.</param><returns>Les deux octets encodés.</returns>
    public static (byte High, byte Low) EncodeFourAndFour(byte value) => ((byte)((value >> 1) | AppleIIGcrFormat.FourAndFourMask), (byte)(value | AppleIIGcrFormat.FourAndFourMask));

    /// <summary>Décode un bloc de secteur Apple II 6-and-2 depuis un flux de bits.</summary>
    /// <param name="bits">Bits de la piste.</param><param name="offset">Position de lecture initiale, en bits.</param><returns>Données, validité du checksum et position finale, ou <see langword="null"/> en cas de bloc incomplet ou de symbole inconnu.</returns>
    public static (byte[] Data, bool Valid, int EndOffset)? TryDecodeSixAndTwo(IReadOnlyList<bool> bits, int offset)
    {
        var cursor = offset;
        var encoded = AppleBitLatch.TryReadBytes(bits, ref cursor, AppleIIGcrFormat.SixAndTwoEncodedByteCount);
        if (encoded is null) return null;
        var values = new byte[AppleIIGcrFormat.SixAndTwoEncodedByteCount];
        for (var index = 0; index < values.Length; index++)
            if (!AppleIIGcrFormat.InverseSixAndTwoTable.TryGetValue(encoded[index], out values[index])) return null;
        var decoded = new byte[AppleIIGcrFormat.SixAndTwoDecodedByteCount];
        byte previous = 0;
        var encodedIndex = 0;
        for (var index = AppleIIGcrFormat.SixAndTwoDecodedByteCount - 1; index >= AppleIIGcrFormat.SectorSize; index--)
        {
            decoded[index] = (byte)(values[encodedIndex++] ^ previous);
            previous = decoded[index];
        }
        for (var index = 0; index < AppleIIGcrFormat.SectorSize; index++)
        {
            decoded[index] = (byte)(values[encodedIndex++] ^ previous);
            previous = decoded[index];
        }
        var valid = (byte)(values[AppleIIGcrFormat.SixAndTwoEncodedByteCount - 1] ^ previous) == 0;
        var data = new byte[AppleIIGcrFormat.SectorSize];
        byte auxiliaryOffset = 0;
        for (var index = 0; index < AppleIIGcrFormat.SectorSize; index++)
        {
            auxiliaryOffset = (byte)((auxiliaryOffset + AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount - 1) % AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount);
            var auxiliary = decoded[AppleIIGcrFormat.SectorSize + auxiliaryOffset];
            decoded[AppleIIGcrFormat.SectorSize + auxiliaryOffset] = (byte)(auxiliary >> 2);
            data[index] = (byte)((decoded[index] << 2) | ((auxiliary & 2) >> 1) | ((auxiliary & 1) << 1));
        }
        return (data, valid, cursor);
    }

    /// <summary>Décode un bloc de secteur Apple II 5-and-3 depuis un flux de bits.</summary>
    /// <param name="bits">Bits de la piste.</param><param name="offset">Position de lecture initiale, en bits.</param><returns>Données, validité du checksum et position finale, ou <see langword="null"/> en cas de bloc incomplet ou de symbole inconnu.</returns>
    public static (byte[] Data, bool Valid, int EndOffset)? TryDecodeFiveAndThree(IReadOnlyList<bool> bits, int offset)
    {
        var cursor = offset;
        var encoded = AppleBitLatch.TryReadBytes(bits, ref cursor, AppleIIGcrFormat.FiveAndThreeEncodedByteCount);
        if (encoded is null) return null;
        var values = new byte[AppleIIGcrFormat.FiveAndThreeEncodedByteCount];
        for (var index = 0; index < values.Length; index++)
            if (!AppleIIGcrFormat.InverseFiveAndThreeTable.TryGetValue(encoded[index], out values[index])) return null;
        const int threeSize = AppleIIGcrFormat.FiveAndThreeAuxiliaryByteCount;
        const int chunkSize = AppleIIGcrFormat.FiveAndThreeChunkByteCount;
        var threes = new byte[threeSize];
        var bases = new byte[AppleIIGcrFormat.SectorSize];
        byte checksum = 0;
        var source = 0;
        for (var index = threeSize - 1; index >= 0; index--)
        {
            checksum ^= values[source++];
            threes[index] = checksum;
        }
        for (var index = 0; index < AppleIIGcrFormat.SectorSize; index++)
        {
            checksum ^= values[source++];
            bases[index] = (byte)(checksum << 3);
        }
        var valid = values[source] == checksum;
        var data = new byte[AppleIIGcrFormat.SectorSize];
        var destination = 0;
        for (var index = chunkSize - 1; index >= 0; index--)
        {
            var one = threes[index];
            var two = threes[chunkSize + index];
            var three = threes[chunkSize * 2 + index];
            var four = (byte)(((one & 2) << 1) | (two & 2) | ((three & 2) >> 1));
            var five = (byte)(((one & 1) << 2) | ((two & 1) << 1) | (three & 1));
            data[destination++] = (byte)(bases[index] | ((one >> 2) & 7));
            data[destination++] = (byte)(bases[chunkSize + index] | ((two >> 2) & 7));
            data[destination++] = (byte)(bases[chunkSize * 2 + index] | ((three >> 2) & 7));
            data[destination++] = (byte)(bases[chunkSize * 3 + index] | (four & 7));
            data[destination++] = (byte)(bases[chunkSize * 4 + index] | (five & 7));
        }
        data[destination] = (byte)(bases[AppleIIGcrFormat.SectorSize - 1] | (threes[threeSize - 1] & 7));
        return (data, valid, cursor);
    }

    /// <summary>Encode un secteur Apple II en représentation 6-and-2.</summary>
    /// <param name="source">Données sectorielles à encoder.</param><returns>Symboles GCR encodés, checksum inclus.</returns>
    public static byte[] EncodeSixAndTwo(IReadOnlyList<byte> source)
    {
        var buffer = new byte[AppleIIGcrFormat.SixAndTwoWorkBufferByteCount];
        for (var index = 0; index < source.Count; index++) buffer[index] = source[index];
        var encoded = new List<byte>(AppleIIGcrFormat.SixAndTwoEncodedByteCount);
        byte checksum = 0;
        for (var index = 0; index < AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount; index++)
        {
            var value = (byte)(((buffer[index] & 1) << 1) | ((buffer[index] & 2) >> 1) | ((buffer[index + AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount] & 1) << 3) | ((buffer[index + AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount] & 2) << 1) | ((buffer[index + AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount * 2] & 1) << 5) | ((buffer[index + AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount * 2] & 2) << 3));
            encoded.Add(AppleIIGcrFormat.SixAndTwoTable[value ^ checksum]);
            checksum = value;
        }
        for (var index = 0; index < AppleIIGcrFormat.SectorSize; index++)
        {
            var value = (byte)(source[index] >> 2);
            encoded.Add(AppleIIGcrFormat.SixAndTwoTable[value ^ checksum]);
            checksum = value;
        }
        encoded.Add(AppleIIGcrFormat.SixAndTwoTable[checksum]);
        return encoded.ToArray();
    }

    /// <summary>Encode un secteur Apple II en représentation 5-and-3.</summary>
    /// <param name="source">Données sectorielles à encoder.</param><returns>Symboles GCR encodés, checksum inclus.</returns>
    public static byte[] EncodeFiveAndThree(IReadOnlyList<byte> source)
    {
        const int chunkSize = AppleIIGcrFormat.FiveAndThreeChunkByteCount;
        const int threeSize = AppleIIGcrFormat.FiveAndThreeAuxiliaryByteCount;
        var top = new byte[AppleIIGcrFormat.SectorSize];
        var threes = new byte[threeSize];
        var chunk = chunkSize - 1;
        var sourceOffset = 0;
        for (var index = 0; index < chunkSize * 5; index += 5)
        {
            var zero = source[sourceOffset++];
            var one = source[sourceOffset++];
            var two = source[sourceOffset++];
            var three = source[sourceOffset++];
            var four = source[sourceOffset++];
            top[chunk] = (byte)(zero >> 3);
            top[chunk + chunkSize] = (byte)(one >> 3);
            top[chunk + chunkSize * 2] = (byte)(two >> 3);
            top[chunk + chunkSize * 3] = (byte)(three >> 3);
            top[chunk + chunkSize * 4] = (byte)(four >> 3);
            threes[chunk] = (byte)(((zero & 7) << 2) | ((three & 4) >> 1) | ((four & 4) >> 2));
            threes[chunk + chunkSize] = (byte)(((one & 7) << 2) | (three & 2) | ((four & 2) >> 1));
            threes[chunk + chunkSize * 2] = (byte)(((two & 7) << 2) | ((three & 1) << 1) | (four & 1));
            chunk--;
        }
        var last = source[sourceOffset];
        top[AppleIIGcrFormat.SectorSize - 1] = (byte)(last >> 3);
        threes[^1] = (byte)(last & 7);
        var encoded = new List<byte>(AppleIIGcrFormat.FiveAndThreeEncodedByteCount);
        byte checksum = 0;
        for (var index = threeSize - 1; index >= 0; index--)
        {
            encoded.Add(AppleIIGcrFormat.FiveAndThreeTable[threes[index] ^ checksum]);
            checksum = threes[index];
        }
        for (var index = 0; index < AppleIIGcrFormat.SectorSize; index++)
        {
            encoded.Add(AppleIIGcrFormat.FiveAndThreeTable[top[index] ^ checksum]);
            checksum = top[index];
        }
        encoded.Add(AppleIIGcrFormat.FiveAndThreeTable[checksum]);
        return encoded.ToArray();
    }
}
