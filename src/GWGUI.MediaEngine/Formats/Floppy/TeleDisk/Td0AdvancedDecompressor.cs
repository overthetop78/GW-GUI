namespace GWGUI.MediaEngine.Formats.Floppy.TeleDisk;

/// <summary>Décompresse le flux LZSS/Huffman adaptatif des images TeleDisk 2.x avancées.</summary>
internal static class Td0AdvancedDecompressor
{
    private const int WindowSize = 4096;
    private const int LookAhead = 60;
    private const int Threshold = 2;
    private const int SymbolCount = 256 + LookAhead - Threshold;
    private const int MaximumOutputSize = 3_000_000;

    public static byte[] Decompress(ReadOnlySpan<byte> compressed)
    {
        var reader = new BitReader(compressed.ToArray());
        var tree = new AdaptiveTree(SymbolCount);
        var dictionary = new byte[WindowSize];
        Array.Fill(dictionary, (byte)' ', 0, WindowSize - LookAhead);
        var position = WindowSize - LookAhead;
        var output = new List<byte>(Math.Min(MaximumOutputSize, compressed.Length * 4));
        while (output.Count < MaximumOutputSize)
        {
            int symbol;
            try { symbol = tree.DecodeCharacter(reader); }
            catch (EndOfStreamException) { break; }
            if (symbol < 256) Emit((byte)symbol);
            else
            {
                int distance;
                try { distance = tree.DecodePosition(reader); }
                catch (EndOfStreamException) { break; }
                var source = (position - distance - 1) & (WindowSize - 1);
                var length = symbol + Threshold - 255;
                for (var index = 0; index < length && output.Count < MaximumOutputSize; index++)
                    Emit(dictionary[(source + index) & (WindowSize - 1)]);
            }
        }
        if (output.Count == MaximumOutputSize) throw new InvalidDataException("Le flux TeleDisk avancé dépasse la taille décompressée maximale.");
        return output.ToArray();

        void Emit(byte value)
        {
            output.Add(value);
            dictionary[position] = value;
            position = (position + 1) & (WindowSize - 1);
        }
    }

    private sealed class AdaptiveTree
    {
        private const int MaximumFrequency = 0x8000;
        private readonly int symbolCount;
        private readonly int nodeCount;
        private readonly int root;
        private readonly int[] frequency;
        private readonly int[] parent;
        private readonly int[] son;
        private readonly int[] symbolMap;
        private static readonly byte[] PositionLength = BuildPositionTable(true);
        private static readonly byte[] PositionCode = BuildPositionTable(false);

        public AdaptiveTree(int symbols)
        {
            symbolCount = symbols;
            nodeCount = 2 * symbols - 1;
            root = nodeCount - 1;
            frequency = new int[nodeCount + 1];
            parent = new int[nodeCount];
            son = new int[nodeCount];
            symbolMap = new int[symbols];
            for (var index = 0; index < symbols; index++)
            {
                frequency[index] = 1;
                son[index] = index + nodeCount;
                symbolMap[index] = index;
            }
            for (int child = 0, branch = symbols; branch <= root; child += 2, branch++)
            {
                frequency[branch] = frequency[child] + frequency[child + 1];
                son[branch] = child;
                parent[child] = parent[child + 1] = branch;
            }
            frequency[nodeCount] = ushort.MaxValue;
        }

        public int DecodeCharacter(BitReader reader)
        {
            var node = son[root];
            while (node < nodeCount) node = son[node + reader.ReadBit()];
            var symbol = node - nodeCount;
            Update(symbol);
            return symbol;
        }

        public int DecodePosition(BitReader reader)
        {
            var bits = reader.ReadByte();
            var position = PositionCode[bits] << 6;
            var remaining = PositionLength[bits] - 2;
            while (remaining-- > 0) bits = (byte)((bits << 1) | reader.ReadBit());
            return position | bits & 0x3f;
        }

        private void Update(int symbol)
        {
            if (frequency[root] == MaximumFrequency) Rebuild();
            var node = symbolMap[symbol];
            while (true)
            {
                var value = ++frequency[node];
                var ordered = node + 1;
                if (value > frequency[ordered])
                {
                    while (value > frequency[ordered]) ordered++;
                    ordered--;
                    frequency[node] = frequency[ordered];
                    frequency[ordered] = value;
                    var firstSon = son[node];
                    Connect(firstSon, ordered);
                    var secondSon = son[ordered];
                    son[ordered] = firstSon;
                    Connect(secondSon, node);
                    son[node] = secondSon;
                    node = ordered;
                }
                node = parent[node];
                if (node == 0) break;
            }
        }

        private void Connect(int child, int branch)
        {
            if (child < nodeCount) parent[child] = parent[child + 1] = branch;
            else symbolMap[child - nodeCount] = branch;
        }

        private void Rebuild()
        {
            var packed = 0;
            for (var index = 0; index < nodeCount; index++)
            {
                if (son[index] < nodeCount) continue;
                frequency[packed] = (frequency[index] + 1) / 2;
                son[packed++] = son[index];
            }
            for (int child = 0, branch = symbolCount; branch < nodeCount; child += 2, branch++)
            {
                var value = frequency[child] + frequency[child + 1];
                var insertion = branch - 1;
                while (value < frequency[insertion]) insertion--;
                insertion++;
                var length = (branch - insertion) * 2;
                if (length > 0)
                {
                    Array.Copy(frequency, insertion, frequency, insertion + 1, length);
                    Array.Copy(son, insertion, son, insertion + 1, length);
                }
                frequency[insertion] = value;
                son[insertion] = child;
            }
            for (var branch = 0; branch < nodeCount; branch++)
            {
                var child = son[branch];
                if (child >= nodeCount) symbolMap[child - nodeCount] = branch;
                else parent[child] = parent[child + 1] = branch;
            }
        }

        private static byte[] BuildPositionTable(bool lengths)
        {
            ReadOnlySpan<byte> codeLengths = [3,4,4,4,5,5,5,5,5,5,5,5,6,6,6,6,6,6,6,6,6,6,6,6,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8];
            ReadOnlySpan<byte> codes = [0x00,0x20,0x30,0x40,0x50,0x58,0x60,0x68,0x70,0x78,0x80,0x88,0x90,0x94,0x98,0x9c,0xa0,0xa4,0xa8,0xac,0xb0,0xb4,0xb8,0xbc,0xc0,0xc2,0xc4,0xc6,0xc8,0xca,0xcc,0xce,0xd0,0xd2,0xd4,0xd6,0xd8,0xda,0xdc,0xde,0xe0,0xe2,0xe4,0xe6,0xe8,0xea,0xec,0xee,0xf0,0xf1,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf8,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
            var table = new byte[256];
            for (var value = 0; value < codeLengths.Length; value++)
            {
                var count = 1 << (8 - codeLengths[value]);
                for (var suffix = 0; suffix < count; suffix++) table[codes[value] + suffix] = lengths ? codeLengths[value] : (byte)value;
            }
            return table;
        }
    }

    private sealed class BitReader(byte[] data)
    {
        private int bitOffset;
        public int ReadBit()
        {
            if (bitOffset >= data.Length * 8) throw new EndOfStreamException();
            var value = data[bitOffset / 8] >> (7 - bitOffset % 8) & 1;
            bitOffset++;
            return value;
        }
        public byte ReadByte()
        {
            var value = 0;
            for (var index = 0; index < 8; index++) value = value << 1 | ReadBit();
            return (byte)value;
        }
    }
}
