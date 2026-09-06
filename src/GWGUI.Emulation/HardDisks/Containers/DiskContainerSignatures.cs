using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Identifies known container markers, without claiming that their complete contents are valid.</summary>
public static class DiskContainerSignatures
{
    public static string? Identify(Stream image)
    {
        if (!image.CanRead || !image.CanSeek) throw new ArgumentException("A readable seekable image is required.");
        var position = image.Position;
        try
        {
            var head = new byte[(int)Math.Min(80, image.Length)]; image.Position = 0; image.ReadExactly(head);
            var footer = image.Length >= 512 ? new byte[512] : [];
            if (footer.Length != 0) { image.Position = image.Length - 512; image.ReadExactly(footer); }
            return Identify(head, footer);
        }
        finally { image.Position = position; }
    }

    internal static string? Identify(ReadOnlySpan<byte> head, ReadOnlySpan<byte> footer)
    {
        if (head.StartsWith("vhdxfile"u8)) return "vhdx";
        if (head.StartsWith("sprs"u8)) return "sparseimage";
        if (head.StartsWith("conectix"u8) || footer.StartsWith("conectix"u8)) return "vhd";
        if (head.Length >= 72 && BinaryPrimitives.ReadUInt32LittleEndian(head[64..]) == 0xbeda107f) return "vdi";
        if (head.StartsWith("KDMV"u8) || head.StartsWith("# Disk DescriptorFile"u8)) return "vmdk";
        if (head.StartsWith(new byte[] { 0x51, 0x46, 0x49, 0xfb })) return "qcow";
        if (head.StartsWith(new byte[] { 0x51, 0x45, 0x44, 0 })) return "qed";
        if (head.StartsWith("MComprHD"u8)) return "chd";
        if (head.StartsWith("2IMG"u8)) return "twoimg";
        if (head.StartsWith("WithouFreSpacExt"u8) || head.StartsWith("WithoutFreeSpace"u8)) return "parallels";
        if (head.StartsWith("Bochs Virtual HD Image"u8)) return "bochs";
        if (head.StartsWith("#!/bin/sh\n#V2.0 Format\n"u8)) return "cloop";
        if (head.StartsWith("T98HDDIMAGE.R0"u8)) return "nhd";
        if (footer.StartsWith("koly"u8)) return "udif";
        if (head.StartsWith(new byte[] { 0x1f, 0x8b, 0x08 })) return "gzip";
        return null;
    }
}
