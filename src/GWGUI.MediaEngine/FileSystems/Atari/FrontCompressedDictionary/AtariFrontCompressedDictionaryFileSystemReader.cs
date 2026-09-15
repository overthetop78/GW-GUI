using System.Collections.Frozen;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Atari.FrontCompressedDictionary;

/// <summary>Recognizes Atari dictionary disks indexed by initial letter and compressed by shared prefixes.</summary>
public sealed class AtariFrontCompressedDictionaryFileSystemReader : IFileSystemReader
{
    private const int SectorCount = 720;
    private const int SectorSize = 128;
    private const int AlphabetEntryCount = 26;
    private const int AlphabetEntrySize = 3;
    private const int MarkerOffset = 79;
    private const int DataStartOffset = 81;
    private const int MaximumWordLength = 255;

    public string Id => FileSystemIds.AtariFrontCompressedDictionary;

    public IReadOnlySet<string> CatalogFormatIds { get; } = new[]
    {
        DiskImageFormatIds.Atari90,
        DiskImageFormatIds.AtariXfd90
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public bool CanRead(SectorImage image) =>
        TryReadImage(image, out var content) && TryDecode(content, out _);

    public FileSystemVolume Read(SectorImage image)
    {
        if (!TryReadImage(image, out var content) || !TryDecode(content, out var dictionary))
            throw new InvalidDataException("The sector image is not an Atari front-compressed dictionary disk.");

        var decodedContent = System.Text.Encoding.ASCII.GetBytes(string.Join("\r\n", dictionary.Words) + "\r\n");
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["container"] = FileSystemIds.AtariFrontCompressedDictionary,
            ["sectorCount"] = SectorCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["sectorSize"] = SectorSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["alphabetIndexEntries"] = AlphabetEntryCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["wordCount"] = dictionary.Words.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["prefixBits"] = "4",
            ["characterBits"] = "5",
            ["bitOrder"] = "most-significant-bit-first",
            ["decodedEncoding"] = "US-ASCII"
        };
        var entry = new FileSystemEntry(
            "DICTIONARY.TXT",
            FileSystemEntryKind.File,
            decodedContent.Length,
            null,
            string.Empty,
            0,
            1,
            true,
            [],
            decodedContent,
            nativeTypeId: "atari-front-compressed-word-list",
            occupiedSize: dictionary.OccupiedBytes,
            attributes: ["compressed", "dictionary", "decoded"],
            dataValid: true,
            syntheticName: true,
            metadata: metadata);

        return new FileSystemVolume(
            FileSystemDisplayNames.AtariFrontCompressedDictionary,
            FileSystemIds.AtariFrontCompressedDictionary,
            image.Capacity,
            0,
            null,
            null,
            [entry],
            [],
            freeSpaceKnown: false,
            attributes: ["dictionary", "alphabet-indexed", "front-compressed"],
            bootable: false);
    }

    private bool TryReadImage(SectorImage image, out byte[] content)
    {
        content = [];
        if (!CatalogFormatIds.Contains(image.FormatId)
            || image.BlockCount != SectorCount
            || image.BlockSize != SectorSize)
            return false;

        content = new byte[SectorCount * SectorSize];
        for (var logicalBlock = 0; logicalBlock < SectorCount; logicalBlock++)
        {
            if (!image.TryGetBlock(logicalBlock, out var block) || block.Data.Count != SectorSize)
                return false;
            block.Data.ToArray().CopyTo(content, logicalBlock * SectorSize);
        }

        return true;
    }

    private static bool TryDecode(IReadOnlyList<byte> content, out DecodedDictionary dictionary)
    {
        dictionary = new([], 0);
        if (content.Count != SectorCount * SectorSize
            || content[MarkerOffset] != (byte)'S'
            || content[MarkerOffset + 1] != (byte)'M')
            return false;

        var groupPositions = new int[AlphabetEntryCount];
        var previousPosition = -1;
        for (var letter = 0; letter < AlphabetEntryCount; letter++)
        {
            var indexOffset = letter * AlphabetEntrySize;
            var sector = content[indexOffset] | content[indexOffset + 1] << 8;
            var byteOffset = content[indexOffset + 2];
            if (sector < 1 || sector > SectorCount || byteOffset >= SectorSize)
                return false;

            var position = (sector - 1) * SectorSize + byteOffset;
            if (position < DataStartOffset || position <= previousPosition)
                return false;
            groupPositions[letter] = position;
            previousPosition = position;
        }

        var words = new List<string>();
        var lastDataPosition = DataStartOffset;
        for (var letter = 0; letter < AlphabetEntryCount; letter++)
        {
            var reader = new BitReader(content, groupPositions[letter]);
            var wordBuffer = new char[MaximumWordLength];
            string? previousWord = null;
            var groupWordCount = 0;
            while (true)
            {
                if (!reader.TryRead(4, out var sharedPrefix))
                    return false;
                if (sharedPrefix == 15)
                    break;

                var wordLength = sharedPrefix;
                while (true)
                {
                    if (!reader.TryRead(5, out var symbol))
                        return false;
                    if (symbol == 30)
                        break;
                    if (wordLength >= wordBuffer.Length || !TryDecodeSymbol(symbol, out wordBuffer[wordLength]))
                        return false;
                    wordLength++;
                }

                if (wordLength == 0)
                    return false;
                var word = new string(wordBuffer, 0, wordLength);
                if (word[0] != 'A' + letter
                    || previousWord is not null && string.CompareOrdinal(previousWord, word) > 0)
                    return false;
                words.Add(word);
                previousWord = word;
                groupWordCount++;
            }

            if (groupWordCount == 0
                || letter + 1 < AlphabetEntryCount && reader.BytePosition > groupPositions[letter + 1])
                return false;
            lastDataPosition = Math.Max(lastDataPosition, reader.BytePosition + 1);
        }

        if (words.Count == 0)
            return false;
        dictionary = new(words.AsReadOnly(), lastDataPosition);
        return true;
    }

    private static bool TryDecodeSymbol(int symbol, out char value)
    {
        value = symbol switch
        {
            >= 1 and <= 26 => (char)('A' + symbol - 1),
            28 => '\'',
            29 => '-',
            _ => '\0'
        };
        return value != '\0';
    }

    private sealed record DecodedDictionary(IReadOnlyList<string> Words, int OccupiedBytes);

    private sealed class BitReader(IReadOnlyList<byte> content, int bytePosition)
    {
        private int _mask = 0x80;

        public int BytePosition { get; private set; } = bytePosition;

        public bool TryRead(int bitCount, out int value)
        {
            value = 0;
            for (var bit = 0; bit < bitCount; bit++)
            {
                if (BytePosition >= content.Count)
                    return false;
                value <<= 1;
                if ((content[BytePosition] & _mask) != 0)
                    value |= 1;
                _mask >>= 1;
                if (_mask != 0)
                    continue;
                _mask = 0x80;
                BytePosition++;
            }

            return true;
        }
    }
}
