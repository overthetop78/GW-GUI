namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

public static class GreaseweazleFluxStreamDecoder
{
    public static GreaseweazleFluxCapture Decode(ReadOnlySpan<byte> stream, uint sampleFrequency)
    {
        if (sampleFrequency == 0) throw new ArgumentOutOfRangeException(nameof(sampleFrequency));
        if (stream.IsEmpty || stream[^1] != GreaseweazleFluxProtocol.EndOfStream)
            throw new InvalidDataException("The Greaseweazle flux stream is not terminated.");

        var flux = new List<uint>();
        var indexes = new List<uint>();
        ulong pendingTicks = 0;
        long ticksSinceIndex = 0;
        var position = 0;
        while (position < stream.Length - 1)
        {
            var value = stream[position++];
            if (value == GreaseweazleFluxProtocol.Escape)
            {
                EnsureAvailable(stream, position, 1);
                var opcode = (GreaseweazleFluxOpcode)stream[position++];
                EnsureAvailable(stream, position, GreaseweazleFluxProtocol.ExtendedValueLength);
                var extended = ReadExtendedValue(stream.Slice(position, GreaseweazleFluxProtocol.ExtendedValueLength));
                position += GreaseweazleFluxProtocol.ExtendedValueLength;
                if (opcode == GreaseweazleFluxOpcode.Index)
                {
                    indexes.Add(checked((uint)(ticksSinceIndex + checked((long)pendingTicks) + extended)));
                    ticksSinceIndex = -checked((long)(pendingTicks + extended));
                }
                else if (opcode == GreaseweazleFluxOpcode.Space)
                {
                    pendingTicks += extended;
                }
                else
                {
                    throw new InvalidDataException($"Unsupported Greaseweazle flux opcode {(byte)opcode}.");
                }
                continue;
            }

            uint interval;
            if (value < GreaseweazleFluxProtocol.LongIntervalStart)
            {
                interval = value;
            }
            else
            {
                EnsureAvailable(stream, position, 1);
                var low = stream[position++];
                if (low == 0) throw new InvalidDataException("A long Greaseweazle flux interval has an invalid low byte.");
                interval = checked((uint)(GreaseweazleFluxProtocol.LongIntervalStart +
                    (value - GreaseweazleFluxProtocol.LongIntervalStart) * byte.MaxValue + low - 1));
            }

            pendingTicks += interval;
            var decoded = checked((uint)pendingTicks);
            if (decoded == 0) throw new InvalidDataException("A Greaseweazle flux interval cannot be zero.");
            flux.Add(decoded);
            ticksSinceIndex += decoded;
            pendingTicks = 0;
        }

        return new GreaseweazleFluxCapture(flux, indexes, sampleFrequency, stream.ToArray());
    }

    private static uint ReadExtendedValue(ReadOnlySpan<byte> value) =>
        checked((uint)(((value[0] & 254) >> 1) +
            ((value[1] & 254) << 6) +
            ((value[2] & 254) << 13) +
            ((value[3] & 254) << 20)));

    private static void EnsureAvailable(ReadOnlySpan<byte> stream, int position, int length)
    {
        if (position + length > stream.Length - 1)
            throw new InvalidDataException("The Greaseweazle flux stream ends inside an encoded value.");
    }
}
