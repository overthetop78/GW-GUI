using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

internal static class ClassicHfsBTreeWriter
{
    internal static byte[] Create(int size, string? label)
    {
        var tree = new byte[size];
        var catalog = label is not null;
        var used = catalog ? 2 : 1;
        tree[8] = 1; U16(tree,10,3);
        if (catalog)
        {
            U16(tree,14,1); U32(tree,16,1); U32(tree,20,2); U32(tree,24,1); U32(tree,28,1);
        }
        U16(tree,32,512); U16(tree,34,catalog?37:7);
        U32(tree,36,(uint)(size/512)); U32(tree,40,(uint)(size/512-used));
        tree[248]=catalog?(byte)0xc0:(byte)0x80;
        U16(tree,510,14); U16(tree,508,120); U16(tree,506,248); U16(tree,504,504);
        if (!catalog) return tree;
        var leaf=tree.AsSpan(512,512); leaf[8]=0xff; leaf[9]=1; U16(leaf,10,2);
        var folder=new byte[70]; folder[0]=1; U32(folder,6,2);
        var thread=new byte[46]; thread[0]=3; U32(thread,10,1);
        var name=ClassicMacLabel.Encode(label!);
        thread[14]=(byte)name.Length; name.CopyTo(thread,15);
        var first=Key(1,name); var second=Key(2,[]); var cursor=14;
        U16(leaf,510,cursor); first.CopyTo(leaf[cursor..]); cursor+=first.Length;
        folder.CopyTo(leaf[cursor..]); cursor+=folder.Length;
        U16(leaf,508,cursor); second.CopyTo(leaf[cursor..]); cursor+=second.Length;
        thread.CopyTo(leaf[cursor..]); cursor+=thread.Length; U16(leaf,506,cursor);
        return tree;
    }
    private static byte[] Key(uint parent,byte[] name)
    {
        var key=new byte[(7+name.Length+1)&~1]; key[0]=(byte)(6+name.Length);
        U32(key,2,parent); key[6]=(byte)name.Length; name.CopyTo(key,7); return key;
    }
    private static void U16(Span<byte> bytes,int offset,int value)=>BinaryPrimitives.WriteUInt16BigEndian(bytes[offset..],checked((ushort)value));
    private static void U32(Span<byte> bytes,int offset,uint value)=>BinaryPrimitives.WriteUInt32BigEndian(bytes[offset..],value);
}
