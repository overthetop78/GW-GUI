using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Vmdk;

/// <summary>Declares the monolithic VMDK layouts supported by MediaEngine.</summary>
internal static class VmdkFormat
{
    public static readonly ReadOnlyMemory<byte> SparseSignature = CreateSparseSignature();
    public static readonly ReadOnlyMemory<byte> DescriptorSignature = System.Text.Encoding.ASCII.GetBytes("# Disk DescriptorFile");
    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Vmdk }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static readonly IReadOnlySet<string> AssociatedExtensions =
        new[] { DiskImageFileExtensions.Vmdk }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static readonly IReadOnlySet<HardDiskImageVariant> Variants = new[]
    {
        HardDiskImageVariant.Fixed,
        HardDiskImageVariant.Sparse
    }.ToFrozenSet();

    public static string FormatId => HardDiskImageFormatIds.Vmdk;
    public static bool CanRead => true;
    public static bool CanWrite => true;

    private static byte[] CreateSparseSignature()
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, HardDiskFormatConstants.VmdkSparseMagic);
        return bytes;
    }
}
