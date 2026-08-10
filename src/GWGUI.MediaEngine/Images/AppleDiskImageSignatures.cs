using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Images;

internal static class AppleDiskImageSignatures
{
    public static bool LooksLikeDos33(ReadOnlySpan<byte> data)
    {
        if (data.Length != 143_360) return false;
        var vtoc = data.Slice(17 * 16 * 256, 256);
        return vtoc[1] is > 0 and < 35 && vtoc[2] < 16 && vtoc[0x35] is >= 13 and <= 16 && vtoc[0x36] == 0;
    }

    public static bool LooksLikeProDos(ReadOnlySpan<byte> data)
    {
        if (data.Length < 3 * 512) return false;
        var block = data.Slice(2 * 512, 512);
        var storage = block[4] >> 4;
        var nameLength = block[4] & 0x0f;
        return storage == 0x0f && nameLength is > 0 and <= 15 && block[0x23] == 0x27;
    }

    public static bool LooksLikeMac(ReadOnlySpan<byte> data)
    {
        if (data.Length < 1536) return false;
        var signature = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(1024, 2));
        return signature is 0xd2d7 or 0x4244;
    }

    public static bool LooksLikeLisaOfficePayload(ReadOnlySpan<byte> data)
    {
        if (data.Length != 409_600) return false;
        for (var offset = 0; offset + 64 <= data.Length; offset += 512)
        {
            var version = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(offset, 2));
            if (version is not (0x000e or 0x000f or 0x0011)) continue;
            var nameLength = data[offset + 12];
            if (nameLength is 0 or > 31 || offset + 13 + nameLength > data.Length) continue;
            var name = data.Slice(offset + 13, nameLength);
            var printable = true;
            foreach (var value in name)
                if (value is < 0x20 or > 0x7e) { printable = false; break; }
            if (printable) return true;
        }
        return false;
    }

    public static bool LooksLikeSos(ReadOnlySpan<byte> data)
    {
        if (data.Length != 143_360) return false;
        var boot = System.Text.Encoding.ASCII.GetString(data[..Math.Min(128, data.Length)]);
        return boot.Contains("SOS", StringComparison.OrdinalIgnoreCase);
    }
}
