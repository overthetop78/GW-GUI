using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.Tests;

public sealed class AdditionalFluxDecoderTests
{
    [Fact]
    public void HpMmfmDecoderExtractsIdentityPayloadAndCrc()
    {
        const byte cylinder = 12;
        const byte head = 1;
        const byte sector = 5;
        var payload = Enumerable.Range(0, 256).Select(index => (byte)index).ToArray();
        var id = AddCrc([ReverseBits(cylinder), ReverseBits((byte)(sector | head << 7))]);
        var encodedPayload = payload.ToArray();
        for (var index = 0; index < encodedPayload.Length; index += 2) (encodedPayload[index], encodedPayload[index + 1]) = (encodedPayload[index + 1], encodedPayload[index]);
        for (var index = 0; index < encodedPayload.Length; index++) encodedPayload[index] = ReverseBits(encodedPayload[index]);
        var data = AddCrc(encodedPayload);
        var bits = RawBytes([0x55, 0x55, 0x2a, 0x54]) + EncodeMfm(id) + string.Concat(Enumerable.Repeat("10", 64))
            + RawBytes([0x55, 0x55, 0x2a, 0x44]) + EncodeMfm(data) + new string('0', 32);

        var result = new HpMmfmDecoder().Decode(Revolution(bits));

        var decoded = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, decoded.Cylinder);
        Assert.Equal(head, decoded.Head);
        Assert.Equal(sector, decoded.Number);
        Assert.True(decoded.IntegrityValid);
        Assert.Equal(payload, result.DecodedBytes);
    }

    [Fact]
    public void DataGeneralDecoderExtractsPayloadAndChecksum()
    {
        const byte cylinder = 23;
        const byte head = 1;
        const byte sector = 4;
        var payload = Enumerable.Range(0, 512).Select(index => (byte)(index * 3)).ToArray();
        var checksum = DataGeneralChecksum(payload);
        var bits = EncodeFm([0, 1]) + EncodeFm([(byte)(cylinder | head << 7), (byte)(sector << 2)])
            + new string('0', 64) + EncodeFm([0, 1]) + EncodeFm(payload.Concat([(byte)(checksum >> 8), (byte)checksum]).ToArray());

        var result = new DataGeneralFmDecoder().Decode(Revolution(bits));

        var decoded = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, decoded.Cylinder);
        Assert.Equal(head, decoded.Head);
        Assert.Equal(sector, decoded.Number);
        Assert.True(decoded.IntegrityValid);
        Assert.Equal(payload, result.DecodedBytes);
    }

    [Fact]
    public void MicropolisDecoderExtractsPayloadAndChecksum()
    {
        const byte cylinder = 34;
        const byte sector = 7;
        var payload = Enumerable.Range(0, 256).Select(index => (byte)(255 - index)).ToArray();
        var record = new List<byte> { 0xff, cylinder, sector };
        record.AddRange(new byte[10]);
        record.AddRange(payload);
        record.Add(MicropolisChecksum(record.Skip(1).ToArray()));
        record.AddRange(new byte[5]);
        var bits = EncodeMfm(new byte[40]) + EncodeMfm(record.ToArray());

        var result = new MicropolisMfmDecoder().Decode(Revolution(bits));

        var decoded = Assert.Single(result.Sectors!);
        Assert.Equal(cylinder, decoded.Cylinder);
        Assert.Equal(sector, decoded.Number);
        Assert.True(decoded.IntegrityValid);
        Assert.Equal(payload, result.DecodedBytes);
    }

    [Fact]
    public void RegistryContainsEveryGreaseweazleCodecFamily()
    {
        var ids = new FluxDecoderRegistry().Decoders.Select(decoder => decoder.Id).ToHashSet();
        Assert.Contains("hp.mmfm", ids);
        Assert.Contains("datageneral.fm", ids);
        Assert.Contains("micropolis.mfm", ids);
    }

    private static FluxRevolution Revolution(string bits)
    {
        var intervals = new List<uint>();
        var cells = 0;
        foreach (var bit in bits)
        {
            cells++;
            if (bit != '1') continue;
            intervals.Add((uint)(cells * 40));
            cells = 0;
        }
        if (cells > 0) intervals.Add((uint)(cells * 40));
        return new(8_000_000, intervals);
    }

    private static string EncodeMfm(IReadOnlyList<byte> values)
    {
        var result = new System.Text.StringBuilder(values.Count * 16);
        var previous = false;
        foreach (var value in values)
        {
            for (var bit = 7; bit >= 0; bit--)
            {
                var current = (value & 1 << bit) != 0;
                result.Append(!previous && !current ? '1' : '0').Append(current ? '1' : '0');
                previous = current;
            }
        }
        return result.ToString();
    }

    private static string EncodeFm(IReadOnlyList<byte> values)
    {
        var result = new System.Text.StringBuilder(values.Count * 16);
        foreach (var value in values) for (var bit = 7; bit >= 0; bit--) result.Append('1').Append((value & 1 << bit) != 0 ? '1' : '0');
        return result.ToString();
    }

    private static string RawBytes(IReadOnlyList<byte> values) => string.Concat(values.Select(value => Convert.ToString(value, 2).PadLeft(8, '0')));

    private static byte[] AddCrc(IReadOnlyList<byte> values)
    {
        var result = values.ToList();
        var crc = Crc16(values);
        result.Add((byte)(crc >> 8));
        result.Add((byte)crc);
        return result.ToArray();
    }

    private static ushort Crc16(IEnumerable<byte> values)
    {
        ushort crc = 0xffff;
        foreach (var value in values)
        {
            crc ^= (ushort)(value << 8);
            for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
        }
        return crc;
    }

    private static ushort DataGeneralChecksum(ReadOnlySpan<byte> data)
    {
        ushort value = 0;
        for (var index = 0; index <= data.Length; index++)
        {
            var input = index < data.Length ? data[index] : (byte)0;
            value = (ushort)(((value & 0xff) ^ (value >> 8)) | (((value & 0xff) ^ input) << 8));
        }
        return value;
    }

    private static byte MicropolisChecksum(ReadOnlySpan<byte> data)
    {
        var value = 0;
        foreach (var item in data) { if (value > 255) value -= 255; value += item; }
        return (byte)value;
    }

    private static byte ReverseBits(byte value)
    {
        var result = 0;
        for (var bit = 0; bit < 8; bit++) result = result << 1 | value >> bit & 1;
        return (byte)result;
    }
}
