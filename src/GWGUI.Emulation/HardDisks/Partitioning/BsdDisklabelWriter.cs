using System.Buffers.Binary;
using System.Globalization;

namespace GWGUI.Emulation.HardDisks.Partitioning;

/// <summary>32-bit BSD disklabel, eight slots, raw disk in slot c, label at sector one.</summary>
public static class BsdDisklabelWriter
{
    public static void Validate(long capacity,IReadOnlyList<DiskVolumePlan> volumes)
    {
        if(capacity<8192 || capacity%512!=0 || capacity/512>uint.MaxValue || volumes.Count>7)
            throw new ArgumentException("Invalid BSD disklabel capacity or partition count.");
        long end=8192;
        foreach(var volume in volumes.OrderBy(v=>v.OffsetBytes))
        {
            if(volume.OffsetBytes<end || volume.LengthBytes<=0 || volume.OffsetBytes%512!=0 || volume.LengthBytes%512!=0 ||
                volume.OffsetBytes>capacity || volume.LengthBytes>capacity-volume.OffsetBytes || volume.MbrLogical || volume.AhdiLogical)
                throw new ArgumentException("BSD data partitions must leave the boot area reserved and not overlap.");
            _=Type(volume); end=checked(volume.OffsetBytes+volume.LengthBytes);
        }
    }
    public static void Write(Stream disk,IReadOnlyList<DiskVolumePlan> volumes)=>Write(disk,volumes,false);
    public static void Write(Stream disk,IReadOnlyList<DiskVolumePlan> volumes,bool bigEndian)
    {
        Validate(disk.Length,volumes);
        var label=new byte[512];
        void Word(int offset,ushort value){if(bigEndian)BinaryPrimitives.WriteUInt16BigEndian(label.AsSpan(offset),value);else BinaryPrimitives.WriteUInt16LittleEndian(label.AsSpan(offset),value);}
        void Long(int offset,uint value){if(bigEndian)BinaryPrimitives.WriteUInt32BigEndian(label.AsSpan(offset),value);else BinaryPrimitives.WriteUInt32LittleEndian(label.AsSpan(offset),value);}
        Long(0,0x82564557); "Virtual disk"u8.CopyTo(label.AsSpan(8));
        Long(40,512); Long(44,1); Long(48,1); Long(52,(uint)(disk.Length/512)); Long(56,1); Long(60,(uint)(disk.Length/512));
        Word(74,1); Long(132,0x82564557); Word(138,8); Long(140,8192); Long(144,8192);
        Long(148+2*16,(uint)(disk.Length/512)); // Slot c describes the whole disk, not a formatted volume.
        for(var i=0;i<volumes.Count;i++)
        {
            var slot=i<2?i:i+1; var at=148+slot*16; var volume=volumes[i];
            Long(at,checked((uint)(volume.LengthBytes/512))); Long(at+4,checked((uint)(volume.OffsetBytes/512)));
            label[at+12]=Type(volume);
        }
        ushort checksum=0;
        for(var i=0;i<148+8*16;i+=2)checksum^=bigEndian?BinaryPrimitives.ReadUInt16BigEndian(label.AsSpan(i)):BinaryPrimitives.ReadUInt16LittleEndian(label.AsSpan(i));
        Word(136,checksum); disk.Position=512;disk.Write(label);disk.Flush();
    }
    private static byte Type(DiskVolumePlan volume)
    {
        if(volume.PartitionType is {} type && byte.TryParse(type,NumberStyles.None,CultureInfo.InvariantCulture,out var value))return value;
        if(volume.PartitionType is null && volume.FileSystemId.Equals("none",StringComparison.OrdinalIgnoreCase))return 0;
        throw new ArgumentException("Provide the BSD filesystem type as a decimal PartitionType byte.");
    }
}
