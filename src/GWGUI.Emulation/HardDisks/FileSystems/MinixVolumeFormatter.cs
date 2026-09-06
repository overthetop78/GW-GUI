using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.FileSystems;

public enum MinixStorageOrder { LittleEndian, BigEndian16, BigEndian32, BigEndian64 }

/// <summary>Empty Minix v1/v2/v3 volumes with explicit byte/bitmap order and 1 KiB blocks and zones.</summary>
public static class MinixVolumeFormatter
{
    public static void Validate(long capacity,int version,int inodes=256,
        MinixStorageOrder order=MinixStorageOrder.LittleEndian,int nameLength=0)
    {
        if(version is <1 or >3 || inodes is <16 or >65535)
            throw new ArgumentOutOfRangeException(nameof(version));
        if (!Enum.IsDefined(order)) throw new ArgumentOutOfRangeException(nameof(order));
        if (nameLength != 0 && (version == 3 ? nameLength != 60 : nameLength is not (14 or 30)))
            throw new ArgumentOutOfRangeException(nameof(nameLength));
        if(capacity<64L<<10 || capacity%1024!=0 || capacity>(version==1?65535L*1024:256L<<30))
            throw new ArgumentOutOfRangeException(nameof(capacity));
        var layout=Layout(capacity,version,inodes);
        if(layout.First>65535 || layout.First>=capacity/1024)
            throw new ArgumentException("The metadata leaves no addressable data zone.");
    }

    public static void Format(Stream volume,int version,int inodes=256,
        MinixStorageOrder order=MinixStorageOrder.LittleEndian,int nameLength=0)
    {
        Validate(volume.Length,version,inodes,order,nameLength);
        nameLength = nameLength == 0 ? (version == 3 ? 60 : 30) : nameLength;
        var bigEndian = order != MinixStorageOrder.LittleEndian;
        var bitmapWordBytes = order switch { MinixStorageOrder.BigEndian16 => 2, MinixStorageOrder.BigEndian32 => 4,
            MinixStorageOrder.BigEndian64 => 8, _ => 1 };
        void U16(byte[] bytes, int offset, int value)
        {
            if (bigEndian) BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(offset), checked((ushort)value));
            else BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), checked((ushort)value));
        }
        void U32(byte[] bytes, int offset, uint value)
        {
            if (bigEndian) BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset), value);
            else BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);
        }
        var (imapBlocks,zmapBlocks,inodeBlocks,first)=Layout(volume.Length,version,inodes);
        var zones=checked((uint)(volume.Length/1024));
        var super=new byte[1024];
        if(version==3)
        {
            U32(super,0,(uint)inodes); U16(super,6,imapBlocks); U16(super,8,zmapBlocks);
            U16(super,10,first); U32(super,16,0x7fffffff); U32(super,20,zones);
            U16(super,24,0x4d5a); U16(super,28,1024);
        }
        else
        {
            U16(super,0,inodes); if(version==1)U16(super,2,(int)zones);
            U16(super,4,imapBlocks); U16(super,6,zmapBlocks); U16(super,8,first);
            U32(super,12,version==1?(7u+512+512*512)*1024:0x7fffffffu);
            U16(super,16,version==1?(nameLength==14?0x137f:0x138f):(nameLength==14?0x2468:0x2478)); U16(super,18,1);
            if(version==2) U32(super,20,zones);
        }
        var inodeMap=Map(imapBlocks,inodes+1,2,bitmapWordBytes);
        var zoneMap=Map(zmapBlocks,checked((int)(zones-first+1)),2,bitmapWordBytes);
        var inodeTable=new byte[inodeBlocks*1024]; U16(inodeTable,0,0x41ed);
        var entrySize=nameLength+(version==3?4:2);
        if(version==1)
        {
            U32(inodeTable,4,(uint)(2*entrySize)); inodeTable[13]=2; U16(inodeTable,14,first);
        }
        else
        {
            U16(inodeTable,2,2); U32(inodeTable,8,(uint)(2*entrySize)); U32(inodeTable,24,(uint)first);
        }
        var root=new byte[1024];
        if(version==3) { U32(root,0,1); U32(root,entrySize,1); root[4]=(byte)'.'; root[entrySize+4]=root[entrySize+5]=(byte)'.'; }
        else { U16(root,0,1); U16(root,entrySize,1); root[2]=(byte)'.'; root[entrySize+2]=root[entrySize+3]=(byte)'.'; }
        volume.Position=0; volume.Write(new byte[1024]); volume.Write(super); volume.Write(inodeMap); volume.Write(zoneMap); volume.Write(inodeTable);
        volume.Position=first*1024L; volume.Write(root); volume.Flush();
    }
    private static (int Imap,int Zmap,int Inodes,int First) Layout(long capacity,int version,int inodes)
    {
        var imap=(inodes+1+8191)/8192;
        var inodeBlocks=(inodes*(version==1?32:64)+1023)/1024;
        var zmap=checked((int)((capacity/1024+8191)/8192));
        return(imap,zmap,inodeBlocks,2+imap+zmap+inodeBlocks);
    }
    private static byte[] Map(int blocks,int validBits,int usedBits,int wordBytes)
    {
        var map=new byte[blocks*1024];
        for(var i=0;i<usedBits;i++) map[i/8]|=(byte)(1<<(i%8));
        for(var i=validBits;i<map.Length*8;i++) map[i/8]|=(byte)(1<<(i%8));
        if (wordBytes > 1)
            for (var offset = 0; offset < map.Length; offset += wordBytes) Array.Reverse(map, offset, wordBytes);
        return map;
    }
}
