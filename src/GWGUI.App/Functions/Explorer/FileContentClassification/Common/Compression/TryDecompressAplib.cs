namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool TryDecompressAplib(
        IReadOnlyList<byte>? data,
        int maximumOutputLength,
        out byte[] decoded)
    {
        decoded = [];
        if (data is not { Count: > 0 } || maximumOutputLength <= 0)
            return false;

        var sourceOffset = 0;
        var tag = 0;
        var remainingTagBits = 0;
        var output = new List<byte>();
        var lastOffset = -1;
        var lastTokenWasMatch = false;

        bool TryReadByte(out int value)
        {
            if (sourceOffset >= data.Count)
            {
                value = 0;
                return false;
            }

            value = data[sourceOffset++];
            return true;
        }

        bool TryReadBit(out int value)
        {
            if (remainingTagBits == 0)
            {
                if (!TryReadByte(out tag))
                {
                    value = 0;
                    return false;
                }

                remainingTagBits = 8;
            }

            value = (tag & 0x80) == 0 ? 0 : 1;
            tag = (tag << 1) & 0xff;
            remainingTagBits--;
            return true;
        }

        bool TryReadGamma(out int value)
        {
            value = 1;
            while (true)
            {
                if (!TryReadBit(out var dataBit) || value > (int.MaxValue >> 1))
                    return false;
                value = (value << 1) | dataBit;
                if (!TryReadBit(out var continuationBit))
                    return false;
                if (continuationBit == 0)
                    return true;
            }
        }

        bool TryCopy(int distance, int length)
        {
            if (distance <= 0
                || distance > output.Count
                || length < 0
                || length > maximumOutputLength - output.Count)
                return false;

            for (var index = 0; index < length; index++)
                output.Add(output[^distance]);
            return true;
        }

        if (!TryReadByte(out var firstByte))
            return false;
        output.Add((byte)firstByte);

        while (output.Count <= maximumOutputLength)
        {
            if (!TryReadBit(out var firstTokenBit))
                return false;
            if (firstTokenBit == 0)
            {
                if (!TryReadByte(out var literal) || output.Count == maximumOutputLength)
                    return false;
                output.Add((byte)literal);
                lastTokenWasMatch = false;
                continue;
            }

            if (!TryReadBit(out var secondTokenBit))
                return false;
            if (secondTokenBit == 0)
            {
                if (!TryReadGamma(out var encodedOffset))
                    return false;

                int distance;
                int length;
                if (!lastTokenWasMatch && encodedOffset == 2)
                {
                    distance = lastOffset;
                    if (!TryReadGamma(out length))
                        return false;
                }
                else
                {
                    encodedOffset -= lastTokenWasMatch ? 2 : 3;
                    if (encodedOffset < 0
                        || encodedOffset > 0x7fffff
                        || !TryReadByte(out var offsetLow))
                        return false;
                    distance = (encodedOffset << 8) | offsetLow;
                    if (!TryReadGamma(out length))
                        return false;
                    if (distance >= 32000) length++;
                    if (distance >= 1280) length++;
                    if (distance < 128) length += 2;
                }

                if (!TryCopy(distance, length))
                    return false;
                lastOffset = distance;
                lastTokenWasMatch = true;
                continue;
            }

            if (!TryReadBit(out var thirdTokenBit))
                return false;
            if (thirdTokenBit == 0)
            {
                if (!TryReadByte(out var encoded))
                    return false;
                var distance = encoded >> 1;
                if (distance == 0)
                {
                    if (sourceOffset != data.Count)
                        return false;
                    decoded = output.ToArray();
                    return true;
                }

                if (!TryCopy(distance, 2 + (encoded & 1)))
                    return false;
                lastOffset = distance;
                lastTokenWasMatch = true;
                continue;
            }

            var shortDistance = 0;
            for (var index = 0; index < 4; index++)
            {
                if (!TryReadBit(out var offsetBit))
                    return false;
                shortDistance = (shortDistance << 1) | offsetBit;
            }

            if (output.Count == maximumOutputLength)
                return false;
            if (shortDistance == 0)
                output.Add(0);
            else if (!TryCopy(shortDistance, 1))
                return false;
            lastTokenWasMatch = false;
        }

        return false;
    }
}
