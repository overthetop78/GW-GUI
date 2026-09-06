using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

internal static class HfsBTreeWriter
{
    private const int Node = 4096;
    internal static byte[] EmptyExtents() => Header(catalog: false, caseSensitive: false);
    internal static byte[] EmptyAttributes()
    {
        var header = Header(catalog: false, caseSensitive: false);
        U16(header, 14 + 20, 266); U32(header, 14 + 38, 6);
        return header;
    }
    internal static byte[] EmptyCatalog(string label, bool caseSensitive)
    {
        var tree = new byte[2 * Node]; Header(catalog: true, caseSensitive).CopyTo(tree, 0);
        var leaf = tree.AsSpan(Node); leaf[8] = 0xff; leaf[9] = 1; U16(leaf, 10, 2);
        var folder = new byte[88]; U16(folder, 0, 1); U32(folder, 8, 2); U16(folder, 42, 0x41ed);
        var thread = new byte[10 + label.Length * 2]; U16(thread, 0, 3); U32(thread, 4, 1);
        U16(thread, 8, label.Length); Encoding.BigEndianUnicode.GetBytes(label).CopyTo(thread, 10);
        var firstKey = Key(1, label); var secondKey = Key(2, "");
        var cursor = 14;
        U16(leaf, Node - 2, cursor); firstKey.CopyTo(leaf[cursor..]); cursor += firstKey.Length;
        folder.CopyTo(leaf[cursor..]); cursor += folder.Length;
        U16(leaf, Node - 4, cursor); secondKey.CopyTo(leaf[cursor..]); cursor += secondKey.Length;
        thread.CopyTo(leaf[cursor..]); cursor += thread.Length;
        U16(leaf, Node - 6, cursor);
        return tree;
    }
    private static byte[] Header(bool catalog, bool caseSensitive)
    {
        var node = new byte[Node]; node[8] = 1; U16(node, 10, 3);
        var header = node.AsSpan(14, 106);
        if (catalog)
        {
            U16(header, 0, 1); U32(header, 2, 1); U32(header, 6, 2); U32(header, 10, 1); U32(header, 14, 1);
            header[37] = caseSensitive ? (byte)0xbc : (byte)0xcf;
        }
        U16(header, 18, Node); U16(header, 20, catalog ? 516 : 10);
        U32(header, 22, catalog ? 2u : 1u); U32(header, 32, Node); U32(header, 38, catalog ? 6u : 2u);
        node[248] = catalog ? (byte)0xc0 : (byte)0x80;
        U16(node, Node - 2, 14); U16(node, Node - 4, 120); U16(node, Node - 6, 248); U16(node, Node - 8, Node - 8);
        return node;
    }
    private static byte[] Key(uint parent, string name)
    {
        var key = new byte[8 + name.Length * 2]; U16(key, 0, key.Length - 2); U32(key, 2, parent);
        U16(key, 6, name.Length); Encoding.BigEndianUnicode.GetBytes(name).CopyTo(key, 8); return key;
    }
    private static void U16(Span<byte> bytes, int offset, int value) => BinaryPrimitives.WriteUInt16BigEndian(bytes[offset..], checked((ushort)value));
    private static void U32(Span<byte> bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes[offset..], value);
}
