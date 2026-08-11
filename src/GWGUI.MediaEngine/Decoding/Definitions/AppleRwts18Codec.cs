namespace GWGUI.MediaEngine.Decoding.Definitions;

internal static class AppleRwts18Codec
{
    public static (byte[] Data, bool Valid) DecodePayload(IReadOnlyList<byte> values)
    {
        var page1 = new byte[AppleRwts18Format.PageByteCount]; var page2 = new byte[AppleRwts18Format.PageByteCount]; var page3 = new byte[AppleRwts18Format.PageByteCount];
        byte accumulator = 0; byte previousPage1 = 0;
        for (var index = 0; index < AppleRwts18Format.PageByteCount; index++)
        {
            var high = values[index * AppleRwts18Format.SymbolsPerPageGroup];
            var checksum = (byte)(accumulator ^ previousPage1 ^ high);
            page1[index] = (byte)(((high << AppleRwts18Format.FirstPageHighBitShift) & AppleRwts18Format.HighBitMask) | values[index * AppleRwts18Format.SymbolsPerPageGroup + 1]);
            previousPage1 = page1[index];
            page2[index] = (byte)(((high << AppleRwts18Format.SecondPageHighBitShift) & AppleRwts18Format.HighBitMask) | values[index * AppleRwts18Format.SymbolsPerPageGroup + 2]);
            page3[index] = (byte)(((high << AppleRwts18Format.ThirdPageHighBitShift) & AppleRwts18Format.HighBitMask) | values[index * AppleRwts18Format.SymbolsPerPageGroup + 3]);
            accumulator = (byte)(page3[index] ^ page2[index] ^ checksum);
        }
        var valid = ((accumulator ^ values[AppleRwts18Format.PayloadChecksumOffset] ^ previousPage1) & AppleRwts18Format.SixBitMask) == 0;
        return ([.. page1, .. page2, .. page3], valid);
    }

    public static byte[] EncodePayload(IReadOnlyList<byte> data)
    {
        var encoded = new byte[AppleRwts18Format.PayloadWithChecksumSymbolCount]; byte checksum = 0;
        for (var index = 0; index < AppleRwts18Format.PageByteCount; index++)
        {
            var one = data[index]; var two = data[AppleRwts18Format.PageByteCount * AppleRwts18Format.SecondPageIndex + index]; var three = data[AppleRwts18Format.PageByteCount * AppleRwts18Format.ThirdPageIndex + index];
            var high = (byte)(((one >> AppleRwts18Format.SourceHighBitShift) << AppleRwts18Format.FirstPagePackedShift) | ((two >> AppleRwts18Format.SourceHighBitShift) << AppleRwts18Format.SecondPagePackedShift) | (three >> AppleRwts18Format.SourceHighBitShift));
            var values = new[] { high, (byte)(one & AppleRwts18Format.SixBitMask), (byte)(two & AppleRwts18Format.SixBitMask), (byte)(three & AppleRwts18Format.SixBitMask) };
            for (var valueIndex = 0; valueIndex < values.Length; valueIndex++)
            {
                var value = values[valueIndex];
                checksum ^= value;
                encoded[index * AppleRwts18Format.SymbolsPerPageGroup + valueIndex] = AppleIIGcrFormat.SixAndTwoTable[value];
            }
        }
        encoded[AppleRwts18Format.PayloadChecksumOffset] = AppleIIGcrFormat.SixAndTwoTable[checksum & AppleRwts18Format.SixBitMask];
        return encoded;
    }
}
