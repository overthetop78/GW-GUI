using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Arburg.</summary>
public sealed class ArburgTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => ArburgFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => ArburgFormat.CodecDisplayName;
    /// <summary>Encode les blocs système ou de données d'une piste Arburg.</summary>
    /// <param name="request">Piste logique et attribut indiquant la nature système de chaque secteur.</param>
    /// <returns>Cellules binaires de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas une taille Arburg admise.</exception>
    /// <remarks>Chaque bloc est complété avec son contrôle avant l'encodage des cellules.</remarks>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            var definition = ArburgFormat.Definition(Attribute(sector, ArburgFormat.SystemAttribute, 0) != 0);
            var block = BuildBlock(sector.Data, definition);
            if (definition.Kind == ArburgFormat.BlockKind.System) WriteSystemBlock(bits, block, definition.Mark);
            else WriteDataBlock(bits, block, definition.Mark);
            bits.Gap(ArburgFormat.GapBitCount, true);
        }
        return bits;
    }

    /// <summary>Reconstruit toujours le checksum et le remplissage depuis les octets utiles reçus.</summary>
    private static byte[] BuildBlock(IReadOnlyList<byte> source, ArburgFormat.BlockDefinition definition)
    {
        if (source.Count != definition.UsefulSize && source.Count != definition.TotalSize) throw ArburgFormat.InvalidPayloadSize(definition, source.Count);
        return ArburgChecksum.CreateBlock(source.Take(definition.UsefulSize).ToArray(), definition.TotalSize);
    }

    /// <summary>Écrit la marque puis le codage variable d'un bloc système.</summary>
    private static void WriteSystemBlock(List<bool> bits, IReadOnlyList<byte> block, IReadOnlyList<byte> mark)
    {
        bits.Raw(mark.ToArray());
        bits.AddRange(ArburgSystemCodec.Encode(block));
    }

    /// <summary>Écrit la marque puis le double FM d'un bloc de données aux bits inversés.</summary>
    private static void WriteDataBlock(List<bool> bits, IReadOnlyList<byte> block, IReadOnlyList<byte> mark)
    {
        bits.Raw(mark.ToArray());
        bits.DoubleFm(block.Select(Primitives.BitPrimitives.ReverseBits));
    }
}
